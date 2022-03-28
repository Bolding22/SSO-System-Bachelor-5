using Shared;

namespace IdentityServerAspNetIdentity.ClaimHandling;

using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

public class ClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (!principal.HasClaim(claim => claim.Type == AjourClaims.UserAlias))
        {
            var claimsIdentity = principal.Identities.First();
            claimsIdentity.AddClaim(new Claim(AjourClaims.UserAlias, "myClaimValue"));
        }
        
        return Task.FromResult(principal);
    }
}