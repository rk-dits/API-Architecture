using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IdentityService.Application.Organizations.Commands;
using IdentityService.Contracts.Common;
using IdentityService.Contracts.Organizations;
using IdentityService.IntegrationTests.Helpers;

namespace IdentityService.IntegrationTests.Controllers;

public class OrganizationsControllerTests : IClassFixture<IdentityServiceWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly IdentityServiceWebApplicationFactory _factory;

    public OrganizationsControllerTests(IdentityServiceWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateOrganization_ValidRequest_ReturnsCreatedOrganization()
    {
        // Arrange
        var request = new CreateOrganizationCommand(
            TestHelper.GenerateUniqueName("Organization"),
            "Test Description",
            "https://test.com",
            TestHelper.GenerateUniqueEmail(),
            "+1234567890",
            "Enterprise");

        // Act
        var response = await _client.PostAsJsonAsync("/api/organizations", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<OrganizationDto>(response);

        result.Should().NotBeNull();
        result!.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);
        result.Website.Should().Be(request.Website);
        result.ContactEmail.Should().Be(request.ContactEmail);
        result.ContactPhone.Should().Be(request.ContactPhone);
        result.Type.Should().Be(request.Type);
        result.Status.Should().Be("Active");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateOrganization_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateOrganizationCommand(
            "", // Invalid empty name
            "Test Description",
            "https://test.com",
            TestHelper.GenerateUniqueEmail(),
            "+1234567890",
            "Enterprise");

        // Act
        var response = await _client.PostAsJsonAsync("/api/organizations", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrganizations_ValidRequest_ReturnsPagedResult()
    {
        // Arrange
        // First create some test organizations
        for (int i = 0; i < 3; i++)
        {
            var createRequest = new CreateOrganizationCommand(
                TestHelper.GenerateUniqueName($"TestOrg{i}"),
                $"Test Description {i}",
                $"https://test{i}.com",
                TestHelper.GenerateUniqueEmail(),
                $"+12345678{i:D2}",
                "Enterprise");

            await _client.PostAsJsonAsync("/api/organizations", createRequest);
        }

        // Act
        var response = await _client.GetAsync("/api/organizations?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<PageResult<OrganizationDto>>(response);

        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public async Task GetOrganizationById_ExistingId_ReturnsOrganization()
    {
        // Arrange
        var createRequest = new CreateOrganizationCommand(
            TestHelper.GenerateUniqueName("GetByIdTest"),
            "Test Description",
            "https://test.com",
            TestHelper.GenerateUniqueEmail(),
            "+1234567890",
            "Enterprise");

        var createResponse = await _client.PostAsJsonAsync("/api/organizations", createRequest);
        var createdOrganization = await TestHelper.DeserializeResponse<OrganizationDto>(createResponse);

        // Act
        var response = await _client.GetAsync($"/api/organizations/{createdOrganization!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<OrganizationDto>(response);

        result.Should().NotBeNull();
        result!.Id.Should().Be(createdOrganization.Id);
        result.Name.Should().Be(createdOrganization.Name);
    }

    [Fact]
    public async Task GetOrganizationById_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/organizations/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateOrganization_ValidRequest_ReturnsUpdatedOrganization()
    {
        // Arrange
        var createRequest = new CreateOrganizationCommand(
            TestHelper.GenerateUniqueName("UpdateTest"),
            "Original Description",
            "https://original.com",
            TestHelper.GenerateUniqueEmail(),
            "+1234567890",
            "Enterprise");

        var createResponse = await _client.PostAsJsonAsync("/api/organizations", createRequest);
        var createdOrganization = await TestHelper.DeserializeResponse<OrganizationDto>(createResponse);

        var updateRequest = new UpdateOrganizationCommand(
            createdOrganization!.Id,
            "Updated Organization Name",
            "Updated Description",
            "https://updated.com",
            TestHelper.GenerateUniqueEmail(),
            "+0987654321");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/organizations/{createdOrganization.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<OrganizationDto>(response);

        result.Should().NotBeNull();
        result!.Id.Should().Be(createdOrganization.Id);
        result.Name.Should().Be(updateRequest.Name);
        result.Description.Should().Be(updateRequest.Description);
        result.Website.Should().Be(updateRequest.Website);
        result.ContactEmail.Should().Be(updateRequest.ContactEmail);
        result.ContactPhone.Should().Be(updateRequest.ContactPhone);
    }

    [Fact]
    public async Task DeleteOrganization_ExistingId_ReturnsSuccessResult()
    {
        // Arrange
        var createRequest = new CreateOrganizationCommand(
            TestHelper.GenerateUniqueName("DeleteTest"),
            "Test Description",
            "https://test.com",
            TestHelper.GenerateUniqueEmail(),
            "+1234567890",
            "Enterprise");

        var createResponse = await _client.PostAsJsonAsync("/api/organizations", createRequest);
        var createdOrganization = await TestHelper.DeserializeResponse<OrganizationDto>(createResponse);

        // Act
        var response = await _client.DeleteAsync($"/api/organizations/{createdOrganization!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<bool>(response);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetOrganizationDetails_ExistingId_ReturnsOrganizationDetails()
    {
        // Arrange
        var createRequest = new CreateOrganizationCommand(
            TestHelper.GenerateUniqueName("DetailsTest"),
            "Test Description",
            "https://test.com",
            TestHelper.GenerateUniqueEmail(),
            "+1234567890",
            "Enterprise");

        var createResponse = await _client.PostAsJsonAsync("/api/organizations", createRequest);
        var createdOrganization = await TestHelper.DeserializeResponse<OrganizationDto>(createResponse);

        // Act
        var response = await _client.GetAsync($"/api/organizations/{createdOrganization!.Id}/details");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<OrganizationDetailsResponse>(response);

        result.Should().NotBeNull();
        result!.Organization.Should().NotBeNull();
        result.Organization.Id.Should().Be(createdOrganization.Id);
        result.RecentMembers.Should().NotBeNull();
        result.RecentRoles.Should().NotBeNull();
        result.RecentActivity.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckNameAvailability_AvailableName_ReturnsTrue()
    {
        // Arrange
        var uniqueName = TestHelper.GenerateUniqueName("AvailabilityTest");

        // Act
        var response = await _client.GetAsync($"/api/organizations/check-name-availability?name={uniqueName}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestHelper.DeserializeResponse<NameAvailabilityResponse>(response);

        result.Should().NotBeNull();
        result!.Name.Should().Be(uniqueName);
        result.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task CheckNameAvailability_EmptyName_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/organizations/check-name-availability?name=");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}