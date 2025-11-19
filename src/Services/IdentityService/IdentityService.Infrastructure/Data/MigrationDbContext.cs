using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Infrastructure.Data;

public class MigrationDbContext : DbContext
{
    public MigrationDbContext(DbContextOptions<MigrationDbContext> options) : base(options)
    {
    }

    // Identity entities
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<IdentityUserRole<Guid>> UserRoles { get; set; }
    public DbSet<IdentityUserClaim<Guid>> UserClaims { get; set; }
    public DbSet<IdentityUserLogin<Guid>> UserLogins { get; set; }
    public DbSet<IdentityRoleClaim<Guid>> RoleClaims { get; set; }
    public DbSet<IdentityUserToken<Guid>> UserTokens { get; set; }

    // Basic entities without complex relationships
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Identity tables explicitly
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("AspNetUsers");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("AspNetRoles");
        });

        modelBuilder.Entity<IdentityUserRole<Guid>>(entity =>
        {
            entity.ToTable("AspNetUserRoles");
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });
        });

        modelBuilder.Entity<IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToTable("AspNetUserClaims");
            entity.HasKey(uc => uc.Id);
        });

        modelBuilder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToTable("AspNetUserLogins");
            entity.HasKey(ul => new { ul.LoginProvider, ul.ProviderKey });
        });

        modelBuilder.Entity<IdentityRoleClaim<Guid>>(entity =>
        {
            entity.ToTable("AspNetRoleClaims");
            entity.HasKey(rc => rc.Id);
        });

        modelBuilder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.ToTable("AspNetUserTokens");
            entity.HasKey(ut => new { ut.UserId, ut.LoginProvider, ut.Name });
        });

        // Basic User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ProfilePicture).HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired().HasConversion<string>();

            // Ignore complex properties for basic migration
            entity.Ignore(e => e.MfaBackupCodes);
            entity.Ignore(e => e.SecurityQuestions);
            entity.Ignore(e => e.UserOrganizations);
            entity.Ignore(e => e.UserPermissions);
            entity.Ignore(e => e.UserRoles);
            entity.Ignore(e => e.UserSessions);
            entity.Ignore(e => e.AuditLogs);
        });

        // Basic Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Type).IsRequired().HasConversion<string>();
            entity.Property(e => e.Level).IsRequired().HasConversion<string>();

            // Ignore complex properties for basic migration
            entity.Ignore(e => e.ChildRoles);
            entity.Ignore(e => e.RolePermissions);
            entity.Ignore(e => e.UserRoles);
        });

        // Basic Organization configuration
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Status).IsRequired().HasConversion<string>();
            entity.Property(e => e.Type).IsRequired().HasConversion<string>();
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Ignore complex properties for basic migration
            entity.Ignore(e => e.SubOrganizations);
            entity.Ignore(e => e.Metadata);
            entity.Ignore(e => e.UserOrganizations);
            entity.Ignore(e => e.OrganizationRoles);
            entity.Ignore(e => e.OrganizationPermissions);
            entity.Ignore(e => e.ApiKeys);
        });

        // Basic Permission configuration
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Resource).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Type).IsRequired().HasConversion<string>();
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Ignore complex properties for basic migration
            entity.Ignore(e => e.ChildPermissions);
            entity.Ignore(e => e.Conditions);
            entity.Ignore(e => e.Metadata);
            entity.Ignore(e => e.RolePermissions);
            entity.Ignore(e => e.UserPermissions);
            entity.Ignore(e => e.OrganizationPermissions);
        });

        // Basic UserSession configuration
        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).HasMaxLength(255).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(1000);
            entity.Property(e => e.Status).IsRequired().HasConversion<string>();
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Basic relationship to User
            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Basic Subscription configuration
        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Tier).IsRequired().HasConversion<string>();
            entity.Property(e => e.Status).IsRequired().HasConversion<string>();
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Ignore complex properties for basic migration
            entity.Ignore(e => e.FeatureLimits);
            entity.Ignore(e => e.CurrentUsage);
            entity.Ignore(e => e.IncludedFeatures);
            entity.Ignore(e => e.Metadata);

            // Basic relationship to Organization
            entity.HasOne<Organization>()
                  .WithMany()
                  .HasForeignKey(e => e.OrganizationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Add indexes
        modelBuilder.Entity<User>()
            .HasIndex(e => e.Email)
            .IsUnique();

        modelBuilder.Entity<Organization>()
            .HasIndex(e => e.Name)
            .IsUnique();

        modelBuilder.Entity<Permission>()
            .HasIndex(e => e.Name)
            .IsUnique();
    }
}