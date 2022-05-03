using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IdentityServerAspNetIdentity.Pages.Directories.Invite;

public class Index : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _userDbContext;

    public Index(UserManager<ApplicationUser> userManager, ApplicationDbContext userDbContext)
    {
        _userManager = userManager;
        _userDbContext = userDbContext;
    }
    
    public ViewModel View { get; set; }
    [BindProperty]
    public InputModel Input { get; set; }
    
    public async Task<ActionResult> OnGet()
    {
        // Get pending invites
        await BuildModelAsync();
        
        return Page();
    }

    public async Task<ActionResult> OnPost()
    {
        var fromUser = await _userManager.GetUserAsync(User);

        try
        {
            var toUser = await _userManager.FindByEmailAsync(Input.Email);

            if (toUser == null)
            {
                throw new ArgumentNullException(
                    nameof(toUser.Email), "User does not exist");     
                // Potential problem with other users using this to see which costumers are present in the system.
                // Some costumers do not want this information to be available to others.
            }
            
            if (fromUser.HomeDirectoryId == null)
            {
                throw new ArgumentNullException(
                    nameof(fromUser.HomeDirectory), "User needs a home directory to invite other users");
            }

            var toDirectoryId = (Guid) fromUser.HomeDirectoryId;


            if (toUser.Id == fromUser.Id)
            {
                throw new ArgumentException("You can't invite yourself");
            }

            if (await IsUserAlreadyInvited(fromUser, toUser, toDirectoryId))
            {
                throw new ArgumentException("User is already invited");
            }

            if (await IsUserAlreadyAGuest(toUser, toDirectoryId))
            {
                throw new ArgumentException("User already exists in the directory");
            }
            
            _userDbContext.Invites.Add(new Models.Invite
            {
                ToId = toUser.Id,
                FromId = fromUser.Id,
                ToDirectoryId = toDirectoryId,
                Created = DateTimeOffset.UtcNow
            });

            await _userDbContext.SaveChangesAsync();
        }
        catch (ArgumentException e)
        {
            ModelState.AddModelError(string.Empty, e.Message);
            await BuildModelAsync();
            return Page();
        }

        return RedirectToPage();
    }

    private async Task<bool> IsUserAlreadyAGuest(ApplicationUser toUser, Guid toDirectoryId)
    {
        var usersInDirectory = _userDbContext.Users.Where(user => user.UserAliases.Any(alias => alias.DirectoryId == toDirectoryId));
        var userExists = await usersInDirectory.ContainsAsync(toUser);
        return userExists;
    }

    private Task<bool> IsUserAlreadyInvited(ApplicationUser fromUser, ApplicationUser toUser, Guid toDirectoryId)
    {
        return _userDbContext.Invites.AnyAsync(invite => invite.FromId == fromUser.Id && invite.ToId == toUser.Id && invite.ToDirectoryId == toDirectoryId);
    }

    private async Task BuildModelAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        var invites = _userDbContext.Invites.Where(invite => invite.ToDirectoryId == user.HomeDirectoryId);
        var directory = user.HomeDirectoryId != Guid.Empty
            ? await _userDbContext.Directories.SingleOrDefaultAsync(dir => dir.Id == user.HomeDirectoryId)
            : null;

        View = new ViewModel()
        {
            CurrentDirectory = directory,
            PendingInvites = invites.Select(invite => new PendingInvite()
            {
                InvitedUser = invite.To,
                Created = invite.Created
            })
        };
    }
}