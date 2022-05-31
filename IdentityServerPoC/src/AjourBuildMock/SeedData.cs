using Microsoft.EntityFrameworkCore;
using WebClient.Persistence;
using WebClient.Persistence.Models;

namespace WebClient;

public static class SeedData
{
    public static void EnsureSeedData(WebApplication app)
    {
        using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ServiceProviderDbContext>();
            
        dbContext.Database.Migrate();
            
        var systemUserIdAlice = Guid.Parse("7243B8E0-7FD3-40D4-A46C-802D68BC0F76");
        var aliceExists = dbContext.Users.Any(user => user.Id == systemUserIdAlice);

        if (!aliceExists)
        {
            dbContext.Users.Add(new User()
            {
                Id = systemUserIdAlice,
                FirstName = "Alice",
                LastName = "Doe",
                Email = "AliceSmith@email.com",
                PhoneNumber = "12345678"
            });
            dbContext.SaveChanges();
        }
    }
}