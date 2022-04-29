namespace IdentityServerAspNetIdentity.Pages.Directories.Invitations;

public class ViewModel
{
    public IEnumerable<Invitation> Invitations { get; set; }
}

public class Invitation
{
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; }
    public string FromEmail { get; set; }
    public DateTimeOffset TimeSent { get; set; }
}