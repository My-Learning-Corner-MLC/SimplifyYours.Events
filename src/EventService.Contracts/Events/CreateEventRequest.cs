namespace EventService.Contracts.Events;

public sealed record CreateEventRequest(
    string EventName,
    string? EventDate,
    string EventType,
    string? EventDescription);
