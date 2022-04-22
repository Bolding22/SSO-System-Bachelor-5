using Microsoft.EntityFrameworkCore;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Directory = IdentityServerAspNetIdentity.Models.Directory;

namespace IdentityServerAspNetIdentity.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<UserAlias> UserAliases { get; set; }
    public DbSet<Directory> Directories { get; set; }
    public DbSet<Invite> Invites { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .HasMany(user => user.Invites)
            .WithOne(invite => invite.To).HasForeignKey(invite => invite.ToId);
    }
}
