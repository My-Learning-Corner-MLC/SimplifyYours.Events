using EventService.Application.Abstractions.Events;
using EventService.Domain.Events;
using MediatR;

namespace EventService.Application.Events.CreateEvent;

public sealed class CreateEventCommandHandler(
    IEventRepository eventRepository,
    TimeProvider timeProvider)
    : IRequestHandler<CreateEventCommand, CreateEventResult>
{
    public async Task<CreateEventResult> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var eventTime = ResolveEventTime(request.EventTime, now);
        var eventType = ResolveEventType(request.EventType);

        var plannedEvent = PlannedEvent.Create(
            Guid.NewGuid(),
            request.EventName,
            eventTime,
            eventType,
            request.EventDescription,
            now);

        await eventRepository.AddAsync(plannedEvent, cancellationToken);

        return new CreateEventResult(
            plannedEvent.Id,
            plannedEvent.Name,
            plannedEvent.EventTime,
            plannedEvent.Type.ToString().ToLowerInvariant(),
            plannedEvent.Description,
            plannedEvent.CreatedAt,
            plannedEvent.UpdatedAt);
    }

    private static DateTimeOffset ResolveEventTime(string? value, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return now;
        }

        if (CreateEventParsing.TryParseEventTime(value, out var eventTime))
        {
            return eventTime;
        }

        throw new ArgumentException("Event time must be a valid date-time string.", nameof(value));
    }

    private static EventType ResolveEventType(string value)
    {
        if (CreateEventParsing.TryParseEventType(value, out var eventType))
        {
            return eventType;
        }

        throw new ArgumentException("Event type must be one of: birthday, wedding, event.", nameof(value));
    }
}
