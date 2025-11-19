using IntegrationHub.Domain.Entities;
using Xunit;

namespace UnitTests;

public class OperationTests
{
    [Fact]
    public void Create_ShouldInitializeWithPendingStatus()
    {
        var op = Operation.Create("Test");
        Assert.Equal(OperationStatus.Pending, op.Status);
        Assert.Equal("Test", op.Name);
        Assert.True((DateTime.UtcNow - op.CreatedAt) < TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void StatusTransitions_ShouldUpdate()
    {
        var op = Operation.Create("Flow");
        op.MarkRunning();
        Assert.Equal(OperationStatus.Running, op.Status);
        op.MarkCompleted();
        Assert.Equal(OperationStatus.Completed, op.Status);
        op.MarkFailed();
        Assert.Equal(OperationStatus.Failed, op.Status);
    }
}
