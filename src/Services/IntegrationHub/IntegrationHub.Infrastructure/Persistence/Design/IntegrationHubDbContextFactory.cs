using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace IntegrationHub.Infrastructure.Persistence.Design;

public class IntegrationHubDbContextFactory : IDesignTimeDbContextFactory<IntegrationHubDbContext>
{
    public IntegrationHubDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.json", optional: true);
        var config = builder.Build();
        var cs = config.GetConnectionString("Default") ?? "Host=localhost;Port=5432;Database=integrationhub;Username=postgres;Password=postgres";
        var optionsBuilder = new DbContextOptionsBuilder<IntegrationHubDbContext>()
            .UseNpgsql(cs);
        return new IntegrationHubDbContext(optionsBuilder.Options);
    }
}