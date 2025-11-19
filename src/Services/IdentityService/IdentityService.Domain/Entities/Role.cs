using Common.Entities;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Domain.Entities;

public class Role : IdentityRole<Guid>, IAuditableEntity, IAggregateRoot
{
    public string? Description { get; set; }
    public RoleType Type { get; set; }
    public RoleLevel Level { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; } = true;

    // Hierarchical roles
    public Guid? ParentRoleId { get; set; }
    public Role? ParentRole { get; set; }
    public virtual ICollection<Role> ChildRoles { get; set; } = new List<Role>();

    // Organization context
    public Guid? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    // Permissions
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    // Audit
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public void AddPermission(Permission permission)
    {
        if (!RolePermissions.Any(rp => rp.PermissionId == permission.Id))
        {
            RolePermissions.Add(new RolePermission { RoleId = this.Id, PermissionId = permission.Id });
        }
    }

    public void RemovePermission(Permission permission)
    {
        var rolePermission = RolePermissions.FirstOrDefault(rp => rp.PermissionId == permission.Id);
        if (rolePermission != null)
        {
            RolePermissions.Remove(rolePermission);
        }
    }
}

public enum RoleType
{
    System,
    Organization,
    Custom
}

public enum RoleLevel
{
    SuperAdmin = 0,
    PlatformAdmin = 1,
    OrganizationAdmin = 2,
    Manager = 3,
    User = 4,
    Guest = 5
}

public class UserRole : Entity<Guid>
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public Guid? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    public DateTime AssignedAt { get; set; }
    public string AssignedBy { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt;
}

public class OrganizationRole : Entity<Guid>
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}