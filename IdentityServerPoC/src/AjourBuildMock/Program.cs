using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Shared;
using WebClient.Authorization;
using WebClient.Persistence;
using WebClient.Persistence.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/", "UserExists")
        .AllowAnonymousToPage("/Index")
        .AllowAnonymousToPage("/Signout")
        .AllowAnonymousToPage("/Account/AccessDenied");
    options.Conventions.AuthorizePage("/Signout");
});

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

var authority = builder.Configuration["Authority"];
Console.WriteLine(authority);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "Cookies";
        options.DefaultChallengeScheme = "oidc";
    })
    .AddCookie("Cookies")
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = builder.Configuration["Authority"];

        options.MetadataAddress = $"{builder.Configuration["IssuerUri"]}/.well-known/openid-configuration";
        options.RequireHttpsMetadata = false;

        options.Events.OnRedirectToIdentityProvider = context =>
        {
            context.ProtocolMessage.IssuerAddress = $"{builder.Configuration["Authority"]}/connect/authorize";
            return Task.CompletedTask;
        };
        
        options.Events.OnRedirectToIdentityProviderForSignOut = context =>
        {
            context.ProtocolMessage.IssuerAddress = $"{builder.Configuration["Authority"]}/connect/endsession";
            return Task.CompletedTask;
        };

        options.ClientId = ClientIds.AjourServiceProvider;
        options.ClientSecret = "secret";
        options.ResponseType = "code";
        
        options.Scope.Add(IdentityResourceNames.UserAliases);
        options.ClaimActions.MapJsonKey(AjourClaims.UserAlias, AjourClaims.UserAlias);
        options.ClaimActions.MapJsonKey("email_verified", "email_verified");
        
        options.Scope.Add(ApiScopeNames.Api);
        options.Scope.Add("offline_access");

        options.GetClaimsFromUserInfoEndpoint = true;

        options.SaveTokens = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserExists", policy =>
    {
        var organizationId = Guid.Parse("68C450CF-A13F-4957-990D-E27E5DB8BD7B");
        policy.Requirements.Add(new UserExistsRequirement(organizationId));
    });
});

// Replace with your server version and type.
// Use 'MariaDbServerVersion' for MariaDB.
// Alternatively, use 'ServerVersion.AutoDetect(connectionString)'.
// For common usages, see pull request #1233.
var connectionString = builder.Configuration.GetConnectionString("ServiceProviderConnectionString");
var serverVersion = ServerVersion.AutoDetect(connectionString);
builder.Services.AddDbContext<ServiceProviderDbContext>(
    optionsBuilder =>
    {
        optionsBuilder.UseMySql(connectionString, serverVersion);
            // The following three options help with debugging, but should
            // be changed or removed for production.
            //.LogTo(Console.WriteLine, LogLevel.Information)
            //.EnableSensitiveDataLogging()
            //.EnableDetailedErrors();
    }
);

// Uses user alias to authorize users
builder.Services.AddSingleton<IAuthorizationHandler, UserExistsHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages().RequireAuthorization();

SeedData.EnsureSeedData(app);

app.Run();

public static class SeedData
{
    public static void EnsureSeedData(WebApplication app)
    {
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ServiceProviderDbContext>();
            
            dbContext.Database.Migrate();
            
            var systemUserIdAlice = Guid.Parse("7243B8E0-7FD3-40D4-A46C-802D68BC0F76");
            var aliceExists = dbContext.Users.Any(user => user.Id == systemUserIdAlice);

            if (!aliceExists)
            {
                dbContext.Users.Add(new User()
                {
                    Id = systemUserIdAlice,
                    FirstName = "Alice",
                    LastName = "Doe",
                    Email = "AliceSmith@email.com",
                    PhoneNumber = "12345678"
                });
                dbContext.SaveChanges();
            }
        }
    }
}
