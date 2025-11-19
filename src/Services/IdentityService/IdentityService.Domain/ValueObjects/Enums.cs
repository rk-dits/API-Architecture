namespace IdentityService.Domain.ValueObjects;

public enum MfaMethod
{
    None = 0,
    SMS = 1,
    Email = 2,
    Authenticator = 3,
    Voice = 4,
    BackupCode = 5
}

public enum UserStatus
{
    Active = 0,
    Inactive = 1,
    Suspended = 2,
    Deleted = 3,
    PendingVerification = 4,
    Locked = 5
}

public enum SessionStatus
{
    Active = 0,
    Expired = 1,
    Terminated = 2,
    Invalid = 3
}

public enum OrganizationStatus
{
    Active = 0,
    Inactive = 1,
    Suspended = 2,
    Deleted = 3
}

public enum OrganizationType
{
    Enterprise = 0,
    Team = 1,
    Personal = 2,
    Trial = 3
}

public enum UserOrganizationRole
{
    Owner = 0,
    Admin = 1,
    Member = 2,
    Guest = 3
}

public enum AuditEventType
{
    UserLogin = 0,
    UserLogout = 1,
    UserCreated = 2,
    UserUpdated = 3,
    UserDeleted = 4,
    RoleAssigned = 5,
    RoleRemoved = 6,
    PermissionGranted = 7,
    PermissionRevoked = 8,
    PasswordChanged = 9,
    MfaEnabled = 10,
    MfaDisabled = 11,
    SecurityAlert = 12,
    Other = 99
}