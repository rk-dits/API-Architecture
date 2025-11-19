namespace IdentityService.Contracts.SuperAdmin;

public class SystemStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalOrganizations { get; set; }
    public int TotalRoles { get; set; }
    public TimeSpan SystemUptime { get; set; }
    public DateTime LastBackup { get; set; }
}