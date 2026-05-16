namespace EventService.Application.Events.CreateEvent;

public sealed record CreateEventResult(
    Guid Id,
    string EventName,
    DateTimeOffset EventTime,
    string EventType,
    string? EventDescription,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
