using BuildingBlocks.Persistence.Ef;
using CoreWorkflowService.Domain.Entities;
using CoreWorkflowService.Infrastructure.Persistence.Entity;
using Microsoft.EntityFrameworkCore;

namespace CoreWorkflowService.Infrastructure.Persistence;

public class CoreWorkflowServiceDbContext : AppDbContextBase
{
    public CoreWorkflowServiceDbContext(DbContextOptions<CoreWorkflowServiceDbContext> options) : base(options)
    {
    }

    public DbSet<WorkflowCase> WorkflowCases => Set<WorkflowCase>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
}
