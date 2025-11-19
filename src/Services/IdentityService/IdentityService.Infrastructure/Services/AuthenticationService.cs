using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Services;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace IdentityService.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IdentityDbContext _context;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtService jwtService,
        IUserSessionRepository sessionRepository,
        IdentityDbContext context,
        ILogger<AuthenticationService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _sessionRepository = sessionRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<(bool Success, string? Token, User? User)> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(username) ?? await _userManager.FindByEmailAsync(username);

            if (user == null)
            {
                _logger.LogWarning("Authentication failed: User not found for username {Username}", username);
                return (false, null, null);
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Authentication failed: User {UserId} is not active", user.Id);
                return (false, null, null);
            }

            if (user.IsLocked)
            {
                _logger.LogWarning("Authentication failed: User {UserId} is locked", user.Id);
                return (false, null, null);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    user.LockAccount(TimeSpan.FromMinutes(30));
                }
                else
                {
                    user.RecordFailedLogin();
                }

                await _userManager.UpdateAsync(user);
                _logger.LogWarning("Authentication failed for user {UserId}: {Reason}", user.Id, result.ToString());
                return (false, null, null);
            }

            // Get user roles and permissions
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await GetUserPermissionsAsync(user.Id);

            // Generate JWT token
            var token = _jwtService.GenerateAccessToken(user, roles, permissions);

            // Update last login
            user.RecordSuccessfulLogin();
            user.RefreshToken = _jwtService.GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userManager.UpdateAsync(user);

            _logger.LogInformation("User {UserId} authenticated successfully", user.Id);
            return (true, token, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for username {Username}", username);
            return (false, null, null);
        }
    }

    public async Task<(bool Success, string? Token)> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, cancellationToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token validation failed: Invalid or expired token");
                return (false, null);
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Refresh token validation failed: User {UserId} is not active", user.Id);
                return (false, null);
            }

            // Get user roles and permissions
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await GetUserPermissionsAsync(user.Id);

            // Generate new access token
            var newAccessToken = _jwtService.GenerateAccessToken(user, roles, permissions);

            _logger.LogInformation("Token refreshed successfully for user {UserId}", user.Id);
            return (true, newAccessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return (false, null);
        }
    }

    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var principal = _jwtService.ValidateToken(token);
            return principal != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return false;
        }
    }

    public async Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, you might want to maintain a blacklist of revoked tokens
            // For now, we'll just log the action
            _logger.LogInformation("Token revoked");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token");
        }
    }

    public async Task<string> GeneratePasswordResetTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Password reset token generated for user {UserId}", user.Id);
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating password reset token for user {UserId}", user.Id);
            throw;
        }
    }

    public async Task<bool> ValidatePasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token, cancellationToken);
            return user != null && user.PasswordResetTokenExpiry > DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating password reset token");
            return false;
        }
    }

    public async Task LogoutAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, you might want to blacklist the token
            _logger.LogInformation("User logged out");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
        }
    }

    private async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        var permissions = new List<string>();

        // Get permissions from roles using custom UserRole entity
        var rolePermissions = await _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId && ur.IsActive && !ur.IsExpired)
            .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .ToListAsync();

        permissions.AddRange(rolePermissions);

        // Get direct user permissions
        var directPermissions = await _context.UserPermissions
            .Where(up => up.UserId == userId && up.IsActive && !up.IsExpired)
            .Select(up => up.Permission.Name)
            .ToListAsync();

        permissions.AddRange(directPermissions);

        return permissions.Distinct().ToList();
    }
}