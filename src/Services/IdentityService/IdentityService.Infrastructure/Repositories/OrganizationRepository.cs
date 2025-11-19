using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Data;

namespace IdentityService.Infrastructure.Repositories;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly IdentityDbContext _context;

    public OrganizationRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Organizations
            .Include(o => o.ParentOrganization)
            .Include(o => o.SubOrganizations)
            .Include(o => o.Subscription)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Organization?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Organizations
            .FirstOrDefaultAsync(o => o.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Organization>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Organizations
            .Include(o => o.ParentOrganization)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Organization>> GetByParentAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        return await _context.Organizations
            .Where(o => o.ParentOrganizationId == parentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Organization> AddAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync(cancellationToken);
        return organization;
    }

    public async Task UpdateAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        _context.Organizations.Update(organization);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var organization = await GetByIdAsync(id, cancellationToken);
        if (organization != null)
        {
            _context.Organizations.Remove(organization);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Organizations.AnyAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Organization>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _context.Organizations
            .Where(o => o.Name.Contains(searchTerm) ||
                       (o.Description != null && o.Description.Contains(searchTerm)))
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Organization> Organizations, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Organizations.AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);
        var organizations = await query
            .Include(o => o.ParentOrganization)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (organizations, totalCount);
    }
}