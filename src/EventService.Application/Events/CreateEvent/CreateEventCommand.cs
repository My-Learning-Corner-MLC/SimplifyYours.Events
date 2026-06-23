using EventService.Application.Authorization;
using MediatR;

namespace EventService.Application.Events.CreateEvent;

public sealed record CreateEventCommand(
    string EventName,
    string? EventTime,
    string EventType,
    string? EventDescription,
    CurrentUser? CurrentUser = null) : IRequest<CreateEventResult>;
