namespace EventService.Contracts.Events;

public sealed record UpdateEventRequest(
    string EventName,
    string EventDate,
    string? EventDescription,
    string ConcurrencyToken);
