using EventService.Application.Authorization;
using EventService.Application.Events;
using MediatR;

namespace EventService.Application.Events.UpdateEvent;

public sealed record UpdateEventCommand(
    Guid EventId,
    string EventName,
    string EventDate,
    string? EventDescription,
    string ConcurrencyToken,
    EventLocationInput? Location = null,
    string? TimeZoneId = null,
    string? EventStartTime = null,
    string? EventEndTime = null) : BaseCommand, IRequest<UpdateEventResult>;
