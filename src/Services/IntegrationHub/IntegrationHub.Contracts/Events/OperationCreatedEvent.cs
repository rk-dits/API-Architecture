using BuildingBlocks.Messaging.Abstractions;

namespace IntegrationHub.Contracts.Events;

public class OperationCreatedEvent : IntegrationEvent
{
    public Guid OperationId { get; set; }
    public string Name { get; set; }

    public OperationCreatedEvent() : base(Guid.Empty, DateTimeOffset.MinValue) { }

    public OperationCreatedEvent(Guid operationId, string name)
        : base(Guid.NewGuid(), DateTimeOffset.UtcNow)
    {
        OperationId = operationId;
        Name = name;
    }

    public OperationCreatedEvent(Guid id, DateTimeOffset occurredAt, Guid operationId, string name)
        : base(id, occurredAt)
    {
        OperationId = operationId;
        Name = name;
    }
}