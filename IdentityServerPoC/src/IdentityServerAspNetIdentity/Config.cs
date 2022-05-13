using System.Security.Claims;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityModel;
using Shared;
using IdentityResources = Duende.IdentityServer.Models.IdentityResources;

namespace IdentityServerAspNetIdentity;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new() 
            {
                Name = IdentityResourceNames.UserAliases,
                DisplayName = "User Aliases",
                UserClaims = 
                {
                    AjourClaims.UserAlias
                }
            }
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope(ApiScopeNames.Api, "Ajour Mock Api")
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // interactive ASP.NET Core Web App
            // webclient
            new Client
            {
                ClientId = ClientIds.AjourServiceProvider,
                ClientName = "Ajour Service Provider Mock",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                AllowOfflineAccess = true,
                
                // where to redirect to after login
                RedirectUris = { "https://localhost:5002/signin-oidc" },

                // where to redirect to after logout
                PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },
                
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    ApiScopeNames.Api,                      // The Ajour API
                    IdentityResourceNames.UserAliases       // The user aliases for the systems
                }
            }
        };
}
