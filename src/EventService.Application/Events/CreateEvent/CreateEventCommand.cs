using EventService.Application.Authorization;
using MediatR;

namespace EventService.Application.Events.CreateEvent;

public sealed record CreateEventCommand(
    string EventName,
    string? EventTime,
    string EventType,
    string? EventDescription,
    CreateEventLocation? Location = null,
    string? TimeZoneId = null) : BaseCommand, IRequest<CreateEventResult>;

public sealed record CreateEventLocation(
    string? VenueName,
    string? Address,
    string? OnlineUrl,
    string? Notes);
