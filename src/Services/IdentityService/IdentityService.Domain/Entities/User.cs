using IdentityService.Domain.ValueObjects;
using Common.Entities;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Domain.Entities;

public class User : IdentityUser<Guid>, IAuditableEntity, IAggregateRoot
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
    public UserStatus Status { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? PasswordChangedAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedOutUntil { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Multi-tenancy
    public Guid? DefaultOrganizationId { get; set; }
    public Organization? DefaultOrganization { get; set; }

    // MFA Settings
    public bool IsMfaEnabled { get; set; }
    public string? MfaSecret { get; set; }
    public List<string> MfaBackupCodes { get; set; } = new();
    public MfaMethod PreferredMfaMethod { get; set; }

    // Security
    public List<string> SecurityQuestions { get; set; } = new();
    public DateTime? LastPasswordResetAt { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<UserOrganization> UserOrganizations { get; set; } = new List<UserOrganization>();
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public string FullName => $"{FirstName} {LastName}".Trim();

    public bool IsActive => Status == UserStatus.Active;
    public bool IsLocked => LockedOutUntil.HasValue && LockedOutUntil > DateTime.UtcNow;

    public void LockAccount(TimeSpan lockoutDuration)
    {
        LockedOutUntil = DateTime.UtcNow.Add(lockoutDuration);
        FailedLoginAttempts = 0;
    }

    public void UnlockAccount()
    {
        LockedOutUntil = null;
        FailedLoginAttempts = 0;
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5)
        {
            LockAccount(TimeSpan.FromMinutes(30));
        }
    }

    public void RecordSuccessfulLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockedOutUntil = null;
    }

    public void EnableMfa(MfaMethod method, string secret)
    {
        IsMfaEnabled = true;
        PreferredMfaMethod = method;
        MfaSecret = secret;
        GenerateMfaBackupCodes();
    }

    public void DisableMfa()
    {
        IsMfaEnabled = false;
        MfaSecret = null;
        MfaBackupCodes.Clear();
    }

    public void GenerateMfaBackupCodes()
    {
        MfaBackupCodes.Clear();
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        for (int i = 0; i < 10; i++)
        {
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var randomNumber = BitConverter.ToUInt32(bytes, 0);
            var code = (randomNumber % 900000 + 100000).ToString(System.Globalization.CultureInfo.InvariantCulture);
            MfaBackupCodes.Add(code);
        }
    }
}