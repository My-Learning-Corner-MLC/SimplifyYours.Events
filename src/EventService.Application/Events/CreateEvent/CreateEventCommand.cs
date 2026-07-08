using EventService.Application.Authorization;
using MediatR;

namespace EventService.Application.Events.CreateEvent;

public sealed record CreateEventCommand(
    string EventName,
    string? EventDate,
    string EventType,
    string? EventDescription) : BaseCommand, IRequest<CreateEventResult>;
