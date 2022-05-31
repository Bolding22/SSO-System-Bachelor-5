using System.Security.Claims;
using IdentityModel;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Directory = IdentityServerAspNetIdentity.Models.Directory;

namespace IdentityServerAspNetIdentity;

public class SeedData
{
    public static void EnsureSeedData(WebApplication app)
    {
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            CreateOrganizations(scope, out var organizationId1, out var organizationId2);

            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var alice = userMgr.FindByNameAsync("alice").Result;
            if (alice == null)
            {
                alice = new ApplicationUser
                {
                    UserName = "alice",
                    Email = "AliceSmith@email.com",
                    EmailConfirmed = true,
                    HomeDirectoryId = organizationId1
                };
                var result = userMgr.CreateAsync(alice, "Pass123$").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(alice, new Claim[]{
                            new Claim(JwtClaimTypes.Name, "Alice Smith"),
                            new Claim(JwtClaimTypes.GivenName, "Alice"),
                            new Claim(JwtClaimTypes.FamilyName, "Smith"),
                            new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                        }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                Log.Debug("alice created");
            }
            else
            {
                Log.Debug("alice already exists");
            }

            var bob = userMgr.FindByNameAsync("bob").Result;
            if (bob == null)
            {
                bob = new ApplicationUser
                {
                    UserName = "bob",
                    Email = "BobSmith@email.com",
                    EmailConfirmed = true,
                    HomeDirectoryId = organizationId2
                };
                var result = userMgr.CreateAsync(bob, "Pass123$").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(bob, new Claim[]{
                            new Claim(JwtClaimTypes.Name, "Bob Smith"),
                            new Claim(JwtClaimTypes.GivenName, "Bob"),
                            new Claim(JwtClaimTypes.FamilyName, "Smith"),
                            new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                            new Claim("location", "somewhere")
                        }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                Log.Debug("bob created");
            }
            else
            {
                Log.Debug("bob already exists");
            }

            CreateAliases(scope, organizationId1, alice);
        }
    }

    private static void CreateAliases(IServiceScope scope, Guid organizationId1, ApplicationUser alice)
    {
        var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var systemUserIdAlice = Guid.Parse("7243B8E0-7FD3-40D4-A46C-802D68BC0F76");

        var aliceAliasExists = applicationDbContext.UserAliases
            .Any(alias => alias.DirectoryId == organizationId1 
                          && alias.SystemUserId == systemUserIdAlice);
        
        if (!aliceAliasExists)
        {
            var userAlias = new UserAlias()
            {
                DirectoryId = organizationId1,
                SystemUserId = systemUserIdAlice
            };
            applicationDbContext.UserAliases.Add(userAlias);
            applicationDbContext.SaveChanges();
            var dbAlice = applicationDbContext.Users
                .Include(user => user.UserAliases)
                .Single(user => user.Id == alice.Id);
            dbAlice.UserAliases.Add(userAlias);
            applicationDbContext.SaveChanges();
        }
    }

    private static void CreateOrganizations(IServiceScope scope, out Guid organizationId1, out Guid organizationId2)
    {
        var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        organizationId1 = Guid.Parse("68C450CF-A13F-4957-990D-E27E5DB8BD7B");
        var id1 = organizationId1;
        var directory1Exists = applicationDbContext.Directories.Any(directory => directory.Id == id1);

        if (!directory1Exists)
        {
            applicationDbContext.Directories.Add(new Directory()
            {
                Id = organizationId1,
                Name = "Primary organization"
            });
            applicationDbContext.SaveChanges();
        }

        organizationId2 = Guid.Parse("FC250A49-810A-4550-B39A-223DBF08EFE8");
        var id2 = organizationId2;
        var directory2Exists = applicationDbContext.Directories.Any(directory => directory.Id == id2);

        if (!directory2Exists)
        {
            applicationDbContext.Directories.Add(new Directory()
            {
                Id = organizationId2,
                Name = "Secondary organization"
            });
            applicationDbContext.SaveChanges();
        }
    }
}
