using System.ComponentModel.DataAnnotations;
using IdentityService.Contracts.Validation;

namespace IdentityService.Contracts.Authentication;

public record LoginRequest(
    [Required(ErrorMessage = "Username is required")]
    [UsernameValidation]
    string Username,

    [Required(ErrorMessage = "Password is required")]
    [PasswordValidation]
    string Password,

    [StringLength(10, MinimumLength = 6, ErrorMessage = "MFA code must be between 6 and 10 characters")]
    string? MfaCode = null,

    bool RememberMe = false
);

public record LoginResponse(
    bool Success,
    string? AccessToken,
    string? RefreshToken,
    DateTime? ExpiresAt,
    UserDto? User,
    bool RequiresMfa = false,
    string? MfaMethod = null,
    string? Message = null
);

public record RefreshTokenRequest(string RefreshToken);

public record RefreshTokenResponse(
    bool Success,
    string? AccessToken,
    string? RefreshToken,
    DateTime? ExpiresAt,
    string? Message = null
);

public record LogoutRequest(string? RefreshToken = null);

public record ChangePasswordRequest(
    [Required(ErrorMessage = "Current password is required")]
    string CurrentPassword,

    [Required(ErrorMessage = "New password is required")]
    [PasswordValidation]
    string NewPassword
);

public record ForgotPasswordRequest(
    [Required(ErrorMessage = "Email is required")]
    [EmailValidation]
    string Email
);

public record ResetPasswordRequest(
    [Required(ErrorMessage = "Reset token is required")]
    string Token,

    [Required(ErrorMessage = "New password is required")]
    [PasswordValidation]
    string NewPassword
);

public record VerifyMfaRequest(
    string UserId,
    string Code
);

public record SetupMfaRequest(
    string Method
);

public record SetupMfaResponse(
    bool Success,
    string? Secret,
    string? QrCode,
    string[]? BackupCodes,
    string? Message = null
);

public record DisableMfaRequest(
    string Password,
    string? MfaCode = null
);

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string? ProfilePicture,
    string Status,
    bool IsEmailVerified,
    bool IsPhoneVerified,
    DateTime? LastLoginAt,
    bool IsMfaEnabled,
    string? PreferredMfaMethod,
    Guid? DefaultOrganizationId,
    DateTime CreatedAt
);

public record OrganizationDto(
    Guid Id,
    string Name,
    string? Description,
    string? Logo,
    string Status,
    string Type,
    Guid? ParentOrganizationId,
    DateTime CreatedAt
);

public record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    string Type,
    string Level,
    bool IsSystemRole,
    Guid? OrganizationId,
    DateTime CreatedAt
);

public record PermissionDto(
    Guid Id,
    string Name,
    string? Description,
    string Resource,
    string Action,
    string Type,
    bool IsSystemPermission
);

public record UserSessionDto(
    Guid Id,
    string SessionId,
    string IpAddress,
    string UserAgent,
    string? DeviceId,
    string? Location,
    DateTime StartedAt,
    DateTime LastAccessedAt,
    string Status,
    bool IsMfaVerified
);