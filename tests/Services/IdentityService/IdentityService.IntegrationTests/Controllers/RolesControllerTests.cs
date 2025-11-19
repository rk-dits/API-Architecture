using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IdentityService.Application.RBAC.Commands;
using IdentityService.Contracts.RBAC;
using IdentityService.IntegrationTests.Helpers;

namespace IdentityService.IntegrationTests.Controllers;

public class RolesControllerTests : IClassFixture<IdentityServiceWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly IdentityServiceWebApplicationFactory _factory;

    public RolesControllerTests(IdentityServiceWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateRole_ValidRequest_ReturnsCreatedRole()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var permissions = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var request = new CreateRoleCommand(
            TestHelper.GenerateUniqueName("Role"),
            "Test Role Description",
            organizationId,
            permissions);

        // Act
        var response = await _client.PostAsJsonAsync("/api/roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<RoleDto>(response);

        result.Should().NotBeNull();
        result!.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);
        result.OrganizationId.Should().Be(organizationId);
        result.IsActive.Should().BeTrue();
        result.IsSystem.Should().BeFalse();
    }

    [Fact]
    public async Task CreateRole_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var request = new CreateRoleCommand(
            "", // Invalid empty name
            "Test Role Description",
            organizationId,
            new List<Guid>());

        // Act
        var response = await _client.PostAsJsonAsync("/api/roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetRoleById_ExistingId_ReturnsRole()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var createRequest = new CreateRoleCommand(
            TestHelper.GenerateUniqueName("GetByIdTest"),
            "Test Role Description",
            organizationId,
            new List<Guid>());

        var createResponse = await _client.PostAsJsonAsync("/api/roles", createRequest);
        var createdRole = await TestHelper.DeserializeResponse<RoleDto>(createResponse);

        // Act
        var response = await _client.GetAsync($"/api/roles/{createdRole!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<RoleDto>(response);

        result.Should().NotBeNull();
        result!.Id.Should().Be(createdRole.Id);
        result.Name.Should().Be(createdRole.Name);
    }

    [Fact]
    public async Task GetRoleById_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/roles/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateRole_ValidRequest_ReturnsUpdatedRole()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var createRequest = new CreateRoleCommand(
            TestHelper.GenerateUniqueName("UpdateTest"),
            "Original Description",
            organizationId,
            new List<Guid>());

        var createResponse = await _client.PostAsJsonAsync("/api/roles", createRequest);
        var createdRole = await TestHelper.DeserializeResponse<RoleDto>(createResponse);

        var updateRequest = new UpdateRoleCommand(
            createdRole!.Id,
            "Updated Role Name",
            "Updated Description",
            new List<Guid> { Guid.NewGuid() });

        // Act
        var response = await _client.PutAsJsonAsync($"/api/roles/{createdRole.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<RoleDto>(response);

        result.Should().NotBeNull();
        result!.Id.Should().Be(createdRole.Id);
        result.Name.Should().Be(updateRequest.Name);
        result.Description.Should().Be(updateRequest.Description);
    }

    [Fact]
    public async Task DeleteRole_ExistingId_ReturnsSuccessResult()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var createRequest = new CreateRoleCommand(
            TestHelper.GenerateUniqueName("DeleteTest"),
            "Test Role Description",
            organizationId,
            new List<Guid>());

        var createResponse = await _client.PostAsJsonAsync("/api/roles", createRequest);
        var createdRole = await TestHelper.DeserializeResponse<RoleDto>(createResponse);

        // Act
        var response = await _client.DeleteAsync($"/api/roles/{createdRole!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<bool>(response);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetRolesByOrganization_ValidOrganizationId_ReturnsRoles()
    {
        // Arrange
        var organizationId = Guid.NewGuid();

        // Create some test roles
        for (int i = 0; i < 3; i++)
        {
            var createRequest = new CreateRoleCommand(
                TestHelper.GenerateUniqueName($"TestRole{i}"),
                $"Test Role Description {i}",
                organizationId,
                new List<Guid>());

            await _client.PostAsJsonAsync("/api/roles", createRequest);
        }

        // Act
        var response = await _client.GetAsync($"/api/roles/organization/{organizationId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<List<RoleDto>>(response);

        result.Should().NotBeNull();
        result!.Should().NotBeEmpty();
        result.All(r => r.OrganizationId == organizationId).Should().BeTrue();
        result.All(r => r.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task AssignRoleToUser_ValidIds_ReturnsSuccessResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var createRoleRequest = new CreateRoleCommand(
            TestHelper.GenerateUniqueName("AssignTest"),
            "Test Role Description",
            organizationId,
            new List<Guid>());

        var createResponse = await _client.PostAsJsonAsync("/api/roles", createRoleRequest);
        var createdRole = await TestHelper.DeserializeResponse<RoleDto>(createResponse);

        var assignRequest = new AssignRoleToUserCommand(userId, createdRole!.Id);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/roles/{createdRole.Id}/assign", assignRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<bool>(response);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllPermissions_ValidRequest_ReturnsPermissions()
    {
        // Act
        var response = await _client.GetAsync("/api/permissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<List<PermissionDto>>(response);

        result.Should().NotBeNull();
        result!.Should().NotBeEmpty();
        result.All(p => !string.IsNullOrEmpty(p.Name)).Should().BeTrue();
        result.All(p => !string.IsNullOrEmpty(p.Category)).Should().BeTrue();
    }

    [Fact]
    public async Task GetUserPermissions_ValidUserId_ReturnsPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/permissions/user/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<List<PermissionDto>>(response);

        result.Should().NotBeNull();
        result!.Should().NotBeEmpty();
        result.All(p => !string.IsNullOrEmpty(p.Name)).Should().BeTrue();
    }

    [Fact]
    public async Task CheckUserPermission_ValidRequest_ReturnsPermissionCheck()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var permission = "user.read";

        // Act
        var response = await _client.GetAsync($"/api/permissions/user/{userId}/check?permission={permission}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<UserPermissionCheckResponse>(response);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.Permission.Should().Be(permission);
        result.HasPermission.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserRoles_ValidUserId_ReturnsUserRoles()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/roles/user/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<List<RoleDto>>(response);

        result.Should().NotBeNull();
        result!.Should().NotBeEmpty();
        result.All(r => r.IsActive).Should().BeTrue();
    }
}