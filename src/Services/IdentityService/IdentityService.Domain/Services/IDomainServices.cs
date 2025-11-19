using IdentityService.Domain.Entities;
using IdentityService.Domain.ValueObjects;

namespace IdentityService.Domain.Services;

public interface IAuthenticationService
{
    Task<(bool Success, string? Token, User? User)> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Token)> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<string> GeneratePasswordResetTokenAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> ValidatePasswordResetTokenAsync(string token, CancellationToken cancellationToken = default);
    Task LogoutAsync(string token, CancellationToken cancellationToken = default);
}

public interface IAuthorizationService
{
    Task<bool> HasPermissionAsync(Guid userId, string permission, Guid? organizationId = null, CancellationToken cancellationToken = default);
    Task<bool> HasRoleAsync(Guid userId, string role, Guid? organizationId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId, Guid? organizationId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetUserRolesAsync(Guid userId, Guid? organizationId = null, CancellationToken cancellationToken = default);
    Task<bool> CanAccessResourceAsync(Guid userId, string resource, string action, Dictionary<string, object>? context = null, CancellationToken cancellationToken = default);
}

public interface IMfaService
{
    Task<(bool Success, string Secret)> SetupMfaAsync(Guid userId, MfaMethod method, CancellationToken cancellationToken = default);
    Task<bool> VerifyMfaAsync(Guid userId, string code, CancellationToken cancellationToken = default);
    Task<bool> VerifyBackupCodeAsync(Guid userId, string backupCode, CancellationToken cancellationToken = default);
    Task DisableMfaAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GenerateBackupCodesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<string> GenerateQrCodeAsync(Guid userId, string secret, CancellationToken cancellationToken = default);
}

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    bool IsPasswordStrong(string password);
    Task<string> GenerateTemporaryPasswordAsync(CancellationToken cancellationToken = default);
    Task<bool> IsPasswordReusedAsync(Guid userId, string password, CancellationToken cancellationToken = default);
}

public interface INotificationService
{
    Task SendWelcomeEmailAsync(User user, string temporaryPassword, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(User user, string resetToken, CancellationToken cancellationToken = default);
    Task SendMfaCodeAsync(User user, string code, MfaMethod method, CancellationToken cancellationToken = default);
    Task SendOrganizationInviteAsync(User user, Organization organization, string inviteLink, CancellationToken cancellationToken = default);
    Task SendSecurityAlertAsync(User user, string alertType, Dictionary<string, object> context, CancellationToken cancellationToken = default);
}

public interface IAuditService
{
    Task LogAsync(string action, string resource, string? resourceId = null, Guid? userId = null, Guid? organizationId = null,
                  string? oldValues = null, string? newValues = null, string? reason = null,
                  Dictionary<string, object>? metadata = null, CancellationToken cancellationToken = default);
    Task LogUserActionAsync(Guid userId, AuditEventType eventType, string resource, string? resourceId = null,
                           string? details = null, CancellationToken cancellationToken = default);
    Task LogSecurityEventAsync(Guid? userId, string eventType, string details, string ipAddress, string userAgent,
                              CancellationToken cancellationToken = default);
}

public interface ISessionService
{
    Task<UserSession> CreateSessionAsync(User user, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    Task<UserSession?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task UpdateSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task EndSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task EndAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserSession>> GetActiveSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CleanupExpiredSessionsAsync(CancellationToken cancellationToken = default);
}

public interface IOrganizationService
{
    Task<Organization> CreateOrganizationAsync(string name, string? description = null, Guid? parentId = null, CancellationToken cancellationToken = default);
    Task<bool> AddUserToOrganizationAsync(Guid userId, Guid organizationId, UserOrganizationRole role, CancellationToken cancellationToken = default);
    Task<bool> RemoveUserFromOrganizationAsync(Guid userId, Guid organizationId, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserRoleInOrganizationAsync(Guid userId, Guid organizationId, UserOrganizationRole newRole, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetOrganizationUsersAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Organization>> GetUserOrganizationsAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface ISsoService
{
    Task<string> InitiateSsoLoginAsync(string provider, string returnUrl, CancellationToken cancellationToken = default);
    Task<(bool Success, User? User)> HandleSsoCallbackAsync(string provider, string code, string state, CancellationToken cancellationToken = default);
    Task<bool> LinkSsoAccountAsync(Guid userId, string provider, string externalId, CancellationToken cancellationToken = default);
    Task<bool> UnlinkSsoAccountAsync(Guid userId, string provider, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetLinkedProvidersAsync(Guid userId, CancellationToken cancellationToken = default);
}