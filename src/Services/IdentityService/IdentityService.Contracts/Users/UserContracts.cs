using IdentityService.Contracts.Common;

namespace IdentityService.Contracts.Users;

public record CreateUserRequest(
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string? Password = null,
    bool SendWelcomeEmail = true,
    Guid? OrganizationId = null
);

public record UpdateUserRequest(
    string? FirstName = null,
    string? LastName = null,
    string? Email = null,
    string? ProfilePicture = null
);

public record UpdateUserStatusRequest(
    string Status,
    string? Reason = null
);

public record AssignRoleRequest(
    Guid RoleId,
    Guid? OrganizationId = null,
    DateTime? ExpiresAt = null
);

public record RemoveRoleRequest(
    Guid RoleId,
    Guid? OrganizationId = null
);

public record GrantPermissionRequest(
    Guid PermissionId,
    Guid? OrganizationId = null,
    DateTime? ExpiresAt = null,
    Dictionary<string, object>? Conditions = null
);

public record RevokePermissionRequest(
    Guid PermissionId,
    Guid? OrganizationId = null
);

public record UserSearchRequest(
    string? SearchTerm = null,
    string? Status = null,
    Guid? OrganizationId = null,
    int Page = 1,
    int PageSize = 20
);

public record UserListResponse(
    IEnumerable<UserDto> Users,
    int TotalCount,
    int Page,
    int PageSize
);

public record UserDetailsResponse(
    UserDto User,
    IEnumerable<RoleDto> Roles,
    IEnumerable<PermissionDto> Permissions,
    IEnumerable<OrganizationDto> Organizations,
    IEnumerable<UserSessionDto> ActiveSessions
);