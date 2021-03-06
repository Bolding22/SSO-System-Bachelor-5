using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using IdentityServerAspNetIdentity.Pages.Directories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

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
        await BuildModelAsync();

        return Page();
    }

    private async Task BuildModelAsync()
    {
        var userId = _userManager.GetUserId(User);
        var user = await _userDbContext.Users
            .Include(u => u.UserAliases)
            .SingleAsync(u => u.Id == userId);
        
        var directoryIds = user.UserAliases.Select(alias => alias.DirectoryId).ToList();
        if (user.HomeDirectoryId != null)
        {
            directoryIds.Add((Guid) user.HomeDirectoryId);
        }
        
        var directories = _userDbContext.Directories
            .Where(directory => directoryIds.Contains(directory.Id));

        View = new ViewModel()
        {
            Directories = directories.Select(directory => new ViewModel.DetailedDirectory()
            {
                Name = directory.Name,
                IsHomeDirectory = directory.Id == user.HomeDirectoryId
            })
        };
    }
}