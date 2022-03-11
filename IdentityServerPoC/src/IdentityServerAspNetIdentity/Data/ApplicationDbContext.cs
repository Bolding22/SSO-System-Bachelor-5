using Microsoft.EntityFrameworkCore;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace IdentityServerAspNetIdentity.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private DbSet<UserAlias> UserAliases { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
