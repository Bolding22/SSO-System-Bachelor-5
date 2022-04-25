using IdentityServerAspNetIdentity.Data.Migrations;

namespace IdentityServerAspNetIdentity.Models;

public class Invite
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FromId { get; set; }
    public ApplicationUser From { get; set; }
    public string ToId { get; set; }
    public ApplicationUser To { get; set; }
    public Guid ToDirectoryId { get; set; }
    public Directory ToDirectory { get; set; }
    public DateTimeOffset Created { get; set; }
}