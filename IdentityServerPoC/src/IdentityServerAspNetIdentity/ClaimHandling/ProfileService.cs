﻿using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace IdentityServerAspNetIdentity.ClaimHandling;

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
        
        context.IssuedClaims.Add(new Claim("test-claim", "test-value"));
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task IsActiveAsync(IsActiveContext context)
    {
        context.IsActive = true;
        return Task.CompletedTask;
    }
}