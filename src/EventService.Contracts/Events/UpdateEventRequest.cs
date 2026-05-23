namespace EventService.Contracts.Events;

public sealed record UpdateEventRequest(
    string EventName,
    string EventTime,
    string? EventDescription,
    string ConcurrencyToken);
