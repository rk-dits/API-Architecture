using System.Reflection;
using BuildingBlocks.Persistence.Entity;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Persistence.Ef;

/// <summary>
/// Base EF Core DbContext with common conventions applied (config scanning, concurrency token wiring).
/// Derive per service to add DbSets and additional configuration.
/// </summary>
public abstract class AppDbContextBase : DbContext
{
    /// <summary>
    /// Initializes the context with externally provided options.
    /// </summary>
    /// <param name="options">The configured <see cref="DbContextOptions"/>.</param>
    protected AppDbContextBase(DbContextOptions options) : base(options) { }

    /// <summary>
    /// Applies entity type configurations and sets up concurrency tokens for entities implementing <see cref="IHasConcurrencyToken"/>.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Apply configurations from derived assemblies
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        // Concurrency tokens
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clr = entityType.ClrType;
            if (typeof(IHasConcurrencyToken).IsAssignableFrom(clr))
            {
                var prop = entityType.FindProperty(nameof(IHasConcurrencyToken.ConcurrencyToken));
                if (prop != null)
                {
                    prop.IsConcurrencyToken = true;
                }
            }
        }
    }
}
