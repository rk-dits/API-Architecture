using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Data;

namespace IdentityService.Infrastructure.Repositories;

public class ApiKeyRepository : IApiKeyRepository
{
    private readonly IdentityDbContext _context;

    public ApiKeyRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<ApiKey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ApiKeys
            .Include(ak => ak.User)
            .Include(ak => ak.Organization)
            .FirstOrDefaultAsync(ak => ak.Id == id, cancellationToken);
    }

    public async Task<ApiKey?> GetByKeyValueAsync(string keyValue, CancellationToken cancellationToken = default)
    {
        return await _context.ApiKeys
            .Include(ak => ak.User)
            .Include(ak => ak.Organization)
            .FirstOrDefaultAsync(ak => ak.KeyValue == keyValue, cancellationToken);
    }

    public async Task<IEnumerable<ApiKey>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.ApiKeys
            .Where(ak => ak.UserId == userId)
            .OrderByDescending(ak => ak.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ApiKey>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _context.ApiKeys
            .Where(ak => ak.OrganizationId == organizationId)
            .OrderByDescending(ak => ak.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ApiKey> AddAsync(ApiKey apiKey, CancellationToken cancellationToken = default)
    {
        _context.ApiKeys.Add(apiKey);
        await _context.SaveChangesAsync(cancellationToken);
        return apiKey;
    }

    public async Task UpdateAsync(ApiKey apiKey, CancellationToken cancellationToken = default)
    {
        _context.ApiKeys.Update(apiKey);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var apiKey = await GetByIdAsync(id, cancellationToken);
        if (apiKey != null)
        {
            _context.ApiKeys.Remove(apiKey);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task CleanupExpiredKeysAsync(CancellationToken cancellationToken = default)
    {
        var expiredKeys = await _context.ApiKeys
            .Where(ak => ak.IsExpired)
            .ToListAsync(cancellationToken);

        _context.ApiKeys.RemoveRange(expiredKeys);
        await _context.SaveChangesAsync(cancellationToken);
    }
}