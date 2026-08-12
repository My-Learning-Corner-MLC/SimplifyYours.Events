namespace EventService.Contracts.Events;

public sealed record UpdateEventRequest(
    string EventName,
    string EventDate,
    string? EventDescription,
    string ConcurrencyToken,
    EventLocationDto? Location = null,
    string? TimeZoneId = null,
    string? EventStartTime = null,
    string? EventEndTime = null);
