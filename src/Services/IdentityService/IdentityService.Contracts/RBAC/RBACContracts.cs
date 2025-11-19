using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Common.Pagination;

namespace IdentityService.Contracts.RBAC;

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

public record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    RoleType Type,
    RoleLevel Level,
    bool IsSystemRole,
    bool IsActive,
    Guid? ParentRoleId,
    string? ParentRoleName,
    Guid? OrganizationId,
    string? OrganizationName,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy,
    IEnumerable<PermissionDto> Permissions
);

public record PermissionDto(
    Guid Id,
    string Name,
    string? Description,
    string? Category,
    Guid? ParentPermissionId,
    string? ParentPermissionName,
    Dictionary<string, object>? Conditions,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy
);

public record RoleHierarchyDto(
    Guid Id,
    string Name,
    RoleLevel Level,
    IEnumerable<RoleHierarchyDto> Children
);

public record PermissionHierarchyDto(
    Guid Id,
    string Name,
    string? Category,
    IEnumerable<PermissionHierarchyDto> Children
);



// Request DTOs
public record CreateRoleRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; init; }

    [Required]
    public RoleType Type { get; init; }

    [Required]
    public RoleLevel Level { get; init; }

    public Guid? OrganizationId { get; init; }
    public Guid? ParentRoleId { get; init; }
    public IReadOnlyCollection<Guid> PermissionIds { get; init; } = Array.Empty<Guid>();
}

public record UpdateRoleRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; init; }

    [Required]
    public RoleType Type { get; init; }

    [Required]
    public RoleLevel Level { get; init; }

    public Guid? ParentRoleId { get; init; }
    public IReadOnlyCollection<Guid> PermissionIds { get; init; } = Array.Empty<Guid>();
}

public record CreatePermissionRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; init; }

    [StringLength(50)]
    public string? Category { get; init; }

    public Guid? ParentPermissionId { get; init; }
    public Dictionary<string, object>? Conditions { get; init; }
}

public record UpdatePermissionRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; init; }

    [StringLength(50)]
    public string? Category { get; init; }

    public Guid? ParentPermissionId { get; init; }
    public Dictionary<string, object>? Conditions { get; init; }
}

public record AssignRoleRequest
{
    [Required]
    public Guid UserId { get; init; }

    [Required]
    public Guid RoleId { get; init; }

    public Guid? OrganizationId { get; init; }
    public DateTime? ExpiresAt { get; init; }
}

public record AssignPermissionRequest
{
    [Required]
    public Guid UserId { get; init; }

    [Required]
    public Guid PermissionId { get; init; }

    public Guid? OrganizationId { get; init; }
    public DateTime? ExpiresAt { get; init; }
}