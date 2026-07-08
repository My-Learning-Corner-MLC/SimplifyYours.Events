namespace EventService.Application.Events.GetEventList;

public sealed record EventSummaryResult(
    Guid Id,
    string EventName,
    DateOnly EventDate,
    string EventType,
    string? EventDescription,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    EventLocationDetails? Location = null,
    TimeOnly? EventStartTime = null,
    TimeOnly? EventEndTime = null);
