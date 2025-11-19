namespace IntegrationHub.Contracts.Events;

public class IntegrationEvent
{
    public Guid Id { get; set; }
    public DateTimeOffset OccurredAt { get; set; }

    public IntegrationEvent()
    {
        Id = Guid.Empty;
        OccurredAt = DateTimeOffset.MinValue;
    }

    public IntegrationEvent(Guid id, DateTimeOffset occurredAt)
    {
        Id = id;
        OccurredAt = occurredAt;
    }
}
