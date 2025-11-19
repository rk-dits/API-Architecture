using FluentAssertions;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.IntegrationTests.Simple;

/// <summary>
/// Simple integration test class to validate test infrastructure is working
/// </summary>
public class InfrastructureIntegrationTests
{
    [Fact]
    public void TestInfrastructure_ShouldWork()
    {
        // Arrange
        var expected = "Integration Test Infrastructure Working";

        // Act
        var actual = GetTestMessage();

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void ServiceCollection_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<string>("Test Service");

        // Act
        var provider = services.BuildServiceProvider();
        var service = provider.GetService<string>();

        // Assert
        service.Should().Be("Test Service");
    }

    [Fact]
    public void HttpClient_ShouldWork()
    {
        // Arrange
        using var client = new HttpClient();
        client.BaseAddress = new Uri("https://api.github.com/");

        // Act & Assert
        client.BaseAddress.Should().NotBeNull();
        client.BaseAddress!.ToString().Should().Be("https://api.github.com/");
    }

    private static string GetTestMessage() => "Integration Test Infrastructure Working";
}