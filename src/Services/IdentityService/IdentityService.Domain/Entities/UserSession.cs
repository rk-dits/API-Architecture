using Common.Entities;
using IdentityService.Domain.ValueObjects;

namespace IdentityService.Domain.Entities;

public class UserSession : Entity<Guid>, IAuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string SessionId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string? DeviceId { get; set; }
    public string? Location { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime LastAccessedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public SessionStatus Status { get; set; }

    // Security
    public bool IsMfaVerified { get; set; }
    public string? AccessToken { get; set; }
    public DateTime? AccessTokenExpiry { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public bool IsActive => Status == SessionStatus.Active && !IsExpired;
    public bool IsExpired => EndedAt.HasValue || (AccessTokenExpiry.HasValue && DateTime.UtcNow > AccessTokenExpiry);

    public void EndSession()
    {
        Status = SessionStatus.Ended;
        EndedAt = DateTime.UtcNow;
    }

    public void UpdateLastAccessed()
    {
        LastAccessedAt = DateTime.UtcNow;
    }
}

public enum SessionStatus
{
    Active,
    Ended,
    Expired,
    Terminated
}

public class UserOrganization : Entity<Guid>
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public UserOrganizationRole Role { get; set; }
    public UserOrganizationStatus Status { get; set; }
    public DateTime JoinedAt { get; set; }
    public string InvitedBy { get; set; } = string.Empty;
    public DateTime? LeftAt { get; set; }

    public bool IsActive => Status == UserOrganizationStatus.Active && !LeftAt.HasValue;
}

// Enums moved to ValueObjects/Enums.cs to avoid duplication

public enum UserOrganizationStatus
{
    Active,
    Inactive,
    Pending,
    Suspended
}

public class ApiKey : Entity<Guid>, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string KeyValue { get; set; } = string.Empty;
    public string HashedKey { get; set; } = string.Empty;

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public Guid? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    public ApiKeyStatus Status { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public int UsageCount { get; set; }
    public int? UsageLimit { get; set; }

    // Permissions & Scopes
    public List<string> Scopes { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();

    // Audit
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public bool IsActive => Status == ApiKeyStatus.Active && !IsExpired && !IsUsageLimitExceeded;
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt;
    public bool IsUsageLimitExceeded => UsageLimit.HasValue && UsageCount >= UsageLimit;

    public void RecordUsage()
    {
        UsageCount++;
        LastUsedAt = DateTime.UtcNow;
    }

    public void Revoke()
    {
        Status = ApiKeyStatus.Revoked;
    }
}

public enum ApiKeyStatus
{
    Active,
    Inactive,
    Revoked,
    Expired
}