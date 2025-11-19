using FluentAssertions;
using Xunit;

namespace IdentityService.UnitTests.Simple;

/// <summary>
/// Simple test class to validate test infrastructure is working
/// </summary>
public class InfrastructureTests
{
    [Fact]
    public void TestInfrastructure_ShouldWork()
    {
        // Arrange
        var expected = "Test Infrastructure Working";

        // Act
        var actual = GetTestMessage();

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void Mathematics_ShouldWork()
    {
        // Arrange
        var a = 5;
        var b = 3;
        var expected = 8;

        // Act
        var result = a + b;

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Collections_ShouldWork()
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2", "Item3" };

        // Act & Assert
        items.Should().HaveCount(3);
        items.Should().Contain("Item2");
        items.Should().NotContain("Item4");
    }

    private static string GetTestMessage() => "Test Infrastructure Working";
}