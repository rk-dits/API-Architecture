using BuildingBlocks.Common.Abstractions;
using IdentityService.Contracts.RBAC;
using MediatR;

namespace IdentityService.Application.RBAC.Commands;

public record CreateRoleCommand(
    string Name,
    string? Description,
    RoleType Type,
    RoleLevel Level,
    Guid? OrganizationId,
    Guid? ParentRoleId,
    IReadOnlyCollection<Guid> PermissionIds
) : IRequest<Guid>;

public record UpdateRoleCommand(
    Guid Id,
    string Name,
    string? Description,
    RoleType Type,
    RoleLevel Level,
    Guid? ParentRoleId,
    IReadOnlyCollection<Guid> PermissionIds
) : IRequest<bool>;

public record DeleteRoleCommand(Guid Id) : IRequest<bool>;

public record AssignRoleToUserCommand(
    Guid UserId,
    Guid RoleId,
    Guid? OrganizationId,
    DateTime? ExpiresAt
) : IRequest<bool>;

public record RemoveRoleFromUserCommand(
    Guid UserId,
    Guid RoleId,
    Guid? OrganizationId
) : IRequest<bool>;

public record AssignPermissionToRoleCommand(
    Guid RoleId,
    Guid PermissionId
) : IRequest<bool>;

public record RemovePermissionFromRoleCommand(
    Guid RoleId,
    Guid PermissionId
) : IRequest<bool>;