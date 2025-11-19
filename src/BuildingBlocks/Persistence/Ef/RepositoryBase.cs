using System.Linq.Expressions;
using BuildingBlocks.Persistence.Abstractions;
using BuildingBlocks.Common.Pagination;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Persistence.Ef;

/// <summary>
/// Generic repository base with simple query and pagination helpers.
/// </summary>
public class RepositoryBase<T> : IRepository<T> where T : class
{
    protected readonly DbContext _dbContext;
    protected readonly DbSet<T> _set;

    public RepositoryBase(DbContext dbContext)
    {
        _dbContext = dbContext;
        _set = _dbContext.Set<T>();
    }

    public async Task<T?> GetAsync(Guid id, CancellationToken ct = default)
        => await _set.FindAsync(new object?[] { id }, ct).AsTask();

    public async Task<PageResult<T>> QueryAsync(Expression<Func<T, bool>> predicate, PageRequest page, CancellationToken ct = default)
    {
        var query = _set.Where(predicate);
        var total = await query.CountAsync(ct);
        var items = await query.Skip(page.Skip).Take(page.Take).ToListAsync(ct);
        return new PageResult<T>(items, total, page.Page, page.Size);
    }

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _set.AddAsync(entity, ct);

    public void Update(T entity) => _set.Update(entity);
    public void Remove(T entity) => _set.Remove(entity);
}
