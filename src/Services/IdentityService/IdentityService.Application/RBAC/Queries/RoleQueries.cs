using BuildingBlocks.Common.Pagination;
using IdentityService.Contracts.RBAC;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Application.RBAC.Queries;

public record GetRoleQuery(Guid Id) : IRequest<RoleDto?>;

public record GetRolesQuery(
    Guid? OrganizationId = null,
    IdentityService.Domain.Entities.RoleType? Type = null,
    IdentityService.Domain.Entities.RoleLevel? Level = null,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PageResult<RoleDto>>;

public record GetUserRolesQuery(
    Guid UserId,
    Guid? OrganizationId = null
) : IRequest<IEnumerable<RoleDto>>;

public record GetRolePermissionsQuery(Guid RoleId) : IRequest<IEnumerable<PermissionDto>>;

public record GetRoleHierarchyQuery(Guid? RootRoleId = null) : IRequest<IEnumerable<RoleHierarchyDto>>;

public record CheckUserPermissionQuery(
    Guid UserId,
    string Permission,
    Guid? OrganizationId = null
) : IRequest<bool>;

public record CheckUserRoleQuery(
    Guid UserId,
    string RoleName,
    Guid? OrganizationId = null
) : IRequest<bool>;