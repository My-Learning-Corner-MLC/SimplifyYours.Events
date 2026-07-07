namespace EventService.Contracts.Events;

public sealed record EventSummaryResponse(
    Guid Id,
    string EventName,
    DateTimeOffset EventTime,
    string EventType,
    string? EventDescription,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    EventLocationDto? Location = null,
    DateTimeOffset? EventStartTime = null,
    DateTimeOffset? EventEndTime = null);
