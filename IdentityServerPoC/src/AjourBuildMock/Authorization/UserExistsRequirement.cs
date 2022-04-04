using Microsoft.AspNetCore.Authorization;

namespace WebClient.Authorization;

public class UserExistsRequirement : IAuthorizationRequirement
{
    public UserExistsRequirement(Guid organizationId)
    {
        OrganizationId = organizationId;
    }

    public Guid OrganizationId { get; set; }
}