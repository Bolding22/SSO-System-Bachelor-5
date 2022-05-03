using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Duende.IdentityServer;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using IdentityServerAspNetIdentity.Services;
using IdentityServerAspNetIdentity.Services.ClaimHandling;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
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
        IdentityModelEventSource.ShowPII = true;

        builder.Services.AddRazorPages();

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
        });

        builder.SetupUserDataStores();
        builder.SetupAjourIdentityServer();
        builder.SetupExternalIdentityProviders();

        if (builder.Environment.IsDevelopment())
        {
            // In development scenarios we don't want to run a cache server, so we just use in memory instead
            builder.Services.AddDistributedMemoryCache();
        }
        else
        {
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString("RedisCacheConnection");
            });
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
                options.ClientId = "122550137758-5ri39h9qant940fd06uuko89bep3crk6.apps.googleusercontent.com";
                options.ClientSecret = "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";
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
                options.IssuerUri = builder.Configuration["issuerUri"];

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
    
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        await InitializeIdentityDatabase(app);
        await MigrateUserDb(app);

        app.UseHsts();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapGet("/debug", async context =>
        {
            context.Response.ContentType = "text/plain";

            // Host info
            var name = Dns.GetHostName(); // get container id
            var ip = Dns.GetHostEntry(name).AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
            Console.WriteLine($"Host Name: { Environment.MachineName} \t {name}\t {ip}");
            await context.Response.WriteAsync($"Host Name: {Environment.MachineName}{Environment.NewLine}");
            await context.Response.WriteAsync(Environment.NewLine);

            // Request method, scheme, and path
            await context.Response.WriteAsync($"Request Method: {context.Request.Method}{Environment.NewLine}");
            await context.Response.WriteAsync($"Request Scheme: {context.Request.Scheme}{Environment.NewLine}");
            await context.Response.WriteAsync($"Request URL: {context.Request.GetDisplayUrl()}{Environment.NewLine}");
            await context.Response.WriteAsync($"Request Path: {context.Request.Path}{Environment.NewLine}");

            // Headers
            await context.Response.WriteAsync($"Request Headers:{Environment.NewLine}");
            foreach (var (key, value) in context.Request.Headers)
            {
                await context.Response.WriteAsync($"\t {key}: {value}{Environment.NewLine}");
            }
            await context.Response.WriteAsync(Environment.NewLine);

            // Connection: RemoteIp
            await context.Response.WriteAsync($"Request Remote IP: {context.Connection.RemoteIpAddress}");
        });
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