using MediatR;
using IdentityService.Contracts.Authentication;

namespace IdentityService.Application.Authentication.Commands;

public record LoginCommand(
    string Username,
    string Password,
    string? MfaCode = null,
    bool RememberMe = false,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<LoginResponse>;

public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<RefreshTokenResponse>;

public record LogoutCommand(
    string? RefreshToken = null,
    string? SessionId = null
) : IRequest<bool>;

public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest<bool>;

public record ForgotPasswordCommand(
    string Email
) : IRequest<bool>;

public record ResetPasswordCommand(
    string Token,
    string NewPassword
) : IRequest<bool>;

public record VerifyMfaCommand(
    Guid UserId,
    string Code,
    string? SessionId = null
) : IRequest<bool>;

public record SetupMfaCommand(
    Guid UserId,
    string Method
) : IRequest<SetupMfaResponse>;

public record DisableMfaCommand(
    Guid UserId,
    string Password,
    string? MfaCode = null
) : IRequest<bool>;

public record VerifyEmailCommand(
    string Token
) : IRequest<bool>;

public record ResendVerificationEmailCommand(
    string Email
) : IRequest<bool>;