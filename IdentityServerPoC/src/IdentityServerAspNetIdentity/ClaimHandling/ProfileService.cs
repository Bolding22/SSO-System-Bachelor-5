using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace IdentityServerAspNetIdentity.ClaimHandling;

public class ProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    
    /// <inheritdoc/>
    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        if (context.RequestedClaimTypes.Any())
        {
            var user = await _userManager.GetUserAsync(context.Subject);
            var claims = await _userManager.GetClaimsAsync(user);
            context.AddRequestedClaims(context.Subject.Claims);
        }
        
        context.IssuedClaims.Add(new Claim("test-claim", "test-value"));
    }

    /// <inheritdoc/>
    public Task IsActiveAsync(IsActiveContext context)
    {
        return Task.CompletedTask;
    }
}