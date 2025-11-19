using CoreWorkflowService.Domain.Entities;
using Xunit;

namespace UnitTests;

public class WorkflowCaseTests
{
    [Fact]
    public void Create_ShouldStartInDraft()
    {
        var c = WorkflowCase.Create("Case A");
        Assert.Equal(WorkflowCaseStatus.Draft, c.Status);
        Assert.Equal("Case A", c.Name);
    }

    [Fact]
    public void Activate_Then_Complete_ShouldSucceed()
    {
        var c = WorkflowCase.Create("Case B");
        c.Activate();
        Assert.Equal(WorkflowCaseStatus.Active, c.Status);
        c.Complete();
        Assert.Equal(WorkflowCaseStatus.Completed, c.Status);
    }

    [Fact]
    public void Cancel_FromDraft_ShouldSucceed()
    {
        var c = WorkflowCase.Create("Case C");
        c.Cancel();
        Assert.Equal(WorkflowCaseStatus.Cancelled, c.Status);
    }

    [Fact]
    public void Complete_FromDraft_ShouldThrow()
    {
        var c = WorkflowCase.Create("Case D");
        Assert.Throws<InvalidOperationException>(() => c.Complete());
    }
}
