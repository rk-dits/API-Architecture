using Common.Entities;

namespace IdentityService.Domain.Entities;

public class Organization : Entity<Guid>, IAuditableEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public string? Website { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public Address? Address { get; set; }
    public OrganizationStatus Status { get; set; }
    public OrganizationType Type { get; set; }
    public string? TaxId { get; set; }

    // Hierarchical structure
    public Guid? ParentOrganizationId { get; set; }
    public Organization? ParentOrganization { get; set; }
    public virtual ICollection<Organization> SubOrganizations { get; set; } = new List<Organization>();

    // Settings
    public OrganizationSettings Settings { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();

    // Subscription & Billing
    public Guid? SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
    public DateTime? TrialStartDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public bool IsTrialActive => TrialEndDate.HasValue && DateTime.UtcNow < TrialEndDate;

    // Audit
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public virtual ICollection<UserOrganization> UserOrganizations { get; set; } = new List<UserOrganization>();
    public virtual ICollection<OrganizationRole> OrganizationRoles { get; set; } = new List<OrganizationRole>();
    public virtual ICollection<OrganizationPermission> OrganizationPermissions { get; set; } = new List<OrganizationPermission>();
    public virtual ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();

    public bool IsActive => Status == OrganizationStatus.Active;

    public void Activate()
    {
        Status = OrganizationStatus.Active;
    }

    public void Deactivate()
    {
        Status = OrganizationStatus.Inactive;
    }

    public void Suspend(string reason)
    {
        Status = OrganizationStatus.Suspended;
        Metadata["SuspensionReason"] = reason;
        Metadata["SuspensionDate"] = DateTime.UtcNow;
    }

    public void StartTrial(int durationDays = 30)
    {
        TrialStartDate = DateTime.UtcNow;
        TrialEndDate = DateTime.UtcNow.AddDays(durationDays);
    }

    public void EndTrial()
    {
        TrialEndDate = DateTime.UtcNow;
    }
}

public enum OrganizationStatus
{
    Active,
    Inactive,
    Suspended,
    PendingApproval
}

public enum OrganizationType
{
    Enterprise,
    SMB,
    Startup,
    NonProfit,
    Educational,
    Government
}

public class OrganizationSettings
{
    public bool RequireMfa { get; set; } = false;
    public bool AllowSso { get; set; } = true;
    public bool AllowGuestUsers { get; set; } = false;
    public int MaxUsers { get; set; } = 100;
    public int SessionTimeoutMinutes { get; set; } = 480; // 8 hours
    public List<string> AllowedDomains { get; set; } = new();
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}