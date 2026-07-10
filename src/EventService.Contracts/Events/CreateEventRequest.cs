namespace EventService.Contracts.Events;

public sealed record CreateEventRequest(
    string EventName,
    string? EventDate,
    string EventType,
    string? EventDescription,
    EventLocationDto? Location = null,
    string? TimeZoneId = null,
    string? EventStartTime = null,
    string? EventEndTime = null);
