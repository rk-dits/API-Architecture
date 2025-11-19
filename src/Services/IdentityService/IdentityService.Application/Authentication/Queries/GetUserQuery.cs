using MediatR;
using IdentityService.Contracts.Authentication;

namespace IdentityService.Application.Authentication.Queries;

public record GetUserQuery(Guid UserId) : IRequest<UserDto?>;