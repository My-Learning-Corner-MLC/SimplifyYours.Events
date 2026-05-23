using EventService.Application.Abstractions.Events;
using EventService.Application.Events;
using MediatR;

namespace EventService.Application.Events.UpdateEvent;

public sealed class UpdateEventCommandHandler(
    IEventRepository eventRepository,
    TimeProvider timeProvider)
    : IRequestHandler<UpdateEventCommand, UpdateEventResult>
{
    public async Task<UpdateEventResult> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var plannedEvent = await eventRepository.GetByIdAsync(
            request.EventId,
            cancellationToken,
            asNoTracking: false);

        if (plannedEvent is null)
        {
            return UpdateEventResult.NotFound();
        }

        var now = timeProvider.GetUtcNow();
        var eventTime = ResolveEventTime(request.EventTime);
        var expectedConcurrencyToken = Convert.FromBase64String(request.ConcurrencyToken);

        plannedEvent.UpdateDetails(
            request.EventName,
            eventTime,
            request.EventDescription,
            now);

        var updated = await eventRepository.UpdateAsync(
            plannedEvent,
            expectedConcurrencyToken,
            cancellationToken);

        if (!updated)
        {
            return UpdateEventResult.Conflict();
        }

        return UpdateEventResult.Updated(EventDetails.From(plannedEvent));
    }

    private static DateTimeOffset ResolveEventTime(string value)
    {
        if (EventParsing.TryParseEventTime(value, out var eventTime))
        {
            return eventTime;
        }

        throw new ArgumentException("Event time must be a valid date-time string.", nameof(value));
    }
}
