namespace EventService.Contracts.Events;

public sealed record GetEventDetailsResponse(
    Guid Id,
    string EventName,
    DateOnly EventDate,
    string EventType,
    string? EventDescription,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string ConcurrencyToken);
