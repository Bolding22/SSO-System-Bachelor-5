using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServerAspNetIdentity.Pages.Organizations;

public class Index : PageModel
{
    public async Task<IActionResult> OnGet()
    {
        return Page();
    }
}