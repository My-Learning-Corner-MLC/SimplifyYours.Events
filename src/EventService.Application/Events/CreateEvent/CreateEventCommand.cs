using EventService.Application.Authorization;
using MediatR;

namespace EventService.Application.Events.CreateEvent;

public sealed record CreateEventCommand(
    string EventName,
    string? EventTime,
    string EventType,
    string? EventDescription,
    CreateEventLocation? Location = null,
    string? TimeZoneId = null,
    string? EventStartTime = null,
    string? EventEndTime = null) : BaseCommand, IRequest<CreateEventResult>;

public sealed record CreateEventLocation(
    string? VenueName,
    string? Address,
    string? Notes);
