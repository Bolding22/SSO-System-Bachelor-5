using System.Reflection;
using Duende.IdentityServer;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityServerAspNetIdentity;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;
        var userConnectionString = builder.Configuration.GetConnectionString("UserConnection");
        var serverVersionUser = ServerVersion.AutoDetect(userConnectionString);
        var identityConnectionString = builder.Configuration.GetConnectionString("IdentityConnection");
        var serverVersionIdentity = ServerVersion.AutoDetect(identityConnectionString);

        builder.Services.AddRazorPages();
        //    .AddRazorPagesOptions(options => { 
        //        options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
        //});

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseMySql(userConnectionString, serverVersionUser, 
                optionsBuilder => optionsBuilder.EnableRetryOnFailure());
        });

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
                    sql => sql.MigrationsAssembly(migrationsAssembly));
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = b =>
                {
                    b.UseMySql(identityConnectionString, serverVersionIdentity,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                };
            })
            .AddAspNetIdentity<ApplicationUser>();
        
        builder.Services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                // register your IdentityServer with Google at https://console.developers.google.com
                // enable the Google+ API
                // set the redirect URI to https://localhost:5001/signin-google
                options.ClientId = "copy client ID from Google here";
                options.ClientSecret = "copy client secret from Google here";
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