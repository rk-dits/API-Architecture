using BuildingBlocks.Common.Security;
namespace CoreWorkflowService.Domain.Entities;

public enum WorkflowCaseStatus
{
    Draft = 0,
    Active = 1,
    Completed = 2,
    Cancelled = 3
}

public class WorkflowCase
{
    public Guid Id { get; private set; }
    [ContainsPHI("Workflow case name may contain patient info")]
    public string Name { get; private set; } = string.Empty;
    public WorkflowCaseStatus Status { get; private set; } = WorkflowCaseStatus.Draft;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private WorkflowCase() { }

    private WorkflowCase(Guid id, string name)
    {
        Id = id;
        Name = name;
        Status = WorkflowCaseStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public static WorkflowCase Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        return new WorkflowCase(Guid.NewGuid(), name.Trim());
    }

    public void Activate()
    {
        if (Status != WorkflowCaseStatus.Draft)
            throw new InvalidOperationException("Only draft cases can be activated.");
        Status = WorkflowCaseStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != WorkflowCaseStatus.Active)
            throw new InvalidOperationException("Only active cases can be completed.");
        Status = WorkflowCaseStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status is WorkflowCaseStatus.Completed or WorkflowCaseStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel a completed or already cancelled case.");
        Status = WorkflowCaseStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
