// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace IdentityServerAspNetIdentity.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public ICollection<UserAlias> UserAliases { get; set; }
}

/// <summary>
/// The representation of a user alias on other systems.
/// </summary>
public class UserAlias
{
    [Key]
    public Guid SystemUserId { get; set; }
    public Guid OrganizationId { get; set; }
}