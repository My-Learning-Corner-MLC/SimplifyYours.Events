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
        var eventDate = new DateOnly(2026, 6, 1);
        var createdAt = new DateTimeOffset(2026, 5, 22, 9, 0, 0, TimeSpan.Zero);
        var updatedAt = new DateTimeOffset(2026, 5, 30, 9, 0, 0, TimeSpan.Zero);

        var response = new EventSummaryResponse(
            id,
            "Product launch",
            eventDate,
            "event",
            "Launch details",
            createdAt,
            updatedAt);

        Assert.Equal(id, response.Id);
        Assert.Equal("Product launch", response.EventName);
        Assert.Equal(eventDate, response.EventDate);
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
            new DateOnly(2026, 6, 1),
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
            new DateOnly(2026, 6, 1),
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
        var eventDate = new DateOnly(2026, 7, 5);
        var startTime = new TimeOnly(14, 0);
        var endTime = new TimeOnly(18, 0);
        var location = new EventLocationDto("The Backyard", "414 Maple Street", "Side gate unlocked");

        var response = new EventSummaryResponse(
            Guid.NewGuid(),
            "Mateo turns five",
            eventDate,
            "birthday",
            "Backyard party",
            new DateTimeOffset(2026, 6, 25, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 7, 3, 9, 0, 0, TimeSpan.Zero),
            location,
            startTime,
            endTime);

        Assert.Equal(location, response.Location);
        Assert.Equal(startTime, response.EventStartTime);
        Assert.Equal(endTime, response.EventEndTime);
    }
}
