namespace EventService.Contracts.Events;

public sealed record GetEventDetailsResponse(
    Guid Id,
    string EventName,
    DateTimeOffset EventTime,
    string EventType,
    string? EventDescription,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
