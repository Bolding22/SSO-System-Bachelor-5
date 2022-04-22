using System.Security.Claims;
using System.Text.Json;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace IdentityServerAspNetIdentity.Services.ClaimHandling;

public class ClaimsTransformation : IClaimsTransformation
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _applicationDbContext;

    public ClaimsTransformation(UserManager<ApplicationUser> userManager, ApplicationDbContext applicationDbContext)
    {
        _userManager = userManager;
        _applicationDbContext = applicationDbContext;
    }
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (!principal.HasClaim(claim => claim.Type == AjourClaims.UserAlias))
        {
            var userId = _userManager.GetUserId(principal);
            if (userId == null)
            {
                return Task.FromResult(principal);
            }
            
            var user = _applicationDbContext.Users
                .Include(applicationUser => applicationUser.UserAliases)
                .Single(applicationUser => applicationUser.Id == userId);
        
            var userAliases = user.UserAliases.ToDictionary(alias => alias.DirectoryId, alias => alias.SystemUserId);
            var userAliasesJson = JsonSerializer.Serialize(userAliases);

            var claimsIdentity = principal.Identities.First();
            claimsIdentity.AddClaim(new Claim(AjourClaims.UserAlias, userAliasesJson));
        }

        return Task.FromResult(principal);
    }
}