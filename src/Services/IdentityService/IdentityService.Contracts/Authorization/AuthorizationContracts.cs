using IdentityService.Contracts.Common;

namespace IdentityService.Contracts.Roles;

public record CreateRoleRequest(
    string Name,
    string? Description = null,
    string Type = "Custom",
    Guid? OrganizationId = null,
    IEnumerable<Guid>? PermissionIds = null
);

public record UpdateRoleRequest(
    string? Name = null,
    string? Description = null,
    bool? IsActive = null
);

public record AssignPermissionToRoleRequest(
    Guid PermissionId,
    Dictionary<string, object>? Conditions = null
);

public record RemovePermissionFromRoleRequest(
    Guid PermissionId
);

public record RoleSearchRequest(
    string? SearchTerm = null,
    string? Type = null,
    Guid? OrganizationId = null,
    bool? IsSystemRole = null,
    int Page = 1,
    int PageSize = 20
);

public record RoleListResponse(
    IEnumerable<RoleDto> Roles,
    int TotalCount,
    int Page,
    int PageSize
);

public record RoleDetailsResponse(
    RoleDto Role,
    IEnumerable<PermissionDto> Permissions,
    IEnumerable<UserDto> Users
);