using System.Text.Json;
using EventService.Application.IntegrationEvents;
using EventService.Contracts.IntegrationEvents;
using EventService.Domain.Events;

namespace EventService.UnitTests.Events.IntegrationEvents;

public sealed class EventReferenceIntegrationEventsTests
{
    private static readonly Guid TestTenantId = Guid.Parse("8f10c8b2-12a4-4d6f-9301-7c52e84b7d20");
    private readonly DateTimeOffset _now = new(2026, 5, 17, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Created_PayloadIncludesLowercaseEventType()
    {
        var plannedEvent = PlannedEvent.Create(
            Guid.NewGuid(),
            TestTenantId,
            "Wedding plan",
            DateOnly.FromDateTime(_now.DateTime),
            EventType.Wedding,
            null,
            _now);

        var envelope = EventReferenceIntegrationEvents.Created(plannedEvent, _now);

        var payload = JsonSerializer.Deserialize<EventReferencePayload>(
            envelope.Payload,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(payload);
        Assert.Equal(plannedEvent.Id, payload.EventId);
        Assert.Equal("wedding", payload.EventType);
    }

    [Theory]
    [InlineData(EventType.Birthday, "birthday")]
    [InlineData(EventType.Other, "other")]
    public void Created_PayloadReflectsPlannedEventType(EventType type, string expected)
    {
        var plannedEvent = PlannedEvent.Create(
            Guid.NewGuid(),
            TestTenantId,
            "Some event",
            DateOnly.FromDateTime(_now.DateTime),
            type,
            null,
            _now);

        var envelope = EventReferenceIntegrationEvents.Created(plannedEvent, _now);

        var payload = JsonSerializer.Deserialize<EventReferencePayload>(
            envelope.Payload,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Equal(expected, payload!.EventType);
    }

    [Fact]
    public void Created_PayloadCarriesDisplayFieldsAtVersion4()
    {
        var plannedEvent = PlannedEvent.Create(
            Guid.NewGuid(),
            TestTenantId,
            "Wedding plan",
            new DateOnly(2026, 9, 12),
            EventType.Wedding,
            "An evening reception",
            _now,
            EventLocation.Create("Rosewood Hall", "12 Sample Street", "Parking at the rear"),
            "Asia/Ho_Chi_Minh",
            new TimeOnly(18, 30),
            new TimeOnly(23, 0));

        var envelope = EventReferenceIntegrationEvents.Created(plannedEvent, _now);
        var payload = Deserialize(envelope.Payload);

        // The version is the consumer's signal that these fields are actually published; without
        // the bump a consumer cannot tell "absent" from "cleared".
        Assert.Equal(4, envelope.Version);
        Assert.NotNull(payload);
        Assert.Equal(new DateOnly(2026, 9, 12), payload.EventDate);
        Assert.Equal(new TimeOnly(18, 30), payload.EventStartTime);
        Assert.Equal(new TimeOnly(23, 0), payload.EventEndTime);
        Assert.Equal("Asia/Ho_Chi_Minh", payload.TimeZoneId);
        Assert.Equal("An evening reception", payload.EventDescription);
        Assert.Equal("Rosewood Hall", payload.Location?.VenueName);
        Assert.Equal("12 Sample Street", payload.Location?.Address);
        Assert.Equal("Parking at the rear", payload.Location?.Notes);
    }

    [Fact]
    public void Created_WhenEventHasNoLocation_OmitsLocationRatherThanEmittingAnEmptyObject()
    {
        var plannedEvent = PlannedEvent.Create(
            Guid.NewGuid(),
            TestTenantId,
            "Wedding plan",
            new DateOnly(2026, 9, 12),
            EventType.Wedding,
            null,
            _now);

        var payload = Deserialize(EventReferenceIntegrationEvents.Created(plannedEvent, _now).Payload);

        Assert.NotNull(payload);
        Assert.Null(payload.Location);
        Assert.Null(payload.TimeZoneId);
        Assert.Null(payload.EventDescription);
    }

    [Fact]
    public void Updated_PayloadCarriesTheSameDisplayFieldsAsCreated()
    {
        var plannedEvent = PlannedEvent.Create(
            Guid.NewGuid(),
            TestTenantId,
            "Wedding plan",
            new DateOnly(2026, 9, 12),
            EventType.Wedding,
            null,
            _now,
            EventLocation.Create("Rosewood Hall", null, null));

        var envelope = EventReferenceIntegrationEvents.Updated(plannedEvent, _now);
        var payload = Deserialize(envelope.Payload);

        // A venue edit only reaches an already-sent invitation through EventUpdated, so this path
        // has to carry the location too.
        Assert.Equal("EventUpdated", envelope.EventType);
        Assert.Equal(4, envelope.Version);
        Assert.Equal("Rosewood Hall", payload?.Location?.VenueName);
    }

    private static EventReferencePayload? Deserialize(string payload)
    {
        return JsonSerializer.Deserialize<EventReferencePayload>(
            payload,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}
