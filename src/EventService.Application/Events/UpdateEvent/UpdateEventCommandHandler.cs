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
    public async Task<UpdateEventResult> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var currentUser = request.CurrentUser;

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
        var eventDate = ResolveEventDate(request.EventDate);
        var expectedConcurrencyToken = Convert.FromBase64String(request.ConcurrencyToken);

        plannedEvent.UpdateDetails(
            request.EventName,
            eventDate,
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

        logger.LogInformation("Event updated. EventId: {EventId}. EventDate: {EventDate}.", plannedEvent.Id, plannedEvent.EventDate);

        return UpdateEventResult.Updated(EventDetails.From(plannedEvent));
    }

    private static DateOnly ResolveEventDate(string value)
    {
        if (EventParsing.TryParseEventDate(value, out var eventDate))
        {
            return eventDate;
        }

        throw new ArgumentException("Event date must be a valid date string.", nameof(value));
    }
}
