using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IdentityService.Domain.Entities;
using System.Text.Json;

namespace IdentityService.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Resource)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ResourceId)
            .HasMaxLength(100);

        builder.Property(x => x.EventType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(x => x.OldValues)
            .HasColumnType("text");

        builder.Property(x => x.NewValues)
            .HasColumnType("text");

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        builder.Property(x => x.IpAddress)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.UserAgent)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.SessionId)
            .HasMaxLength(200);

        builder.Property(x => x.Metadata)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null!) ?? new Dictionary<string, object>());

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany(x => x.AuditLogs)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.OrganizationId);
        builder.HasIndex(x => x.EventType);
        builder.HasIndex(x => x.Resource);
        builder.HasIndex(x => x.Timestamp);
    }
}