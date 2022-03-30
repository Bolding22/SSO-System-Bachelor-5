using System.Reflection;
using Duende.IdentityServer;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using IdentityServerAspNetIdentity.ClaimHandling;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Validators;
using Serilog;
using ILogger = Serilog.ILogger;

namespace IdentityServerAspNetIdentity;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;
        var userConnectionString = builder.Configuration.GetConnectionString("UserConnection");
        Console.WriteLine(userConnectionString);
        var serverVersionUser = ServerVersion.AutoDetect(userConnectionString);
        var identityConnectionString = builder.Configuration.GetConnectionString("IdentityConnection");
        Console.WriteLine(identityConnectionString);
        var serverVersionIdentity = ServerVersion.AutoDetect(identityConnectionString);
        IdentityModelEventSource.ShowPII = true;

        builder.Services.AddRazorPages();
        //    .AddRazorPagesOptions(options => { 
        //        options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
        //});

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(userConnectionString, serverVersionUser, 
                optionsBuilder => optionsBuilder.EnableRetryOnFailure()));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                options.EmitStaticAudienceClaim = true;
            })

            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = b => b.UseMySql(identityConnectionString, serverVersionIdentity,
                    sql =>
                    {
                        sql.MigrationsAssembly(migrationsAssembly);
                        sql.EnableRetryOnFailure();
                    });
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = b =>
                {
                    b.UseMySql(identityConnectionString, serverVersionIdentity,
                        sql =>
                        {
                            sql.MigrationsAssembly(migrationsAssembly);
                            sql.EnableRetryOnFailure();
                        });
                };
            })
            .AddAspNetIdentity<ApplicationUser>()
            // Handles claims
            .AddProfileService<ProfileService>();
        
        builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformation>();
        
        builder.Services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                // register your IdentityServer with Google at https://console.developers.google.com
                // enable the Google+ API
                // set the redirect URI to https://localhost:5001/signin-google
                options.ClientId = "122550137758-5ri39h9qant940fd06uuko89bep3crk6.apps.googleusercontent.com";
                options.ClientSecret = "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";
            }).AddOpenIdConnect("AAD", "Azure AD Login", options =>
            {
                options.ClientId = "b295f200-59d5-49e3-958b-29c136ea3a6e";
                options.ClientSecret = "cy~7Q~uJcfswV6uc6wIKmsYtF4dJiCoVWWUdG";
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.Authority = "https://login.microsoftonline.com/common/v2.0/";
                options.TokenValidationParameters.IssuerValidator = AadIssuerValidator.GetAadIssuerValidator(options.Authority, options.Backchannel).Validate;
                options.ResponseType = "code";
                options.CallbackPath = "/signin-aad";
            });

        return builder.Build();
    }
    
    public static async Task<WebApplication> ConfigurePipeline(this WebApplication app)
    { 
        app.UseSerilogRequestLogging();
    
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        
        await InitializeIdentityDatabase(app);
        await MigrateUserDb(app);
        
        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();
        
        app.MapRazorPages()
            .RequireAuthorization();

        return app;
    }

    private static async Task MigrateUserDb(WebApplication app)
    {
        using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
        await context?.Database?.MigrateAsync();
    }

    private static async Task InitializeIdentityDatabase(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
        
        await serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.MigrateAsync();

        var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        await context.Database.MigrateAsync();

        var clients = context.Clients.Where(client => true);
        context.Clients.RemoveRange(clients);
        await context.SaveChangesAsync();
        
        if (!context.Clients.Any())
        {
            foreach (var client in Config.Clients)
            {
                context.Clients.Add(client.ToEntity());
            }
            await context.SaveChangesAsync();
        }
        
        var identityResources = context.IdentityResources.Where(resource => true);
        context.IdentityResources.RemoveRange(identityResources);
        await context.SaveChangesAsync();

        if (!context.IdentityResources.Any())
        {
            foreach (var resource in Config.IdentityResources)
            {
                context.IdentityResources.Add(resource.ToEntity());
            }
            await context.SaveChangesAsync();
        }

        var apiScopes = context.ApiScopes.Where(apiScope => true);
        context.ApiScopes.RemoveRange(apiScopes);
        await context.SaveChangesAsync();
        
        if (!context.ApiScopes.Any())
        {
            foreach (var resource in Config.ApiScopes)
            {
                context.ApiScopes.Add(resource.ToEntity());
            }
            await context.SaveChangesAsync();
        }
    }
}