using SimplifyYours.Event.Abstractions;

namespace EventService.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    private OutboxMessage(IntegrationEventEnvelope integrationEvent)
    {
        Id = integrationEvent.EventId;
        EventType = integrationEvent.EventType;
        OccurredAt = integrationEvent.OccurredAt;
        CorrelationId = integrationEvent.CorrelationId;
        CausationId = integrationEvent.CausationId;
        Payload = integrationEvent.Payload;
        Version = integrationEvent.Version;
        CreatedAt = integrationEvent.OccurredAt;
        ProcessedAt = null;
        RetryCount = 0;
        Error = null;
    }

    public Guid Id { get; private set; }

    public string EventType { get; private set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; private set; }

    public Guid CorrelationId { get; private set; }

    public Guid? CausationId { get; private set; }

    public string Payload { get; private set; } = string.Empty;

    public int Version { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public int RetryCount { get; private set; }

    public string? Error { get; private set; }

    public static OutboxMessage From(IntegrationEventEnvelope integrationEvent)
    {
        return new OutboxMessage(integrationEvent);
    }

    public void MarkProcessed(DateTimeOffset processedAt)
    {
        ProcessedAt = processedAt;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        RetryCount++;
        Error = error.Length > 2_000 ? error[..2_000] : error;
    }
}
