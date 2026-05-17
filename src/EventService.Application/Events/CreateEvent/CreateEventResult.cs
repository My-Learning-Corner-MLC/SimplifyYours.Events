using EventService.Application.Events;

namespace EventService.Application.Events.CreateEvent;

public sealed record CreateEventResult(EventDetails Event);
