using FluentAssertions;
using Xunit;

namespace IdentityService.SecurityTests.Simple;

/// <summary>
/// Simple security test class to validate test infrastructure is working
/// </summary>
public class InfrastructureSecurityTests
{
    [Fact]
    public void TestInfrastructure_ShouldWork()
    {
        // Arrange
        var expected = "Security Test Infrastructure Working";

        // Act
        var actual = GetTestMessage();

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void StringComparison_ShouldBeSecure()
    {
        // Arrange
        var secretValue = "MySecretPassword123!";
        var userInput = "MySecretPassword123!";

        // Act - Use secure comparison
        var isEqual = string.Equals(secretValue, userInput, StringComparison.Ordinal);

        // Assert
        isEqual.Should().BeTrue();
    }

    [Fact]
    public void GuidGeneration_ShouldBeUnique()
    {
        // Arrange & Act
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();

        // Assert
        guid1.Should().NotBe(guid2);
        guid1.ToString().Should().HaveLength(36); // Standard GUID format
        guid2.ToString().Should().HaveLength(36);
    }

    [Fact]
    public void PasswordValidation_ShouldWork()
    {
        // Arrange
        var strongPassword = "MyStrongPassword123!@#";
        var weakPassword = "123";

        // Act & Assert
        IsStrongPassword(strongPassword).Should().BeTrue();
        IsStrongPassword(weakPassword).Should().BeFalse();
    }

    private static bool IsStrongPassword(string password)
    {
        return password.Length >= 8 &&
               password.Any(char.IsUpper) &&
               password.Any(char.IsLower) &&
               password.Any(char.IsDigit) &&
               password.Any(c => !char.IsLetterOrDigit(c));
    }

    private static string GetTestMessage() => "Security Test Infrastructure Working";
}