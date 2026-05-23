using EventService.Application.Events;

namespace EventService.Application.Events.GetEventDetails;

public sealed record GetEventDetailsResult(EventDetails Event);
