using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Pagination;
using IdentityService.Contracts.Common;
using IdentityService.Contracts.Organizations;
using MediatR;

namespace IdentityService.Application.Organizations.Queries;

// Organization Retrieval Queries
public record GetOrganizationByIdQuery(
    Guid OrganizationId,
    bool IncludeMembers = false,
    bool IncludeSettings = false,
    Guid? RequestedBy = null
) : IRequest<Result<OrganizationDto>>;

public record GetOrganizationDetailsQuery(
    Guid OrganizationId,
    Guid? RequestedBy = null
) : IRequest<Result<OrganizationDetailsResponse>>;

public record GetOrganizationsQuery(
    string? SearchTerm = null,
    string? Status = null,
    string? Type = null,
    Guid? ParentOrganizationId = null,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "Name",
    string SortDirection = "ASC",
    Guid? RequestedBy = null
) : IRequest<Result<OrganizationListResponse>>;

public record GetOrganizationByDomainQuery(
    string Domain,
    Guid? RequestedBy = null
) : IRequest<Result<OrganizationDto>>;

public record GetUserOrganizationsQuery(
    Guid UserId,
    bool IncludeInactive = false,
    Guid? RequestedBy = null
) : IRequest<Result<IEnumerable<OrganizationDto>>>;

// Organization Members Queries
public record GetOrganizationMembersQuery(
    Guid OrganizationId,
    string? SearchTerm = null,
    string? Role = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "JoinedAt",
    string SortDirection = "DESC",
    Guid? RequestedBy = null
) : IRequest<Result<PageResult<UserDto>>>;

public record GetOrganizationMemberQuery(
    Guid OrganizationId,
    Guid UserId,
    Guid? RequestedBy = null
) : IRequest<Result<UserDto>>;

public record GetOrganizationRolesQuery(
    Guid OrganizationId,
    Guid? RequestedBy = null
) : IRequest<Result<IEnumerable<RoleDto>>>;

public record GetOrganizationPermissionsQuery(
    Guid OrganizationId,
    Guid? RequestedBy = null
) : IRequest<Result<IEnumerable<PermissionDto>>>;

// Organization Settings Queries
public record GetOrganizationSettingsQuery(
    Guid OrganizationId,
    Guid? RequestedBy = null
) : IRequest<Result<OrganizationSettingsDto>>;

public record GetOrganizationBrandingQuery(
    Guid OrganizationId,
    Guid? RequestedBy = null
) : IRequest<Result<OrganizationBrandingDto>>;

public record GetOrganizationDomainsQuery(
    Guid OrganizationId,
    Guid? RequestedBy = null
) : IRequest<Result<IEnumerable<OrganizationDomainDto>>>;

// Organization Statistics Queries
public record GetOrganizationStatisticsQuery(
    Guid OrganizationId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    Guid? RequestedBy = null
) : IRequest<Result<OrganizationStatisticsDto>>;

public record GetOrganizationUsageQuery(
    Guid OrganizationId,
    string Period = "Month", // Day, Week, Month, Year
    int Count = 12,
    Guid? RequestedBy = null
) : IRequest<Result<IEnumerable<OrganizationUsageDto>>>;

public record GetOrganizationActivityQuery(
    Guid OrganizationId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string[]? ActivityTypes = null,
    int Page = 1,
    int PageSize = 50,
    Guid? RequestedBy = null
) : IRequest<Result<PageResult<OrganizationActivityDto>>>;

// Organization Hierarchy Queries
public record GetOrganizationHierarchyQuery(
    Guid? RootOrganizationId = null,
    int MaxDepth = 5,
    Guid? RequestedBy = null
) : IRequest<Result<OrganizationHierarchyDto>>;

public record GetChildOrganizationsQuery(
    Guid ParentOrganizationId,
    bool Recursive = false,
    Guid? RequestedBy = null
) : IRequest<Result<IEnumerable<OrganizationDto>>>;

public record GetParentOrganizationQuery(
    Guid OrganizationId,
    Guid? RequestedBy = null
) : IRequest<Result<OrganizationDto>>;

// Organization Invitations Queries
public record GetOrganizationInvitationsQuery(
    Guid OrganizationId,
    string? Status = null, // Pending, Accepted, Expired, Cancelled
    int Page = 1,
    int PageSize = 20,
    Guid? RequestedBy = null
) : IRequest<Result<PageResult<OrganizationInvitationDto>>>;

