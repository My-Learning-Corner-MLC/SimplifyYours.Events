namespace EventService.Contracts.Events;

public sealed record CreateEventResponse(
    Guid Id,
    string EventName,
    DateTimeOffset EventTime,
    string EventType,
    string? EventDescription,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string ConcurrencyToken,
    EventLocationDto? Location = null,
    string? TimeZoneId = null,
    DateTimeOffset? EventStartTime = null,
    DateTimeOffset? EventEndTime = null);
