using System.Linq.Expressions;
using BuildingBlocks.Common.Pagination;

namespace BuildingBlocks.Persistence.Abstractions;

public interface IRepository<T> where T : class
{
    Task<T?> GetAsync(Guid id, CancellationToken ct = default);
    Task<PageResult<T>> QueryAsync(Expression<Func<T, bool>> predicate, PageRequest page, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}
