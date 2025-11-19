using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Data.Configurations;
using Common.Entities;

namespace IdentityService.Infrastructure.Data;

public class IdentityDbContext : IdentityDbContext<User, Role, Guid>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    // Additional entities beyond the standard Identity entities

    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<OrganizationRole> OrganizationRoles { get; set; }
    public DbSet<OrganizationPermission> OrganizationPermissions { get; set; }
    public DbSet<UserOrganization> UserOrganizations { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply configurations
        builder.ApplyConfiguration(new UserConfiguration());
        builder.ApplyConfiguration(new OrganizationConfiguration());
        builder.ApplyConfiguration(new RoleConfiguration());
        builder.ApplyConfiguration(new PermissionConfiguration());
        builder.ApplyConfiguration(new UserRoleConfiguration());
        builder.ApplyConfiguration(new UserPermissionConfiguration());
        builder.ApplyConfiguration(new RolePermissionConfiguration());
        builder.ApplyConfiguration(new OrganizationRoleConfiguration());
        builder.ApplyConfiguration(new OrganizationPermissionConfiguration());
        builder.ApplyConfiguration(new UserOrganizationConfiguration());
        builder.ApplyConfiguration(new UserSessionConfiguration());
        builder.ApplyConfiguration(new ApiKeyConfiguration());
        builder.ApplyConfiguration(new AuditLogConfiguration());
        builder.ApplyConfiguration(new SubscriptionConfiguration());

        // Identity tables are automatically configured by base class
        // Additional custom configurations can be added here if needed
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = "system"; // TODO: Get from current user context
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = "system"; // TODO: Get from current user context
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}