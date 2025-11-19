using BuildingBlocks.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Persistence.Ef;

/// <summary>
/// Implements <see cref="IUnitOfWork"/> on top of an EF Core DbContext.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    public UnitOfWork(DbContext context) => _context = context;
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
    public DbSet<T> Set<T>() where T : class => _context.Set<T>();
}
