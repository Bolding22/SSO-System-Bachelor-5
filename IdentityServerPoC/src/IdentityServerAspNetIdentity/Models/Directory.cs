namespace IdentityServerAspNetIdentity.Models;

/// <summary>
/// A container for user accounts.
/// Users can have multiple directories that they are a part of but only one home directory.
/// </summary>
public class Directory
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}