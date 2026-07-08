namespace EventService.Contracts.Events;

public sealed record CreateEventResponse(
    Guid Id,
    string EventName,
    DateOnly EventDate,
    string EventType,
    string? EventDescription,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string ConcurrencyToken);
