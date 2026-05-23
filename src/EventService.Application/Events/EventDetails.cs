using EventService.Domain.Events;

namespace EventService.Application.Events;

public sealed record EventDetails(
    Guid Id,
    string EventName,
    DateTimeOffset EventTime,
    string EventType,
    string? EventDescription,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string ConcurrencyToken)
{
    internal static EventDetails From(PlannedEvent plannedEvent)
    {
        return new EventDetails(
            plannedEvent.Id,
            plannedEvent.Name,
            plannedEvent.EventTime,
            plannedEvent.Type.ToString().ToLowerInvariant(),
            plannedEvent.Description,
            plannedEvent.CreatedAt,
            plannedEvent.UpdatedAt,
            Convert.ToBase64String(plannedEvent.ConcurrencyToken));
    }
}
