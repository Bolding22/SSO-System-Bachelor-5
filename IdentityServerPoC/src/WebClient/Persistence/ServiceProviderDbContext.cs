using Microsoft.EntityFrameworkCore;
using WebClient.Persistence.Models;

namespace WebClient.Persistence;

public class ServiceProviderDbContext : DbContext
{
    public DbSet<User> Users;

    public ServiceProviderDbContext(DbContextOptions<ServiceProviderDbContext> options) : base(options)
    {
        
    }
}