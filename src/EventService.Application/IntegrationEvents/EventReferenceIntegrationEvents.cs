using System.Text.Json;
using EventService.Contracts.IntegrationEvents;
using EventService.Domain.Events;
using SimplifyYours.Event.Abstractions;

namespace EventService.Application.IntegrationEvents;

public static class EventReferenceIntegrationEvents
{
    /// <summary>
    /// Version 4 added the display fields that public invitation pages render (date, times, time
    /// zone, description, location). Consumers must read their absence below version 4 as "not
    /// published yet", never as "cleared" — otherwise replaying older messages would wipe a venue
    /// that is still current.
    /// </summary>
    private const int CurrentVersion = 4;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static IntegrationEventEnvelope Created(PlannedEvent plannedEvent, DateTimeOffset occurredAt)
    {
        return Create("EventCreated", plannedEvent, occurredAt);
    }

    public static IntegrationEventEnvelope Updated(PlannedEvent plannedEvent, DateTimeOffset occurredAt)
    {
        return Create("EventUpdated", plannedEvent, occurredAt);
    }

    public static IntegrationEventEnvelope Deleted(PlannedEvent plannedEvent, DateTimeOffset occurredAt)
    {
        return Create("EventDeleted", plannedEvent, occurredAt);
    }

    private static IntegrationEventEnvelope Create(
        string eventType,
        PlannedEvent plannedEvent,
        DateTimeOffset occurredAt)
    {
        var payload = JsonSerializer.Serialize(
            new EventReferencePayload(
                plannedEvent.Id,
                plannedEvent.Name,
                plannedEvent.TenantId,
                plannedEvent.Type.ToString().ToLowerInvariant(),
                plannedEvent.EventDate,
                plannedEvent.EventStartTime,
                plannedEvent.EventEndTime,
                plannedEvent.TimeZoneId,
                plannedEvent.Description,
                plannedEvent.Location is null
                    ? null
                    : new EventReferenceLocationPayload(
                        plannedEvent.Location.VenueName,
                        plannedEvent.Location.Address,
                        plannedEvent.Location.Notes)),
            JsonOptions);

        return new IntegrationEventEnvelope(
            Guid.NewGuid(),
            eventType,
            occurredAt.ToUniversalTime(),
            Guid.NewGuid(),
            null,
            payload,
            CurrentVersion);
    }
}
