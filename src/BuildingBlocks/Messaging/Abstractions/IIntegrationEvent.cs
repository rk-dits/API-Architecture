namespace BuildingBlocks.Messaging.Abstractions;

public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTimeOffset OccurredAt { get; }
}

public abstract record IntegrationEvent(Guid Id, DateTimeOffset OccurredAt) : IIntegrationEvent
{
    protected IntegrationEvent() : this(Guid.NewGuid(), DateTimeOffset.UtcNow) { }
}
