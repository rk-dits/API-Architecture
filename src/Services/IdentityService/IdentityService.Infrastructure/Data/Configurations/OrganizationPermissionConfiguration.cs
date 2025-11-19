using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IdentityService.Domain.Entities;
using System.Text.Json;

namespace IdentityService.Infrastructure.Data.Configurations;

public class OrganizationPermissionConfiguration : IEntityTypeConfiguration<OrganizationPermission>
{
    public void Configure(EntityTypeBuilder<OrganizationPermission> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.GrantedBy)
            .IsRequired()
            .HasMaxLength(100);

        // Temporarily ignore complex properties for migration
        builder.Ignore(x => x.Conditions);

        // Relationships
        builder.HasOne(x => x.Organization)
            .WithMany(x => x.OrganizationPermissions)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permission)
            .WithMany(x => x.OrganizationPermissions)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.NoAction);

        // Indexes
        builder.HasIndex(x => new { x.OrganizationId, x.PermissionId }).IsUnique();
    }
}