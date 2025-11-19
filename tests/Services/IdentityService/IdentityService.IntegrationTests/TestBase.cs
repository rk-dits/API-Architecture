using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using IdentityService.Infrastructure.Data;
using IdentityService.IntegrationTests.Helpers;

namespace IdentityService.IntegrationTests;

public class TestBase : IClassFixture<IdentityServiceWebApplicationFactory>
{
    protected readonly IdentityServiceWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly IServiceScope Scope;
    protected readonly MigrationDbContext DbContext;

    public TestBase(IdentityServiceWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Scope = factory.Services.CreateScope();
        DbContext = Scope.ServiceProvider.GetRequiredService<MigrationDbContext>();

        // Ensure clean database state
        CleanDatabase();
    }

    protected virtual void CleanDatabase()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Scope?.Dispose();
        Client?.Dispose();
    }
}