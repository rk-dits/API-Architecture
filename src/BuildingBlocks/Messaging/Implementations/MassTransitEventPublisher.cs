using BuildingBlocks.Messaging.Abstractions;
using MassTransit;

namespace BuildingBlocks.Messaging.Implementations;

public class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : IIntegrationEvent
        => _publishEndpoint.Publish(@event, ct);

    public Task PublishEnvelopeAsync<TEvent>(Envelope<TEvent> envelope, CancellationToken ct = default) where TEvent : IIntegrationEvent
    {
        return _publishEndpoint.Publish(envelope.Event, context =>
        {
            context.Headers.Set("CorrelationId", envelope.CorrelationId);
            context.Headers.Set("CausationId", envelope.CausationId);
        }, ct);
    }
}
