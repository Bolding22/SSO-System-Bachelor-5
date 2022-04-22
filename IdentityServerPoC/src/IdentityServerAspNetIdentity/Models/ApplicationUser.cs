// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace IdentityServerAspNetIdentity.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public ICollection<UserAlias> UserAliases { get; set; }
    public Guid? HomeDirectoryId { get; set; } = null;
    public Directory HomeDirectory { get; set; }
    public ICollection<Invite> Invites { get; set; }
}