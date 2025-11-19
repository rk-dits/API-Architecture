namespace BuildingBlocks.Messaging.Abstractions;

public interface IEventConsumer<in TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken ct = default);
}