public record GetUserInvitationsQuery(
    Guid UserId,
    string? Status = null,
    Guid? RequestedBy = null
) : IRequest<Result<IEnumerable<OrganizationInvitationDto>>>;

public record ValidateInviteTokenQuery(
    string InviteToken
) : IRequest<Result<OrganizationInvitationDto>>;

// Organization Search and Discovery Queries
public record SearchOrganizationsQuery(
    string SearchTerm,
    string[]? Types = null,
    string[]? Statuses = null,
    bool? IsPublic = null,
    int Page = 1,
    int PageSize = 20,
    Guid? RequestedBy = null
) : IRequest<Result<PageResult<OrganizationDto>>>;

public record GetPublicOrganizationsQuery(
    int Page = 1,
    int PageSize = 20,
    string SortBy = "Name",
    Guid? RequestedBy = null
) : IRequest<Result<PageResult<OrganizationDto>>>;

// Organization Compliance and Audit Queries
public record GetOrganizationAuditLogsQuery(
    Guid OrganizationId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string[]? EventTypes = null,
    Guid? UserId = null,
    int Page = 1,
    int PageSize = 50,
    Guid? RequestedBy = null
) : IRequest<Result<PageResult<AuditLogDto>>>;

public record GetOrganizationComplianceStatusQuery(
    Guid OrganizationId,
    string[]? ComplianceTypes = null,
    Guid? RequestedBy = null
) : IRequest<Result<OrganizationComplianceDto>>;

// Validation and Check Queries
public record CheckOrganizationNameAvailabilityQuery(
    string Name,
    Guid? ExcludeOrganizationId = null
) : IRequest<Result<bool>>;

public record CheckOrganizationDomainAvailabilityQuery(
    string Domain,
    Guid? ExcludeOrganizationId = null
) : IRequest<Result<bool>>;

public record CheckUserOrganizationAccessQuery(
    Guid UserId,
    Guid OrganizationId,
    string? RequiredPermission = null
) : IRequest<Result<bool>>;

public record ValidateOrganizationLimitsQuery(
    Guid OrganizationId,
    string LimitType, // Users, Storage, ApiCalls, etc.
    int RequestedAmount = 1
) : IRequest<Result<OrganizationLimitValidationDto>>;

// Additional DTOs for query responses
public record OrganizationBrandingDto(
    string? Logo,
    string? PrimaryColor,
    string? SecondaryColor,
    string? Theme,
    Dictionary<string, object> CustomBranding
);

public record OrganizationDomainDto(
    string Domain,
    bool IsVerified,
    DateTime AddedAt,
    DateTime? VerifiedAt,
    string VerificationMethod,
    string? VerificationToken
);

public record OrganizationStatisticsDto(
    int TotalUsers,
    int ActiveUsers,
    int PendingInvitations,
    int TotalRoles,
    int TotalPermissions,
    DateTime LastActivity,
    Dictionary<string, int> UsersByRole,
    Dictionary<string, object> CustomMetrics
);

public record OrganizationUsageDto(
    DateTime Period,
    int ActiveUsers,
    int ApiCalls,
    long StorageUsed,
    int LoginAttempts,
    Dictionary<string, object> CustomUsage
);

public record OrganizationActivityDto(
    Guid Id,
    string ActivityType,
    string Description,
    Guid UserId,
    string UserName,
    DateTime Timestamp,
    Dictionary<string, object> Metadata
);

public record OrganizationHierarchyDto(
    OrganizationDto Organization,
    IEnumerable<OrganizationHierarchyDto> Children
);

public record OrganizationInvitationDto(
    Guid Id,
    string Email,
    string Role,
    string Status,
    string? Message,
    DateTime InvitedAt,
    DateTime ExpiresAt,
    Guid InvitedBy,
    string InvitedByName,
    string? InviteToken
);

public record OrganizationComplianceDto(
    Dictionary<string, bool> ComplianceChecks,
    DateTime LastAudit,
    string OverallStatus,
    IEnumerable<string> RequiredActions
);

public record OrganizationLimitValidationDto(
    bool IsValid,
    int CurrentUsage,
    int Limit,
    int Available,
    string? WarningMessage
);