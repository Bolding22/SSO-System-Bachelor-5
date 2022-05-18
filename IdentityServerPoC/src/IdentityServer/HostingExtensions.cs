using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using AspNetCoreRateLimit;
using AspNetCoreRateLimit.Redis;
using Duende.IdentityServer;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using IdentityServerAspNetIdentity.Services;
using IdentityServerAspNetIdentity.Services.ClaimHandling;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Validators;
using MySqlConnector;
using Serilog;
using StackExchange.Redis;
using ILogger = Serilog.ILogger;

namespace IdentityServerAspNetIdentity;

internal static class HostingExtensions
{
    private static bool _isContainerized;

    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        IdentityModelEventSource.ShowPII = true;

        builder.Services.AddRazorPages();

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
            //options.KnownNetworks.Clear();
            //options.KnownProxies.Clear();
        });

        builder.SetupUserDataStores();
        builder.SetupAjourIdentityServer();
        builder.SetupExternalIdentityProviders();

        _isContainerized = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        if (_isContainerized)
        {
            Log.Debug("Using Redis for distributed caching");
            var redisConnection = builder.Configuration.GetConnectionString("RedisCacheConnection");

            var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnection);
            builder.Services.AddSingleton<IConnectionMultiplexer>(_ => connectionMultiplexer);
            
            builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = redisConnection; });

            builder.Services.AddDataProtection()
                .PersistKeysToStackExchangeRedis(connectionMultiplexer);
            
            builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
            builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
            builder.Services.AddRedisRateLimiting();
            // inject counter and rules distributed cache stores
            builder.Services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
            builder.Services.AddSingleton<IRateLimitCounterStore,DistributedCacheRateLimitCounterStore>();
            builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        }
        else
        {
            // In development scenarios we don't want to run a cache server, so we just use in memory instead
            Log.Debug("Using local memory cache instead of server for distributed caching");
            builder.Services.AddDistributedMemoryCache();
        }

        return builder.Build();
    }

    private static void SetupExternalIdentityProviders(this WebApplicationBuilder builder)
    {
        // configures the OpenIdConnect handlers to persist the state parameter into the server-side IDistributedCache.
        // makes the external identity provider integration stateless so it works in horizontally scaled environment
        builder.Services.AddOidcStateDataFormatterCache();
        
        builder.Services.AddAuthentication()
            .AddGoogle("Google", "Sign in with Google", options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.ClientId = builder.Configuration["Google:ClientID"];
                options.ClientSecret = builder.Configuration["Google:ClientSecret"];
            })
            .AddOpenIdConnect("AAD", "Sign in with Azure AD / Microsoft", options =>
            {
                options.ClientId = "b295f200-59d5-49e3-958b-29c136ea3a6e";
                options.ClientSecret = "cy~7Q~uJcfswV6uc6wIKmsYtF4dJiCoVWWUdG";
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.Authority = "https://login.microsoftonline.com/common/v2.0/";
                options.TokenValidationParameters.IssuerValidator = AadIssuerValidator
                    .GetAadIssuerValidator(options.Authority, options.Backchannel).Validate;
                options.ResponseType = "code";
                options.CallbackPath = "/signin-aad";
            })
            .AddFacebook("Facebook", "Sign in with Facebook", options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.AppId = "697778028244682";
                options.AppSecret = "1ff283e349b09a161a9bbe7e6dc54f90";
            })
            .AddOpenIdConnect("Slack", "Sign in with Slack", options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.ClientId = "3341690290481.3352834550928";
                options.ClientSecret = "ed0d466b89409430e6785b1faa9502a7";
                options.ResponseType = "code";
                options.CallbackPath = "/signin-slack";
                options.Authority = "https://slack.com";
            });
    }

    private static void SetupUserDataStores(this WebApplicationBuilder builder)
    {
        var userConnectionString = builder.Configuration.GetConnectionString("UserConnection");
        var serverVersionUser = ServerVersion.AutoDetect(userConnectionString);
        
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(userConnectionString, serverVersionUser,
                optionsBuilder => optionsBuilder.EnableRetryOnFailure()));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
    }

    private static void SetupAjourIdentityServer(this WebApplicationBuilder builder)
    {
        var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;
        var identityConnectionString = builder.Configuration.GetConnectionString("IdentityConnection");
        var serverVersionIdentity = ServerVersion.AutoDetect(identityConnectionString);
        
        builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                //options.IssuerUri = builder.Configuration["issuerUri"];

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
            .AddProfileService<ProfileService>()
            // Handles redirects to wildcards
            .AddRedirectUriValidator<RedirectUriValidator>();
        
        builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformation>();
    }

    public static async Task<WebApplication> ConfigurePipeline(this WebApplication app)
    {
        app.UseForwardedHeaders();

        app.UseSerilogRequestLogging();
        
        if (_isContainerized)
            app.UseIpRateLimiting();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
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
        try
        {
            await context.Database.MigrateAsync();
        }
        catch (MySqlException e)
        {
            Log.Warning(e, "The migration failed, but is caught as it is likely due to multiple instances trying to migrate at once");
        }

        var clients = context.Clients.Where(client => true);
        context.Clients.RemoveRange(clients);

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