using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityService.Infrastructure.Data;

/// <summary>
/// Design-time factory for IdentityDbContext to enable EF migrations
/// </summary>
public class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();

        // Use SQL Server for design-time (migrations)
        // This connection string is only used for migration generation
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=IdentityService;Trusted_Connection=true;MultipleActiveResultSets=true");

        return new IdentityDbContext(optionsBuilder.Options);
    }
}