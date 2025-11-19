using IdentityService.Contracts.Organizations;

namespace IdentityService.Contracts.Common;

// Base contracts for common data transfer objects
public record BaseDto(Guid Id);

public record AuditableDto(
    Guid Id,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy) : BaseDto(Id);

public record PaginatedRequest(
    int Page = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    string? SortBy = null,
    bool SortDescending = false);

public record PaginatedResponse<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int CurrentPage,
    int PageSize,
    int TotalPages) where T : class;

// Error handling contracts
public record ErrorResponse(
    string Code,
    string Message,
    Dictionary<string, string[]>? Errors = null,
    string? Details = null);

public record ValidationErrorResponse(
    string Message,
    Dictionary<string, string[]> ValidationErrors) : ErrorResponse("ValidationError", Message, ValidationErrors);

// DTOs referenced in other contracts
public record UserSessionDto(
    Guid Id,
    string SessionId,
    Guid UserId,
    DateTime StartedAt,
    DateTime LastAccessedAt,
    DateTime? EndedAt,
    string IpAddress,
    string UserAgent,
    string Status,
    bool IsMfaVerified);

public record UserDto(
    Guid Id,
    string Email,
    string? UserName,
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool PhoneNumberConfirmed,
    bool TwoFactorEnabled,
    bool LockoutEnabled,
    DateTimeOffset? LockoutEnd,
    int AccessFailedCount,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy);

public record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy);

public record PermissionDto(
    Guid Id,
    string Name,
    string? Description,
    string Resource,
    string Action,
    string? Conditions,
    bool IsActive,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy);

public record OrganizationDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentOrganizationId,
    string Status,
    string Type,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy)
{
    public string? Website { get; init; }
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
    public bool IsActive { get; init; }
    public int MemberCount { get; init; }
    public OrganizationSettingsDto? Settings { get; init; }
};

public record AuditLogDto(
    Guid Id,
    string EventType,
    string? EntityType,
    string? EntityId,
    Guid? UserId,
    string? UserName,
    Guid? OrganizationId,
    string? Action,
    string? Details,
    string? IpAddress,
    string? UserAgent,
    DateTime Timestamp);