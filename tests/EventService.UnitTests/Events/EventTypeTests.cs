using EventService.Application.Events;
using EventService.Domain.Events;

namespace EventService.UnitTests.Events;

public sealed class EventTypeTests
{
    [Fact]
    public void EventType_KeepsExistingValuesStable()
    {
        Assert.Equal(1, (int)EventType.Birthday);
        Assert.Equal(2, (int)EventType.Wedding);
        Assert.Equal(3, (int)EventType.Event);
    }

    [Fact]
    public void EventType_DefinesExpandedValuesWithoutCollisions()
    {
        var values = Enum.GetValues<EventType>();

        Assert.Equal(7, values.Length);
        Assert.Equal(values.Length, values.Distinct().Count());
        Assert.Contains(EventType.Anniversary, values);
        Assert.Contains(EventType.Launch, values);
        Assert.Contains(EventType.Dinner, values);
        Assert.Contains(EventType.Other, values);
    }

    [Theory]
    [InlineData("birthday", EventType.Birthday)]
    [InlineData("wedding", EventType.Wedding)]
    [InlineData("event", EventType.Event)]
    [InlineData("anniversary", EventType.Anniversary)]
    [InlineData("launch", EventType.Launch)]
    [InlineData("dinner", EventType.Dinner)]
    [InlineData("other", EventType.Other)]
    [InlineData("Birthday", EventType.Birthday)]
    [InlineData("ANNIVERSARY", EventType.Anniversary)]
    [InlineData("Other", EventType.Other)]
    public void TryParseEventType_AcceptsAllNamesCaseInsensitively(string value, EventType expected)
    {
        var parsed = EventParsing.TryParseEventType(value, out var eventType);

        Assert.True(parsed);
        Assert.Equal(expected, eventType);
    }

    [Fact]
    public void TryParseEventType_KeepsPersistedEventStringParsable()
    {
        var parsed = EventParsing.TryParseEventType("Event", out var eventType);

        Assert.True(parsed);
        Assert.Equal(EventType.Event, eventType);
    }

    [Theory]
    [InlineData("conference")]
    [InlineData("99")]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void TryParseEventType_RejectsUnknownValues(string? value)
    {
        var parsed = EventParsing.TryParseEventType(value, out _);

        Assert.False(parsed);
    }
}
