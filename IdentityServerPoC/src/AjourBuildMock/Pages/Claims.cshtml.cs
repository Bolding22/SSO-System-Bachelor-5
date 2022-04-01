using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebClient.Pages;

public class Claims : PageModel
{
    private readonly ILogger<Claims> _logger;

    public Claims(ILogger<Claims> logger)
    {
        _logger = logger;
    }

    public async Task OnGet()
    {
        var userClaims = HttpContext.User.Claims;
        var authenticateResult = await HttpContext.AuthenticateAsync();
    }
}