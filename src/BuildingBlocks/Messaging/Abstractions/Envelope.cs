namespace BuildingBlocks.Messaging.Abstractions;

public sealed record Envelope<TEvent>(TEvent Event, string CorrelationId, string CausationId) where TEvent : IIntegrationEvent
{
    public static Envelope<TEvent> Create(TEvent @event, string? correlationId = null, string? causationId = null)
        => new(@event, correlationId ?? Guid.NewGuid().ToString(), causationId ?? @event.Id.ToString());
}
