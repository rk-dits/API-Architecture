using IdentityService.Contracts.Common;

namespace IdentityService.Contracts.Permissions;

public record CreatePermissionRequest(
    string Name,
    string? Description = null,
    string Resource = "",
    string Action = "",
    string Type = "Custom",
    Dictionary<string, object>? Conditions = null
);

public record UpdatePermissionRequest(
    string? Name = null,
    string? Description = null,
    bool? IsActive = null,
    Dictionary<string, object>? Conditions = null
);

public record PermissionSearchRequest(
    string? SearchTerm = null,
    string? Type = null,
    string? Resource = null,
    bool? IsSystemPermission = null,
    int Page = 1,
    int PageSize = 20
);

public record PermissionListResponse(
    IEnumerable<PermissionDto> Permissions,
    int TotalCount,
    int Page,
    int PageSize
);

public record CheckPermissionRequest(
    Guid UserId,
    string Permission,
    Guid? OrganizationId = null,
    Dictionary<string, object>? Context = null
);

public record CheckPermissionResponse(
    bool HasPermission,
    string? Reason = null
);