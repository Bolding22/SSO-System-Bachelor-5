using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;

namespace IdentityServerAspNetIdentity.Services.ClaimHandling;

public class ProfileService : IProfileService
{
    /// <inheritdoc/>
    public Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var identityResources = context.RequestedResources.Resources.IdentityResources;
        var userClaims = identityResources.SelectMany(resource => resource.UserClaims);
        
        context.RequestedClaimTypes = userClaims;

        if (context.RequestedClaimTypes.Any())
        {
            context.AddRequestedClaims(context.Subject.Claims);
        }
        
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task IsActiveAsync(IsActiveContext context)
    {
        return Task.CompletedTask;
    }
}