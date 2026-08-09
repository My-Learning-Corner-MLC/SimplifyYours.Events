namespace EventService.Contracts.Events;

public sealed record UpdateEventResponse(
    Guid Id,
    string EventName,
    DateOnly EventDate,
    string EventType,
    string? EventDescription,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string ConcurrencyToken,
    EventLocationDto? Location = null,
    string? TimeZoneId = null);
