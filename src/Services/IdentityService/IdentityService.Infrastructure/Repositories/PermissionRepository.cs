using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Data;

namespace IdentityService.Infrastructure.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly IdentityDbContext _context;

    public PermissionRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Permissions.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetSystemPermissionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .Where(p => p.IsSystemPermission)
            .ToListAsync(cancellationToken);
    }

    public async Task<Permission> AddAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync(cancellationToken);
        return permission;
    }

    public async Task UpdateAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        _context.Permissions.Update(permission);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var permission = await GetByIdAsync(id, cancellationToken);
        if (permission != null)
        {
            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId, Guid? organizationId = null, CancellationToken cancellationToken = default)
    {
        // Get permissions from roles
        var rolePermissions = _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId && ur.IsActive && !ur.IsExpired)
            .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission);

        // Get direct permissions
        var directPermissionsQuery = _context.Set<UserPermission>()
            .Where(up => up.UserId == userId && up.IsActive && !up.IsExpired)
            .Include(up => up.Permission)
            .AsQueryable();

        if (organizationId.HasValue)
        {
            directPermissionsQuery = directPermissionsQuery
                .Where(up => up.OrganizationId == organizationId);
        }

        var directPermissions = directPermissionsQuery.Select(up => up.Permission);

        // Combine and return unique permissions
        return await rolePermissions
            .Union(directPermissions)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission)
            .ToListAsync(cancellationToken);
    }
}