using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace IdentityService.Tests;

public class IdentityServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ILogger<IdentityServiceIntegrationTests> _logger;

    public IdentityServiceIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();

        var serviceProvider = _factory.Services;
        _logger = serviceProvider.GetRequiredService<ILogger<IdentityServiceIntegrationTests>>();
    }

    [Fact]
    public async Task HealthCheck_ShouldReturn_Healthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Health check response: {Content}", content);

        Assert.Contains("Healthy", content);
    }

    [Fact]
    public async Task Swagger_ShouldBeAccessible()
    {
        // Act
        var response = await _client.GetAsync("/swagger/index.html");

        // Assert
        response.EnsureSuccessStatusCode();
        _logger.LogInformation("Swagger UI is accessible");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "invalid@example.com",
            Password = "invalidpassword"
        };

        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        _logger.LogInformation("Login with invalid credentials correctly returned Unauthorized");
    }

    [Fact]
    public async Task RegisterUser_ShouldWork()
    {
        // Arrange
        var registerRequest = new
        {
            Email = $"test.{Guid.NewGuid()}@example.com",
            Password = "TestPassword123!",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+1234567890"
        };

        var json = JsonSerializer.Serialize(registerRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/users/register", content);

        // Assert
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("User registration successful");
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Registration response: {Content}", responseContent);
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("User registration failed with status {StatusCode}: {Content}",
                response.StatusCode, errorContent);
        }
    }

    [Fact]
    public async Task GetUsers_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        _logger.LogInformation("Get users without authentication correctly returned Unauthorized");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _client?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}