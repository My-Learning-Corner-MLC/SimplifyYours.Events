using MediatR;

namespace EventService.Application.Events.UpdateEvent;

public sealed record UpdateEventCommand(
    Guid EventId,
    string EventName,
    string EventTime,
    string? EventDescription,
    string ConcurrencyToken) : IRequest<UpdateEventResult>;
