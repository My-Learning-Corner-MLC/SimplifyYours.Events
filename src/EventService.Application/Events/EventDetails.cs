using EventService.Domain.Events;

namespace EventService.Application.Events;

public sealed record EventDetails(
    Guid Id,
    string EventName,
    DateOnly EventDate,
    string EventType,
    string? EventDescription,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string ConcurrencyToken,
    EventLocationDetails? Location = null,
    string? TimeZoneId = null,
    TimeOnly? EventStartTime = null,
    TimeOnly? EventEndTime = null)
{
    internal static EventDetails From(PlannedEvent plannedEvent)
    {
        return new EventDetails(
            plannedEvent.Id,
            plannedEvent.Name,
            plannedEvent.EventDate,
            plannedEvent.Type.ToString().ToLowerInvariant(),
            plannedEvent.Description,
            plannedEvent.CreatedAt,
            plannedEvent.UpdatedAt,
            Convert.ToBase64String(plannedEvent.ConcurrencyToken),
            EventLocationDetails.From(plannedEvent.Location),
            plannedEvent.TimeZoneId,
            plannedEvent.EventStartTime,
            plannedEvent.EventEndTime);
    }
}

public sealed record EventLocationDetails(
    string? VenueName,
    string? Address,
    string? Notes)
{
    internal static EventLocationDetails? From(EventLocation? location)
    {
        return location is null
            ? null
            : new EventLocationDetails(
                location.VenueName,
                location.Address,
                location.Notes);
    }
}
