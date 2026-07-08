using EventService.Application.Abstractions.Common;
using EventService.Application.Abstractions.Events;
using EventService.Application.Abstractions.IntegrationEvents;
using EventService.Application.Events;
using EventService.Application.IntegrationEvents;
using EventService.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventService.Application.Events.CreateEvent;

public sealed class CreateEventCommandHandler(
    IEventRepository eventRepository,
    IIntegrationEventOutbox integrationEventOutbox,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<CreateEventCommandHandler> logger)
    : IRequestHandler<CreateEventCommand, CreateEventResult>
{
    public async Task<CreateEventResult> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var currentUser = request.CurrentUser;
        var now = timeProvider.GetUtcNow();
        var eventDate = ResolveEventDate(request.EventDate, now);
        var eventType = ResolveEventType(request.EventType);

        var plannedEvent = PlannedEvent.Create(
            Guid.NewGuid(),
            currentUser.TenantId,
            request.EventName,
            eventDate,
            eventType,
            request.EventDescription,
            now);

        await eventRepository.AddAsync(plannedEvent, cancellationToken);
        await integrationEventOutbox.AddAsync(
            EventReferenceIntegrationEvents.Created(plannedEvent, now),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Event created. EventId: {EventId}. EventType: {EventType}. EventDate: {EventDate}.",
            plannedEvent.Id,
            plannedEvent.Type,
            plannedEvent.EventDate);

        return new CreateEventResult(EventDetails.From(plannedEvent));
    }

    private static DateOnly ResolveEventDate(string? value, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DateOnly.FromDateTime(now.UtcDateTime);
        }

        if (EventParsing.TryParseEventDate(value, out var eventDate))
        {
            return eventDate;
        }

        throw new ArgumentException("Event date must be a valid date string.", nameof(value));
    }

    private static EventType ResolveEventType(string value)
    {
        if (EventParsing.TryParseEventType(value, out var eventType))
        {
            return eventType;
        }

        throw new ArgumentException("Event type must be one of: birthday, wedding, event.", nameof(value));
    }
}
