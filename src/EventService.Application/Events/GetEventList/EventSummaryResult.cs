namespace EventService.Application.Events.GetEventList;

public sealed record EventSummaryResult(
    Guid Id,
    string EventName,
    DateTimeOffset EventTime,
    string EventType,
    string? EventDescription,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
