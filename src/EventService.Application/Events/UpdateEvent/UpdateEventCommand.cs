using EventService.Application.Authorization;
using MediatR;

namespace EventService.Application.Events.UpdateEvent;

public sealed record UpdateEventCommand(
    Guid EventId,
    string EventName,
    string EventDate,
    string? EventDescription,
    string ConcurrencyToken,
    UpdateEventLocation? Location = null,
    string? TimeZoneId = null) : BaseCommand, IRequest<UpdateEventResult>;

public sealed record UpdateEventLocation(
    string? VenueName,
    string? Address,
    string? Notes);
