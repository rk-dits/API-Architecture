namespace BuildingBlocks.Messaging.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : IIntegrationEvent;
    Task PublishEnvelopeAsync<TEvent>(Envelope<TEvent> envelope, CancellationToken ct = default) where TEvent : IIntegrationEvent;
}
