using Common.Entities;

namespace IdentityService.Domain.Entities;

public class Permission : Entity<Guid>, IAuditableEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public PermissionType Type { get; set; }
    public bool IsSystemPermission { get; set; }
    public bool IsActive { get; set; } = true;

    // Hierarchical permissions
    public Guid? ParentPermissionId { get; set; }
    public Permission? ParentPermission { get; set; }
    public virtual ICollection<Permission> ChildPermissions { get; set; } = new List<Permission>();

    // ABAC Support
    public Dictionary<string, object> Conditions { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();

    // Audit
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    public virtual ICollection<OrganizationPermission> OrganizationPermissions { get; set; } = new List<OrganizationPermission>();

    public string FullName => $"{Resource}:{Action}";

    public bool MatchesConditions(Dictionary<string, object> context)
    {
        if (Conditions.Count == 0) return true;

        foreach (var condition in Conditions)
        {
            if (!context.TryGetValue(condition.Key, out var value) ||
                !value.Equals(condition.Value))
            {
                return false;
            }
        }

        return true;
    }
}

public enum PermissionType
{
    System,
    Application,
    Resource,
    Custom
}

public class RolePermission : Entity<Guid>
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;

    public DateTime GrantedAt { get; set; }
    public string GrantedBy { get; set; } = string.Empty;
    public Dictionary<string, object> Conditions { get; set; } = new();
}

public class UserPermission : Entity<Guid>
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;

    public Guid? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    public DateTime GrantedAt { get; set; }
    public string GrantedBy { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public Dictionary<string, object> Conditions { get; set; } = new();

    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt;
}

public class OrganizationPermission : Entity<Guid>
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;

    public DateTime GrantedAt { get; set; }
    public string GrantedBy { get; set; } = string.Empty;
    public Dictionary<string, object> Conditions { get; set; } = new();
}