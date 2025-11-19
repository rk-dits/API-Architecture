using BuildingBlocks.Common.Abstractions;
using IdentityService.Contracts.Common;
using IdentityService.Contracts.Organizations;
using MediatR;

namespace IdentityService.Application.Organizations.Commands;

// Organization Management Commands
public record CreateOrganizationCommand(
    string Name,
    string? Description = null,
    string? Website = null,
    string? ContactEmail = null,
    string? ContactPhone = null,
    Guid? ParentOrganizationId = null,
    string Type = "Enterprise",
    Guid? CreatedBy = null
) : IRequest<Result<OrganizationDto>>;

public record UpdateOrganizationCommand(
    Guid OrganizationId,
    string? Name = null,
    string? Description = null,
    string? Logo = null,
    string? Website = null,
    string? ContactEmail = null,
    string? ContactPhone = null,
    Guid? UpdatedBy = null
) : IRequest<Result<OrganizationDto>>;

public record DeleteOrganizationCommand(
    Guid OrganizationId,
    Guid? DeletedBy = null
) : IRequest<Result<bool>>;

public record DeactivateOrganizationCommand(
    Guid OrganizationId,
    string? Reason = null,
    Guid? DeactivatedBy = null
) : IRequest<Result<bool>>;

public record ActivateOrganizationCommand(
    Guid OrganizationId,
    Guid? ActivatedBy = null
) : IRequest<Result<bool>>;

// User Organization Management Commands
public record InviteUserToOrganizationCommand(
    Guid OrganizationId,
    string Email,
    string Role,
    string? Message = null,
    Guid? InvitedBy = null
) : IRequest<Result<bool>>;

public record AcceptOrganizationInviteCommand(
    string InviteToken,
    string? FirstName = null,
    string? LastName = null,
    string? Password = null,
    Guid? UserId = null
) : IRequest<Result<bool>>;

public record RemoveUserFromOrganizationCommand(
    Guid OrganizationId,
    Guid UserId,
    Guid? RemovedBy = null
) : IRequest<Result<bool>>;

public record UpdateUserOrganizationRoleCommand(
    Guid OrganizationId,
    Guid UserId,
    string Role,
    Guid? UpdatedBy = null
) : IRequest<Result<bool>>;

// Organization Settings Commands
public record UpdateOrganizationSettingsCommand(
    Guid OrganizationId,
    bool? RequireMfa = null,
    bool? AllowSso = null,
    bool? AllowGuestUsers = null,
    int? MaxUsers = null,
    int? SessionTimeoutMinutes = null,
    IEnumerable<string>? AllowedDomains = null,
    Dictionary<string, object>? CustomSettings = null,
    Guid? UpdatedBy = null
) : IRequest<Result<bool>>;

public record UpdateOrganizationBrandingCommand(
    Guid OrganizationId,
    string? Logo = null,
    string? PrimaryColor = null,
    string? SecondaryColor = null,
    string? Theme = null,
    Guid? UpdatedBy = null
) : IRequest<Result<bool>>;

// Domain Management Commands
public record AddOrganizationDomainCommand(
    Guid OrganizationId,
    string Domain,
    bool RequireVerification = true,
    Guid? AddedBy = null
) : IRequest<Result<bool>>;

public record VerifyOrganizationDomainCommand(
    Guid OrganizationId,
    string Domain,
    string VerificationMethod = "TXT",
    Guid? VerifiedBy = null
) : IRequest<Result<bool>>;

public record RemoveOrganizationDomainCommand(
    Guid OrganizationId,
    string Domain,
    Guid? RemovedBy = null
) : IRequest<Result<bool>>;

// Bulk Operations Commands
public record BulkInviteUsersCommand(
    Guid OrganizationId,
    IEnumerable<OrganizationInviteRequest> Invitations,
    Guid? InvitedBy = null
) : IRequest<Result<BulkOperationResult>>;

public record BulkUpdateUserRolesCommand(
    Guid OrganizationId,
    IEnumerable<(Guid UserId, string Role)> UserRoles,
    Guid? UpdatedBy = null
) : IRequest<Result<BulkOperationResult>>;

// Transfer and Merge Commands
public record TransferOrganizationOwnershipCommand(
    Guid OrganizationId,
    Guid NewOwnerId,
    Guid CurrentOwnerId
) : IRequest<Result<bool>>;

public record MergeOrganizationsCommand(
    Guid SourceOrganizationId,
    Guid TargetOrganizationId,
    bool TransferUsers = true,
    bool TransferSettings = false,
    Guid? MergedBy = null
) : IRequest<Result<bool>>;

// Backup and Export Commands
public record ExportOrganizationDataCommand(
    Guid OrganizationId,
    string Format = "JSON",
    string[]? DataTypes = null,
    Guid? RequestedBy = null
) : IRequest<Result<string>>;

public record BackupOrganizationCommand(
    Guid OrganizationId,
    string BackupType = "Full",
    Guid? RequestedBy = null
) : IRequest<Result<string>>;

// Audit and Compliance Commands
public record GenerateOrganizationAuditReportCommand(
    Guid OrganizationId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string[]? EventTypes = null,
    Guid? RequestedBy = null
) : IRequest<Result<string>>;

public record UpdateOrganizationComplianceSettingsCommand(
    Guid OrganizationId,
    Dictionary<string, object> ComplianceSettings,
    Guid? UpdatedBy = null
) : IRequest<Result<bool>>;

// Result types for bulk operations
public record BulkOperationResult(
    int TotalItems,
    int SuccessfulItems,
    int FailedItems,
    IEnumerable<BulkOperationError> Errors
);

public record BulkOperationError(
    string ItemId,
    string ErrorMessage,
    string ErrorCode
);