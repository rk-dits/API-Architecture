using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IdentityService.Application.SuperAdmin.Commands;
using IdentityService.Contracts.SuperAdmin;
using IdentityService.IntegrationTests.Helpers;

namespace IdentityService.IntegrationTests.Controllers;

public class SuperAdminControllerTests : IClassFixture<IdentityServiceWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly IdentityServiceWebApplicationFactory _factory;

    public SuperAdminControllerTests(IdentityServiceWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSystemStats_ValidRequest_ReturnsSystemStats()
    {
        // Act
        var response = await _client.GetAsync("/api/super-admin/system-stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<SystemStatsResponse>(response);

        result.Should().NotBeNull();
        result!.TotalUsers.Should().BeGreaterOrEqualTo(0);
        result.ActiveUsers.Should().BeGreaterOrEqualTo(0);
        result.TotalOrganizations.Should().BeGreaterOrEqualTo(0);
        result.ActiveSessions.Should().BeGreaterOrEqualTo(0);
        result.SystemHealth.Should().NotBeNullOrEmpty();
        result.LastUpdated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task GetSystemUsers_ValidRequest_ReturnsPagedUsers()
    {
        // Act
        var response = await _client.GetAsync("/api/super-admin/users?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<PageResult<UserDto>>(response);

        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetSystemUsers_InvalidPagination_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/super-admin/users?page=0&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSystemOrganizations_ValidRequest_ReturnsPagedOrganizations()
    {
        // Act
        var response = await _client.GetAsync("/api/super-admin/organizations?page=1&pageSize=15");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<PageResult<OrganizationDto>>(response);

        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(15);
        result.TotalCount.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetSystemAuditLogs_ValidRequest_ReturnsPagedAuditLogs()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        // Act
        var response = await _client.GetAsync($"/api/super-admin/audit-logs?page=1&pageSize=20&startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<PageResult<AuditLogDto>>(response);

        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalCount.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetSystemAuditLogs_InvalidDateRange_ReturnsBadRequest()
    {
        // Arrange - End date before start date
        var startDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        var endDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        // Act
        var response = await _client.GetAsync($"/api/super-admin/audit-logs?page=1&pageSize=20&startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetActiveSessions_ValidRequest_ReturnsPagedSessions()
    {
        // Act
        var response = await _client.GetAsync("/api/super-admin/active-sessions?page=1&pageSize=25");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<PageResult<UserSessionDto>>(response);

        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(25);
        result.TotalCount.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task EnableMaintenanceMode_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var request = new EnableMaintenanceModeCommand(
            "Scheduled system maintenance",
            DateTime.UtcNow.AddHours(2));

        // Act
        var response = await _client.PostAsJsonAsync("/api/super-admin/maintenance-mode/enable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<bool>(response);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EnableMaintenanceMode_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange - Past end time
        var request = new EnableMaintenanceModeCommand(
            "Maintenance",
            DateTime.UtcNow.AddHours(-1));

        // Act
        var response = await _client.PostAsJsonAsync("/api/super-admin/maintenance-mode/enable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DisableMaintenanceMode_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var request = new DisableMaintenanceModeCommand();

        // Act
        var response = await _client.PostAsJsonAsync("/api/super-admin/maintenance-mode/disable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<bool>(response);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SuspendUserAccount_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new SuspendUserAccountCommand(
            userId,
            "Policy violation",
            DateTime.UtcNow.AddDays(7));

        // Act
        var response = await _client.PostAsJsonAsync("/api/super-admin/users/suspend", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<bool>(response);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SuspendUserAccount_EmptyUserId_ReturnsBadRequest()
    {
        // Arrange
        var request = new SuspendUserAccountCommand(
            Guid.Empty,
            "Policy violation",
            DateTime.UtcNow.AddDays(7));

        // Act
        var response = await _client.PostAsJsonAsync("/api/super-admin/users/suspend", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UnsuspendUserAccount_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UnsuspendUserAccountCommand(userId, "Appeal approved");

        // Act
        var response = await _client.PostAsJsonAsync("/api/super-admin/users/unsuspend", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<bool>(response);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ForceUserLogout_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ForceUserLogoutCommand(userId, "Security policy enforcement");

        // Act
        var response = await _client.PostAsJsonAsync("/api/super-admin/users/force-logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<bool>(response);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ForceUserLogout_EmptyUserId_ReturnsBadRequest()
    {
        // Arrange
        var request = new ForceUserLogoutCommand(Guid.Empty, "Security policy");

        // Act
        var response = await _client.PostAsJsonAsync("/api/super-admin/users/force-logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}