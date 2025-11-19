using MediatR;
using IdentityService.Contracts.Users;

namespace IdentityService.Application.Users.Commands;

public record CreateUserCommand(
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string? Password = null,
    bool SendWelcomeEmail = true,
    Guid? OrganizationId = null,
    Guid? CreatedBy = null
) : IRequest<Guid>;

public record UpdateUserCommand(
    Guid UserId,
    string? FirstName = null,
    string? LastName = null,
    string? Email = null,
    string? ProfilePicture = null,
    Guid? UpdatedBy = null
) : IRequest<bool>;

public record UpdateUserStatusCommand(
    Guid UserId,
    string Status,
    string? Reason = null,
    Guid? UpdatedBy = null
) : IRequest<bool>;

public record DeleteUserCommand(
    Guid UserId,
    Guid? DeletedBy = null
) : IRequest<bool>;

public record AssignRoleCommand(
    Guid UserId,
    Guid RoleId,
    Guid? OrganizationId = null,
    DateTime? ExpiresAt = null,
    Guid? AssignedBy = null
) : IRequest<bool>;

public record RemoveRoleCommand(
    Guid UserId,
    Guid RoleId,
    Guid? OrganizationId = null,
    Guid? RemovedBy = null
) : IRequest<bool>;

public record GrantPermissionCommand(
    Guid UserId,
    Guid PermissionId,
    Guid? OrganizationId = null,
    DateTime? ExpiresAt = null,
    Dictionary<string, object>? Conditions = null,
    Guid? GrantedBy = null
) : IRequest<bool>;

public record RevokePermissionCommand(
    Guid UserId,
    Guid PermissionId,
    Guid? OrganizationId = null,
    Guid? RevokedBy = null
) : IRequest<bool>;