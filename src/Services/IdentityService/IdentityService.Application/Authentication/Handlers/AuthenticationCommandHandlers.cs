using MediatR;
using IdentityService.Contracts.Authentication;
using IdentityService.Contracts.Common;
using IdentityService.Application.Authentication.Commands;
using IdentityService.Domain.Services;
using IdentityService.Domain.Repositories;
using IdentityService.Domain.Entities;
using IdentityService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Authentication.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthenticationService _authService;
    private readonly IMfaService _mfaService;
    private readonly ISessionService _sessionService;
    private readonly IAuditService _auditService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IAuthenticationService authService,
        IMfaService mfaService,
        ISessionService sessionService,
        IAuditService auditService,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _authService = authService;
        _mfaService = mfaService;
        _sessionService = sessionService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing login request for user: {Username}", request.Username);

            var (success, token, user) = await _authService.AuthenticateAsync(
                request.Username, request.Password, cancellationToken);

            if (!success || user == null)
            {
                await _auditService.LogSecurityEventAsync(
                    null, "LoginFailed", $"Failed login attempt for {request.Username}",
                    request.IpAddress ?? "", request.UserAgent ?? "", cancellationToken);

                return new LoginResponse(false, null, null, null, null, false, null, "Invalid credentials");
            }

            // Check if user is locked or inactive
            if (!user.IsActive)
            {
                await _auditService.LogSecurityEventAsync(
                    user.Id, "LoginBlocked", "Login attempt for inactive user",
                    request.IpAddress ?? "", request.UserAgent ?? "", cancellationToken);

                return new LoginResponse(false, null, null, null, null, false, null, "Account is inactive");
            }

            if (user.IsLocked)
            {
                await _auditService.LogSecurityEventAsync(
                    user.Id, "LoginBlocked", "Login attempt for locked user",
                    request.IpAddress ?? "", request.UserAgent ?? "", cancellationToken);

                return new LoginResponse(false, null, null, null, null, false, null, "Account is locked");
            }

            // Check MFA requirement
            if (user.IsMfaEnabled && string.IsNullOrEmpty(request.MfaCode))
            {
                return new LoginResponse(false, null, null, null, null, true, user.PreferredMfaMethod.ToString(),
                    "MFA verification required");
            }

            // Verify MFA if provided
            if (user.IsMfaEnabled && !string.IsNullOrEmpty(request.MfaCode))
            {
                var mfaValid = await _mfaService.VerifyMfaAsync(user.Id, request.MfaCode, cancellationToken);
                if (!mfaValid)
                {
                    await _auditService.LogSecurityEventAsync(
                        user.Id, "MfaFailed", "Invalid MFA code provided",
                        request.IpAddress ?? "", request.UserAgent ?? "", cancellationToken);

                    return new LoginResponse(false, null, null, null, null, true, user.PreferredMfaMethod.ToString(),
                        "Invalid MFA code");
                }
            }

            // Create session
            var session = await _sessionService.CreateSessionAsync(
                user, request.IpAddress ?? "", request.UserAgent ?? "", cancellationToken);

            // Record successful login
            user.RecordSuccessfulLogin();
            await _userRepository.UpdateAsync(user, cancellationToken);

            await _auditService.LogUserActionAsync(
                user.Id, AuditEventType.UserLogin, "User", user.Id.ToString(),
                "Successful login", cancellationToken);

            var userDto = new Contracts.Authentication.UserDto(
                user.Id, user.UserName!, user.Email!, user.FirstName!, user.LastName!,
                user.ProfilePicture, user.Status.ToString(), user.IsEmailVerified, user.IsPhoneVerified,
                user.LastLoginAt, user.IsMfaEnabled, user.PreferredMfaMethod.ToString(),
                user.DefaultOrganizationId, user.CreatedAt);

            return new LoginResponse(true, session.AccessToken, session.RefreshToken,
                session.AccessTokenExpiry, userDto, false, null, "Login successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing login for user: {Username}", request.Username);
            throw;
        }
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IAuthenticationService authService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var (success, token) = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);

            if (!success)
            {
                return new RefreshTokenResponse(false, null, null, null, "Invalid refresh token");
            }

            // Calculate expiry (typically 1 hour for access tokens)
            var expiresAt = DateTime.UtcNow.AddHours(1);

            return new RefreshTokenResponse(true, token, request.RefreshToken, expiresAt, "Token refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return new RefreshTokenResponse(false, null, null, null, "Token refresh failed");
        }
    }
}

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly ISessionService _sessionService;
    private readonly IAuthenticationService _authService;
    private readonly IAuditService _auditService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        ISessionService sessionService,
        IAuthenticationService authService,
        IAuditService auditService,
        ILogger<LogoutCommandHandler> logger)
    {
        _sessionService = sessionService;
        _authService = authService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!string.IsNullOrEmpty(request.SessionId))
            {
                var session = await _sessionService.GetSessionAsync(request.SessionId, cancellationToken);
                if (session != null)
                {
                    await _auditService.LogUserActionAsync(
                        session.UserId, AuditEventType.UserLogout, "User", session.UserId.ToString(),
                        "User logged out", cancellationToken);
                }

                await _sessionService.EndSessionAsync(request.SessionId, cancellationToken);
            }

            if (!string.IsNullOrEmpty(request.RefreshToken))
            {
                await _authService.RevokeTokenAsync(request.RefreshToken, cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing logout");
            return false;
        }
    }
}