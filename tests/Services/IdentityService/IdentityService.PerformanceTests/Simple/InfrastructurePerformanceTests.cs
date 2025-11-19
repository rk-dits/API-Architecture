using FluentAssertions;
using Xunit;

namespace IdentityService.PerformanceTests.Simple;

/// <summary>
/// Simple performance test class to validate test infrastructure is working
/// </summary>
public class InfrastructurePerformanceTests
{
    [Fact]
    public void TestInfrastructure_ShouldWork()
    {
        // Arrange
        var expected = "Performance Test Infrastructure Working";

        // Act
        var actual = GetTestMessage();

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void PerformanceMeasurement_ShouldWork()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        System.Threading.Thread.Sleep(10); // Simulate some work
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThan(5);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    [Fact]
    public void ListPerformance_ShouldWork()
    {
        // Arrange
        var list = new List<int>();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            list.Add(i);
        }

        // Assert
        list.Should().HaveCount(1000);
        list.First().Should().Be(0);
        list.Last().Should().Be(999);
    }

    private static string GetTestMessage() => "Performance Test Infrastructure Working";
}