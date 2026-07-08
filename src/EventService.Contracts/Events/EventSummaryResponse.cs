namespace EventService.Contracts.Events;

public sealed record EventSummaryResponse(
    Guid Id,
    string EventName,
    DateOnly EventDate,
    string EventType,
    string? EventDescription,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    EventLocationDto? Location = null,
    TimeOnly? EventStartTime = null,
    TimeOnly? EventEndTime = null);
