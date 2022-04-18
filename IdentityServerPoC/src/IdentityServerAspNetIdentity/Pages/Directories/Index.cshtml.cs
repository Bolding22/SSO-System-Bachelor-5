using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using IdentityServerAspNetIdentity.Pages.Directories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServerAspNetIdentity.Pages.Organizations;

[SecurityHeaders]
[Authorize]
public class Index : PageModel
{
    private readonly ApplicationDbContext _userDbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    public ViewModel View { get; set; }

    public Index(ApplicationDbContext userDbContext, UserManager<ApplicationUser> userManager)
    {
        _userDbContext = userDbContext;
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGet()
    {
        var user = await _userManager.GetUserAsync(User);

        var directoryIds = user.UserAliases.Select(alias => alias.DirectoryId);
        var directories = _userDbContext.Directories.Where(directory => directoryIds.Contains(directory.Id));

        View = new ViewModel(directories, user.HomeDirectoryId);

        return Page();
    }
}