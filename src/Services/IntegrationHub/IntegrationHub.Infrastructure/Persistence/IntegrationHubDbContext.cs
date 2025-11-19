using BuildingBlocks.Persistence.Ef;
using IntegrationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using IntegrationHub.Infrastructure.Persistence.Entity;

namespace IntegrationHub.Infrastructure.Persistence;

public class IntegrationHubDbContext : AppDbContextBase
{
    public IntegrationHubDbContext(DbContextOptions<IntegrationHubDbContext> options) : base(options)
    {
    }

    public DbSet<Operation> Operations => Set<Operation>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
}