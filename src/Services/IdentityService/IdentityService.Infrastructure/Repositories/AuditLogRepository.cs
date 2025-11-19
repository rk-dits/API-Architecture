using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Data;

namespace IdentityService.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IdentityDbContext _context;

    public AuditLogRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);
        return auditLog;
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(al => al.UserId == userId)
            .OrderByDescending(al => al.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(al => al.OrganizationId == organizationId)
            .OrderByDescending(al => al.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByResourceAsync(string resource, string? resourceId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs
            .Where(al => al.Resource == resource);

        if (!string.IsNullOrEmpty(resourceId))
        {
            query = query.Where(al => al.ResourceId == resourceId);
        }

        return await query
            .OrderByDescending(al => al.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(al => al.Timestamp >= from && al.Timestamp <= to)
            .OrderByDescending(al => al.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);
        var logs = await query
            .OrderByDescending(al => al.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (logs, totalCount);
    }
}