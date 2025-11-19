using Common.Entities;
using IdentityService.Domain.ValueObjects;

namespace IdentityService.Domain.Entities;

public class Subscription : Entity<Guid>, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SubscriptionTier Tier { get; set; }
    public SubscriptionStatus Status { get; set; }

    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    // Billing
    public decimal Price { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public bool AutoRenew { get; set; } = true;

    // Limits
    public int MaxUsers { get; set; }
    public int MaxApiCalls { get; set; }
    public long MaxStorage { get; set; } // in bytes
    public Dictionary<string, int> FeatureLimits { get; set; } = new();

    // Usage tracking
    public int CurrentUsers { get; set; }
    public int CurrentApiCalls { get; set; }
    public long CurrentStorage { get; set; }
    public Dictionary<string, int> CurrentUsage { get; set; } = new();

    // Features
    public List<string> IncludedFeatures { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();

    // Audit
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public bool IsActive => Status == SubscriptionStatus.Active && !IsExpired;
    public bool IsExpired => EndDate.HasValue && DateTime.UtcNow > EndDate;

    public bool HasFeature(string featureName)
    {
        return IncludedFeatures.Contains(featureName);
    }

    public bool IsWithinLimits()
    {
        return CurrentUsers <= MaxUsers &&
               CurrentApiCalls <= MaxApiCalls &&
               CurrentStorage <= MaxStorage;
    }

    public void Activate()
    {
        Status = SubscriptionStatus.Active;
        StartDate = DateTime.UtcNow;
        CalculateNextBillingDate();
    }

    public void Cancel()
    {
        Status = SubscriptionStatus.Canceled;
        AutoRenew = false;
        EndDate = NextBillingDate; // Let it expire at end of current billing period
    }

    public void Suspend(string reason)
    {
        Status = SubscriptionStatus.Suspended;
        Metadata["SuspensionReason"] = reason;
        Metadata["SuspensionDate"] = DateTime.UtcNow;
    }

    private void CalculateNextBillingDate()
    {
        NextBillingDate = BillingCycle switch
        {
            BillingCycle.Monthly => StartDate.AddMonths(1),
            BillingCycle.Quarterly => StartDate.AddMonths(3),
            BillingCycle.Yearly => StartDate.AddYears(1),
            _ => StartDate.AddMonths(1)
        };
    }
}

public enum SubscriptionTier
{
    Free,
    Basic,
    Professional,
    Enterprise,
    Custom
}

public enum SubscriptionStatus
{
    Active,
    Inactive,
    Suspended,
    Canceled,
    Expired,
    PendingActivation
}

public enum BillingCycle
{
    Monthly,
    Quarterly,
    Yearly
}

public class AuditLog : Entity<Guid>
{
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public Guid? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string? ResourceId { get; set; }
    public AuditEventType EventType { get; set; }

    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? Reason { get; set; }

    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string? SessionId { get; set; }

    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();

    public string GetFormattedMessage()
    {
        return EventType switch
        {
            AuditEventType.UserCreated => $"Created {Resource} {ResourceId}",
            AuditEventType.UserUpdated => $"Updated {Resource} {ResourceId}",
            AuditEventType.UserDeleted => $"Deleted {Resource} {ResourceId}",
            AuditEventType.UserLogin => $"User logged in",
            AuditEventType.UserLogout => $"User logged out",
            AuditEventType.PermissionGranted => $"Granted permission on {Resource}",
            AuditEventType.PermissionRevoked => $"Revoked permission on {Resource}",
            _ => Action
        };
    }
}

// AuditEventType enum moved to ValueObjects/Enums.cs to avoid duplication