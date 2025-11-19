using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Pagination;
using IdentityService.Contracts.RBAC;
using MediatR;

namespace IdentityService.Application.RBAC.Queries;

public record GetPermissionQuery(Guid Id) : IRequest<PermissionDto?>;

public record GetPermissionsQuery(
    string? Category = null,
    Guid? ParentPermissionId = null,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PageResult<PermissionDto>>;

public record GetUserPermissionsQuery(
    Guid UserId,
    Guid? OrganizationId = null
) : IRequest<IEnumerable<PermissionDto>>;

public record GetPermissionHierarchyQuery(Guid? RootPermissionId = null) : IRequest<IEnumerable<PermissionHierarchyDto>>;

public record SearchPermissionsQuery(
    string SearchTerm,
    string? Category = null,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PageResult<PermissionDto>>;