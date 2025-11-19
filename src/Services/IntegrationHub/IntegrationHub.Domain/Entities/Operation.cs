// Domain model kept persistence-agnostic.

using BuildingBlocks.Common.Security;
namespace IntegrationHub.Domain.Entities;

public enum OperationStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3
}

/// <summary>
/// Represents a long-running integration operation composed of multiple provider steps.
/// </summary>
public class Operation
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    [ContainsPHI("Operation name may contain client identifiers")]
    public string Name { get; private set; }
    public OperationStatus Status { get; private set; } = OperationStatus.Pending;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private Operation(string name)
    {
        Name = name;
    }

    public static Operation Create(string name) => new(name);

    public void MarkRunning()
    {
        Status = OperationStatus.Running;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkCompleted()
    {
        Status = OperationStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        Status = OperationStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }
}