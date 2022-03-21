using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityModel;

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
                Name = "verification",
                UserClaims = new List<string> 
                { 
                    JwtClaimTypes.Email,
                    JwtClaimTypes.EmailVerified,
                    JwtClaimTypes.Address,
                    JwtClaimTypes.WebSite
                }
            },
            new() 
            {
                Name = "userAliases",
                UserClaims = new List<string>()
                {
                    JwtClaimTypes.PhoneNumberVerified
                }
            }
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("scope1"),
            new ApiScope("scope2"),
            new ApiScope("api1", "My API")
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // interactive ASP.NET Core Web App
            // webclient
            new Client
            {
                ClientId = "webclient",
                ClientName = "Ajour Service Provider Mock",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,
            
                // where to redirect to after login
                RedirectUris = { "https://localhost:5002/signin-oidc" },

                // where to redirect to after logout
                PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

                AllowOfflineAccess = true,
                
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "verification",
                    "api1",
                    "userAliases"
                },
                Claims =
                {
                    new ClientClaim("customer_id", "123")
                },
                AlwaysSendClientClaims = true
            }
        };
}
