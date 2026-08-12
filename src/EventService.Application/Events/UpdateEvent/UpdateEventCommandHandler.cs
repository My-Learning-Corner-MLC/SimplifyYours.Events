using EventService.Application.Abstractions.Events;
using EventService.Application.Abstractions.IntegrationEvents;
using EventService.Application.Events;
using EventService.Application.IntegrationEvents;
using EventService.Domain.Events;
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
            now,
            ResolveLocation(request.Location, plannedEvent.Location),
            ResolveTimeZoneId(request.TimeZoneId, plannedEvent.TimeZoneId),
            ResolveOptionalTime(request.EventStartTime, plannedEvent.EventStartTime),
            ResolveOptionalTime(request.EventEndTime, plannedEvent.EventEndTime));

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

    private static EventLocation? ResolveLocation(EventLocationInput? location, EventLocation? currentLocation)
    {
        return location is null
            ? currentLocation
            : EventLocation.Create(
                location.VenueName,
                location.Address,
                location.Notes);
    }

    private static string? ResolveTimeZoneId(string? timeZoneId, string? currentTimeZoneId)
    {
        return timeZoneId is null ? currentTimeZoneId : timeZoneId;
    }

    private static TimeOnly? ResolveOptionalTime(string? value, TimeOnly? currentValue)
    {
        if (value is null)
        {
            return currentValue;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (EventParsing.TryParseEventTime(value, out var parsed))
        {
            return parsed;
        }

        throw new ArgumentException("Event time must be a valid time string.", nameof(value));
    }
}
