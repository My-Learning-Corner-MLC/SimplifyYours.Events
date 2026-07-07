using EventService.Contracts.Events;

namespace EventService.UnitTests.Events.Contracts;

public sealed class QueryEventsContractTests
{
    [Fact]
    public void QueryEventsRequest_ExposesRequestValues()
    {
        var request = new QueryEventsRequest(
            2,
            25,
            "launch",
            "event",
            "upcoming",
            "createdAt",
            "desc");

        Assert.Equal(2, request.PageNumber);
        Assert.Equal(25, request.PageSize);
        Assert.Equal("launch", request.Search);
        Assert.Equal("event", request.EventType);
        Assert.Equal("upcoming", request.TimeFilter);
        Assert.Equal("createdAt", request.SortBy);
        Assert.Equal("desc", request.SortDirection);
    }

    [Fact]
    public void EventSummaryResponse_ExposesResponseValues()
    {
        var id = Guid.NewGuid();
        var eventTime = new DateTimeOffset(2026, 6, 1, 9, 0, 0, TimeSpan.Zero);
        var createdAt = eventTime.AddDays(-10);
        var updatedAt = eventTime.AddDays(-2);

        var response = new EventSummaryResponse(
            id,
            "Product launch",
            eventTime,
            "event",
            "Launch details",
            createdAt,
            updatedAt);

        Assert.Equal(id, response.Id);
        Assert.Equal("Product launch", response.EventName);
        Assert.Equal(eventTime, response.EventTime);
        Assert.Equal("event", response.EventType);
        Assert.Equal("Launch details", response.EventDescription);
        Assert.Equal(createdAt, response.CreatedAt);
        Assert.Equal(updatedAt, response.UpdatedAt);
    }

    [Fact]
    public void QueryEventsResponse_ExposesResponseValues()
    {
        var item = new EventSummaryResponse(
            Guid.NewGuid(),
            "Product launch",
            new DateTimeOffset(2026, 6, 1, 9, 0, 0, TimeSpan.Zero),
            "event",
            null,
            new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 5, 17, 10, 0, 0, TimeSpan.Zero));

        var response = new QueryEventsResponse(
            new[] { item },
            2,
            20,
            45,
            3,
            true,
            true);

        Assert.Equal(item, Assert.Single(response.Items));
        Assert.Equal(2, response.PageNumber);
        Assert.Equal(20, response.PageSize);
        Assert.Equal(45, response.TotalCount);
        Assert.Equal(3, response.TotalPages);
        Assert.True(response.HasPreviousPage);
        Assert.True(response.HasNextPage);
    }

    [Fact]
    public void EventSummaryResponse_DefaultsOptionalScheduleAndLocationToNull()
    {
        var response = new EventSummaryResponse(
            Guid.NewGuid(),
            "Product launch",
            new DateTimeOffset(2026, 6, 1, 9, 0, 0, TimeSpan.Zero),
            "event",
            null,
            new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 5, 17, 10, 0, 0, TimeSpan.Zero));

        Assert.Null(response.Location);
        Assert.Null(response.EventStartTime);
        Assert.Null(response.EventEndTime);
    }

    [Fact]
    public void EventSummaryResponse_ExposesLocationAndSchedule()
    {
        var startTime = new DateTimeOffset(2026, 7, 5, 14, 0, 0, TimeSpan.Zero);
        var endTime = new DateTimeOffset(2026, 7, 5, 18, 0, 0, TimeSpan.Zero);
        var location = new EventLocationDto("The Backyard", "414 Maple Street", "Side gate unlocked");

        var response = new EventSummaryResponse(
            Guid.NewGuid(),
            "Mateo turns five",
            startTime,
            "birthday",
            "Backyard party",
            startTime.AddDays(-10),
            startTime.AddDays(-2),
            location,
            startTime,
            endTime);

        Assert.Equal(location, response.Location);
        Assert.Equal(startTime, response.EventStartTime);
        Assert.Equal(endTime, response.EventEndTime);
    }
}
