using IdentityServerAspNetIdentity.Models;
using Directory = IdentityServerAspNetIdentity.Models.Directory;

namespace IdentityServerAspNetIdentity.Pages.Directories.Invite;

public class ViewModel
{
    public Directory CurrentDirectory { get; set; }
    public IEnumerable<PendingInvite> PendingInvites { get; set; }
}

public class PendingInvite
{
    public ApplicationUser InvitedUser { get; set; }
    public DateTimeOffset Created { get; set; }
}