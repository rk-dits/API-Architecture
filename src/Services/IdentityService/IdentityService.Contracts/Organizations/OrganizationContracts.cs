using IdentityService.Contracts.Common;

namespace IdentityService.Contracts.Organizations;

public record CreateOrganizationRequest(
    string Name,
    string? Description = null,
    string? Website = null,
    string? ContactEmail = null,
    string? ContactPhone = null,
    Guid? ParentOrganizationId = null,
    string Type = "Enterprise"
);

public record UpdateOrganizationRequest(
    string? Name = null,
    string? Description = null,
    string? Logo = null,
    string? Website = null,
    string? ContactEmail = null,
    string? ContactPhone = null
);

public record OrganizationInviteRequest(
    string Email,
    string Role,
    string? Message = null
);

public record AcceptInviteRequest(
    string InviteToken,
    string? FirstName = null,
    string? LastName = null,
    string? Password = null
);

public record UpdateUserOrganizationRoleRequest(
    string Role
);

public record OrganizationSearchRequest(
    string? SearchTerm = null,
    string? Status = null,
    string? Type = null,
    int Page = 1,
    int PageSize = 20
);

public record OrganizationListResponse(
    IEnumerable<OrganizationDto> Organizations,
    int TotalCount,
    int Page,
    int PageSize
);

public record OrganizationDetailsResponse(
    OrganizationDto Organization,
    IEnumerable<UserDto> Members,
    IEnumerable<RoleDto> Roles,
    IEnumerable<PermissionDto> Permissions,
    OrganizationSettingsDto Settings
);

public record OrganizationSettingsDto(
    bool RequireMfa,
    bool AllowSso,
    bool AllowGuestUsers,
    int MaxUsers,
    int SessionTimeoutMinutes,
    IEnumerable<string> AllowedDomains,
    Dictionary<string, object> CustomSettings
);

public record UpdateOrganizationSettingsRequest(
    bool? RequireMfa = null,
    bool? AllowSso = null,
    bool? AllowGuestUsers = null,
    int? MaxUsers = null,
    int? SessionTimeoutMinutes = null,
    IEnumerable<string>? AllowedDomains = null,
    Dictionary<string, object>? CustomSettings = null
);