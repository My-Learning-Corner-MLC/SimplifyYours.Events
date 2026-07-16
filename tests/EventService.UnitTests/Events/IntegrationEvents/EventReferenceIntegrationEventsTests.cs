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
}
