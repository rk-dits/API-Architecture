using BuildingBlocks.Common.Pagination;
using IdentityService.Contracts.SuperAdmin;
using CommonContracts = IdentityService.Contracts.Common;
using MediatR;

namespace IdentityService.Application.SuperAdmin.Queries;

public record GetSystemStatsQuery() : IRequest<SystemStatsDto>;

public record GetSystemAuditLogsQuery(int PageNumber = 1, int PageSize = 10, string? UserFilter = null, string? ActionFilter = null, DateTime? FromDate = null, DateTime? ToDate = null) : IRequest<PageResult<CommonContracts.AuditLogDto>>;

public record GetSystemUsersQuery(int PageNumber = 1, int PageSize = 10, string? SearchTerm = null, bool? IsActive = null, Guid? OrganizationId = null) : IRequest<PageResult<CommonContracts.UserDto>>;

public record GetSystemOrganizationsQuery(int PageNumber = 1, int PageSize = 10, string? SearchTerm = null, bool? IsActive = null) : IRequest<PageResult<CommonContracts.OrganizationDto>>;

public record GetActiveSessionsQuery(int PageNumber = 1, int PageSize = 10, Guid? UserId = null) : IRequest<PageResult<CommonContracts.UserSessionDto>>;