namespace EventService.Contracts.Events;

public sealed record CreateEventRequest(
    string EventName,
    string? EventTime,
    string EventType,
    string? EventDescription,
    EventLocationDto? Location = null,
    string? TimeZoneId = null,
    string? EventEndTime = null);
