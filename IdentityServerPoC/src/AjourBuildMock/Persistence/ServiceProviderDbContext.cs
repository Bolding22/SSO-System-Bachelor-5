using Microsoft.EntityFrameworkCore;
using WebClient.Persistence.Models;

namespace WebClient.Persistence;

public class ServiceProviderDbContext : DbContext
{
    public ServiceProviderDbContext(DbContextOptions<ServiceProviderDbContext> options) : base(options)
    {
        
    }

    public DbSet<User> Users { get; set; }
}