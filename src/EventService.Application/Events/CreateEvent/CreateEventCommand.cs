using EventService.Application.Authorization;
using EventService.Application.Events;
using MediatR;

namespace EventService.Application.Events.CreateEvent;

public sealed record CreateEventCommand(
    string EventName,
    string? EventDate,
    string EventType,
    string? EventDescription,
    EventLocationInput? Location = null,
    string? TimeZoneId = null,
    string? EventStartTime = null,
    string? EventEndTime = null) : BaseCommand, IRequest<CreateEventResult>;
