using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Application.RBAC.Commands;

public record CreatePermissionCommand(
    string Name,
    string? Description,
    string? Category,
    Guid? ParentPermissionId,
    Dictionary<string, object>? Conditions = null
) : IRequest<Guid>;

public record UpdatePermissionCommand(
    Guid Id,
    string Name,
    string? Description,
    string? Category,
    Guid? ParentPermissionId,
    Dictionary<string, object>? Conditions = null
) : IRequest<bool>;

public record DeletePermissionCommand(Guid Id) : IRequest<bool>;

public record AssignPermissionToUserCommand(
    Guid UserId,
    Guid PermissionId,
    Guid? OrganizationId,
    DateTime? ExpiresAt
) : IRequest<bool>;

public record RemovePermissionFromUserCommand(
    Guid UserId,
    Guid PermissionId,
    Guid? OrganizationId
) : IRequest<bool>;