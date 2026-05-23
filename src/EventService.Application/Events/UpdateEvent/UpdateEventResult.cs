using EventService.Application.Events;

namespace EventService.Application.Events.UpdateEvent;

public sealed record UpdateEventResult(
    UpdateEventStatus Status,
    EventDetails? Event = null)
{
    public static UpdateEventResult Updated(EventDetails eventDetails) =>
        new(UpdateEventStatus.Updated, eventDetails);

    public static UpdateEventResult NotFound() => new(UpdateEventStatus.NotFound);

    public static UpdateEventResult Conflict() => new(UpdateEventStatus.Conflict);
}
