using MediatR;
using IdentityService.Application.Authentication.Queries;
using IdentityService.Contracts.Authentication;
using IdentityService.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Authentication.Handlers;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserQueryHandler> _logger;

    public GetUserQueryHandler(IUserRepository userRepository, ILogger<GetUserQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserDto?> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving user {UserId}", request.UserId);

            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return null;
            }

            return new UserDto(
                user.Id,
                user.UserName ?? string.Empty,
                user.Email ?? string.Empty,
                user.FirstName,
                user.LastName,
                user.ProfilePicture,
                user.Status.ToString(),
                user.IsEmailVerified,
                user.IsPhoneVerified,
                user.LastLoginAt,
                user.IsMfaEnabled,
                user.PreferredMfaMethod.ToString(),
                user.DefaultOrganizationId,
                user.CreatedAt
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", request.UserId);
            throw;
        }
    }
}