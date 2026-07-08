using EventService.Application.Authorization;
using MediatR;

namespace EventService.Application.Events.UpdateEvent;

public sealed record UpdateEventCommand(
    Guid EventId,
    string EventName,
    string EventDate,
    string? EventDescription,
    string ConcurrencyToken) : BaseCommand, IRequest<UpdateEventResult>;
