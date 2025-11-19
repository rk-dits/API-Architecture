using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CoreWorkflowService.Infrastructure.Persistence.Design;

public class CoreWorkflowServiceDbContextFactory : IDesignTimeDbContextFactory<CoreWorkflowServiceDbContext>
{
    public CoreWorkflowServiceDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.json", optional: true);
        var config = builder.Build();
        var cs = config.GetConnectionString("Default") ?? "Host=localhost;Port=5432;Database=coreworkflow;Username=postgres;Password=postgres";
        var optionsBuilder = new DbContextOptionsBuilder<CoreWorkflowServiceDbContext>()
            .UseNpgsql(cs);
        return new CoreWorkflowServiceDbContext(optionsBuilder.Options);
    }
}
