using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Shared;
using WebClient.Persistence;

namespace WebClient.Authorization;

public class UserExistsHandler : AuthorizationHandler<UserExistsRequirement>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public UserExistsHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserExistsRequirement requirement)
    {
        var userAliasClaim = context.User.FindFirst(claim => claim.Type == AjourClaims.UserAlias);

        // Does the user have the claim?
        if (userAliasClaim is null)
        {
            return;
        }

        // Can the claim be deserialized?
        var orgIdToUserId = JsonSerializer.Deserialize<Dictionary<Guid, Guid>>(userAliasClaim.Value);
        if (orgIdToUserId == null)
        {
            return;
        }
        
        // Is this systems organizationId actually in the claim?
        if (!orgIdToUserId.TryGetValue(requirement.OrganizationId, out var userId))
        {
            return;
        }
        
        // Does the collected user id exist in this system?
        // If a user is added to an organization after the token has been creates, then the token will be missing the updated info.
        // This results in access not being granted to the newly added organization until a new token has been issued. 
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ServiceProviderDbContext>();
        var userExists = await dbContext.Users.AnyAsync(user => user.Id == userId);
        if (!userExists)
        {
            return;
        }

        context.Succeed(requirement);
    }
}