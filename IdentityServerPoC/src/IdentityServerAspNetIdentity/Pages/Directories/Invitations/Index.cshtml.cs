using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServerAspNetIdentity.Pages.Directories.Invitations;

public class Index : PageModel
{
    public ViewModel View { get; set; }
    
    public async Task OnGet()
    {
        var tokenAsync = await HttpContext.AuthenticateAsync();
    }
}