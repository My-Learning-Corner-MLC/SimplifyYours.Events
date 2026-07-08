using EventService.Application.Abstractions.Events;
using EventService.Application.Events.GetEventList;
using EventService.Domain.Events;

namespace EventService.UnitTests.Events.GetEventList;

public sealed class EventListQueryBuilderTests
{
    private readonly DateTimeOffset _now = new(2026, 5, 17, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ApplyFilters_ExcludesSoftDeletedEvents()
    {
        var deletedEvent = CreateEvent("Deleted event", _now.AddDays(1), EventType.Event, null, _now);
        deletedEvent.SoftDelete(_now.AddMinutes(1));
        var events = new[]
        {
            CreateEvent("Visible event", _now.AddDays(1), EventType.Event, null, _now),
            deletedEvent
        };

        var result = EventListQueryBuilder
            .ApplyFilters(events.AsQueryable(), CreateOptions())
            .ToArray();

        var item = Assert.Single(result);
        Assert.Equal("Visible event", item.Name);
    }

    [Fact]
    public void ApplyFilters_WhenSearchMatchesName_ReturnsMatchingEvents()
    {
        var events = new[]
        {
            CreateEvent("Product Launch", _now.AddDays(1), EventType.Event, null, _now),
            CreateEvent("Birthday party", _now.AddDays(1), EventType.Birthday, null, _now)
        };

        var result = EventListQueryBuilder
            .ApplyFilters(events.AsQueryable(), CreateOptions(search: "launch"))
            .ToArray();

        var item = Assert.Single(result);
        Assert.Equal("Product Launch", item.Name);
    }

    [Fact]
    public void ApplyFilters_WhenSearchMatchesDescription_ReturnsMatchingEvents()
    {
        var events = new[]
        {
            CreateEvent("Team event", _now.AddDays(1), EventType.Event, "Annual planning", _now),
            CreateEvent("Birthday party", _now.AddDays(1), EventType.Birthday, "Cake", _now)
        };

        var result = EventListQueryBuilder
            .ApplyFilters(events.AsQueryable(), CreateOptions(search: "planning"))
            .ToArray();

        var item = Assert.Single(result);
        Assert.Equal("Team event", item.Name);
    }

    [Fact]
    public void ApplyFilters_WhenEventTypeIsProvided_ReturnsMatchingEvents()
    {
        var events = new[]
        {
            CreateEvent("Wedding plan", _now.AddDays(1), EventType.Wedding, null, _now),
            CreateEvent("Product launch", _now.AddDays(1), EventType.Event, null, _now)
        };

        var result = EventListQueryBuilder
            .ApplyFilters(events.AsQueryable(), CreateOptions(eventType: EventType.Wedding))
            .ToArray();

        var item = Assert.Single(result);
        Assert.Equal(EventType.Wedding, item.Type);
    }

    [Fact]
    public void ApplyFilters_WhenTimeFilterIsUpcoming_ReturnsTodayAndFutureEvents()
    {
        var events = new[]
        {
            CreateEvent("Past event", _now.AddDays(-1), EventType.Event, null, _now),
            CreateEvent("Today's event", _now, EventType.Event, null, _now),
            CreateEvent("Future event", _now.AddDays(1), EventType.Event, null, _now)
        };

        var result = EventListQueryBuilder
            .ApplyFilters(events.AsQueryable(), CreateOptions(timeFilter: EventTimeFilter.Upcoming))
            .Select(plannedEvent => plannedEvent.Name)
            .ToArray();

        Assert.Equal(new[] { "Today's event", "Future event" }, result);
    }

    [Fact]
    public void ApplyFilters_WhenTimeFilterIsPast_ReturnsPastEventsOnly()
    {
        var events = new[]
        {
            CreateEvent("Past event", _now.AddDays(-1), EventType.Event, null, _now),
            CreateEvent("Today's event", _now, EventType.Event, null, _now),
            CreateEvent("Future event", _now.AddDays(1), EventType.Event, null, _now)
        };

        var result = EventListQueryBuilder
            .ApplyFilters(events.AsQueryable(), CreateOptions(timeFilter: EventTimeFilter.Past))
            .ToArray();

        var item = Assert.Single(result);
        Assert.Equal("Past event", item.Name);
    }

    [Fact]
    public void ApplySorting_WhenCreatedAtDescending_ReturnsNewestFirst()
    {
        var events = new[]
        {
            CreateEvent("Older event", _now.AddDays(1), EventType.Event, null, _now.AddDays(-2)),
            CreateEvent("Newer event", _now.AddDays(1), EventType.Event, null, _now.AddDays(-1))
        };

        var result = EventListQueryBuilder
            .ApplySorting(events.AsQueryable(), EventSortField.CreatedAt, SortDirection.Desc)
            .Select(plannedEvent => plannedEvent.Name)
            .ToArray();

        Assert.Equal(new[] { "Newer event", "Older event" }, result);
    }

    [Fact]
    public void ApplySorting_WhenUpdatedAtAscending_ReturnsOldestModifiedFirst()
    {
        var olderModified = CreateEvent("Older modified", _now.AddDays(1), EventType.Event, null, _now.AddDays(-2));
        var newerModified = CreateEvent("Newer modified", _now.AddDays(1), EventType.Event, null, _now.AddDays(-1));
        olderModified.SoftDelete(_now.AddHours(2));
        olderModified.Restore(_now.AddHours(2));
        newerModified.SoftDelete(_now.AddHours(3));
        newerModified.Restore(_now.AddHours(3));
        var events = new[] { newerModified, olderModified };

        var result = EventListQueryBuilder
            .ApplySorting(events.AsQueryable(), EventSortField.UpdatedAt, SortDirection.Asc)
            .Select(plannedEvent => plannedEvent.Name)
            .ToArray();

        Assert.Equal(new[] { "Older modified", "Newer modified" }, result);
    }

    private static readonly Guid TestTenantId = Guid.Parse("8f10c8b2-12a4-4d6f-9301-7c52e84b7d20");

    private EventListQueryOptions CreateOptions(
        string? search = null,
        EventType? eventType = null,
        EventTimeFilter timeFilter = EventTimeFilter.All)
    {
        return new EventListQueryOptions(
            TestTenantId,
            1,
            20,
            search,
            eventType,
            timeFilter,
            EventSortField.CreatedAt,
            SortDirection.Desc,
            _now);
    }

    private static PlannedEvent CreateEvent(
        string name,
        DateTimeOffset eventDate,
        EventType eventType,
        string? description,
        DateTimeOffset createdAt)
    {
        return PlannedEvent.Create(
            Guid.NewGuid(),
            TestTenantId,
            name,
            DateOnly.FromDateTime(eventDate.DateTime),
            eventType,
            description,
            createdAt);
    }
}
