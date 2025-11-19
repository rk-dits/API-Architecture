using MediatR;
using IdentityService.Contracts.Users;
using IdentityService.Contracts.Common;

namespace IdentityService.Application.Users.Queries;

public record GetUserByIdQuery(
    Guid UserId
) : IRequest<UserDto?>;

public record GetUserByEmailQuery(
    string Email
) : IRequest<UserDto?>;

public record GetUserByUsernameQuery(
    string Username
) : IRequest<UserDto?>;

public record GetUserDetailsQuery(
    Guid UserId
) : IRequest<UserDetailsResponse?>;

public record SearchUsersQuery(
    string? SearchTerm = null,
    string? Status = null,
    Guid? OrganizationId = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<UserListResponse>;

public record GetUserRolesQuery(
    Guid UserId,
    Guid? OrganizationId = null
) : IRequest<IEnumerable<RoleDto>>;

public record GetUserPermissionsQuery(
    Guid UserId,
    Guid? OrganizationId = null
) : IRequest<IEnumerable<PermissionDto>>;

public record GetUserOrganizationsQuery(
    Guid UserId
) : IRequest<IEnumerable<OrganizationDto>>;

public record GetUserSessionsQuery(
    Guid UserId,
    bool ActiveOnly = true
) : IRequest<IEnumerable<UserSessionDto>>;