using EventService.Application.Abstractions.Events;
using EventService.Application.Abstractions.IntegrationEvents;
using EventService.Application.Events;
using EventService.Application.IntegrationEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventService.Application.Events.UpdateEvent;

public sealed class UpdateEventCommandHandler(
    IEventRepository eventRepository,
    IIntegrationEventOutbox integrationEventOutbox,
    TimeProvider timeProvider,
    ILogger<UpdateEventCommandHandler> logger)
    : IRequestHandler<UpdateEventCommand, UpdateEventResult>
{
    private const string UpdateEventsPermission = "events.update";

    public async Task<UpdateEventResult> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var currentUser = request.CurrentUser
            ?? throw new InvalidOperationException("Current user is required to update an event.");

        if (!currentUser.HasPermission(UpdateEventsPermission))
        {
            throw new UnauthorizedAccessException("Caller does not have permission to update events.");
        }

        var plannedEvent = await eventRepository.GetByIdAsync(
            request.EventId,
            currentUser.TenantId,
            cancellationToken,
            asNoTracking: false);

        if (plannedEvent is null)
        {
            logger.LogWarning("Event update requested but event was not found. EventId: {EventId}.", request.EventId);
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

        await integrationEventOutbox.AddAsync(
            EventReferenceIntegrationEvents.Updated(plannedEvent, now),
            cancellationToken);

        var updated = await eventRepository.UpdateAsync(
            plannedEvent,
            expectedConcurrencyToken,
            cancellationToken);

        if (!updated)
        {
            logger.LogWarning("Event update conflict detected. EventId: {EventId}.", request.EventId);
            return UpdateEventResult.Conflict();
        }

        logger.LogInformation("Event updated. EventId: {EventId}. EventTime: {EventTime}.", plannedEvent.Id, plannedEvent.EventTime);

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
