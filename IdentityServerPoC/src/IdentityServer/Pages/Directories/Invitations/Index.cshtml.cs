using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IdentityServerAspNetIdentity.Pages.Directories.Invitations;

public class Index : PageModel
{
    private readonly ApplicationDbContext _userDbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public Index(ApplicationDbContext userDbContext, UserManager<ApplicationUser> userManager)
    {
        _userDbContext = userDbContext;
        _userManager = userManager;
    }

    public ViewModel View { get; set; }

    public async Task<PageResult> OnGet()
    {
        await BuildModelAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAccept(Guid id)
    {
        // Add the user as guest
        var userAlias = new UserAlias()
        {
            SystemUserId =
                Guid.NewGuid(),
            DirectoryId = id
        };
        _userDbContext.UserAliases.Add(userAlias);
        var userId = _userManager.GetUserId(User);
        var user = await _userDbContext.Users
            .Include(u => u.UserAliases)
            .SingleAsync(u => u.Id == userId);
        user.UserAliases.Add(userAlias);

        await RemoveInvite(id);

        // Save changes
        await _userDbContext.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDecline(Guid id)
    {
        await RemoveInvite(id);

        // Save changes
        await _userDbContext.SaveChangesAsync();

        return RedirectToPage();
    }

    /// <summary>
    /// Deletes the invite of the current user where the directory id matches the directory the invite was to.
    /// </summary>
    /// <remarks>Remember to call SaveChanges afterwards</remarks>
    /// <param name="directoryId"></param>
    private async Task RemoveInvite(Guid directoryId)
    {
        var userId = _userManager.GetUserId(User);
        var invite =
            await _userDbContext.Invites.SingleAsync(invite => invite.ToId == userId && invite.ToDirectoryId == directoryId);
        _userDbContext.Invites.Remove(invite);
    }

    public Task BuildModelAsync()
    {
        var userId = _userManager.GetUserId(User);

        View = new ViewModel()
        {
            Invitations = _userDbContext.Invites
                .Where(invite => invite.ToId == userId)
                .Include(invite => invite.From)
                .Include(invite => invite.ToDirectory)
                .Select(invite => new Invitation()
                {
                    FromEmail = invite.From.Email,
                    OrganizationId = invite.ToDirectoryId,
                    OrganizationName = invite.ToDirectory.Name,
                    TimeSent = invite.Created
                })
        };
        return Task.CompletedTask;
    }
}