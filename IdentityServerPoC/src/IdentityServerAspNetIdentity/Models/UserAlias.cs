using System.ComponentModel.DataAnnotations;

namespace IdentityServerAspNetIdentity.Models;

/// <summary>
/// The representation of a user alias on other systems.
/// </summary>
public class UserAlias
{
    [Key]
    public Guid SystemUserId { get; set; }
    public Guid DirectoryId { get; set; }
    public Directory Directory { get; set; }
}

public class Directory
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}