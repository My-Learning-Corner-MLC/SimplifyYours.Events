using EventService.Contracts.Events;

namespace EventService.UnitTests.Events.Contracts;

public sealed class GetEventDetailsContractTests
{
    [Fact]
    public void GetEventDetailsResponse_ExposesCoreResponseValues()
    {
        var id = Guid.NewGuid();
        var eventDate = new DateOnly(2026, 6, 1);
        var createdAt = new DateTimeOffset(2026, 5, 22, 9, 0, 0, TimeSpan.Zero);
        var updatedAt = new DateTimeOffset(2026, 5, 30, 9, 0, 0, TimeSpan.Zero);

        var response = new GetEventDetailsResponse(
            id,
            "Product launch",
            eventDate,
            "event",
            "Launch details",
            createdAt,
            updatedAt,
            "token");

        Assert.Equal(id, response.Id);
        Assert.Equal("Product launch", response.EventName);
        Assert.Equal(eventDate, response.EventDate);
        Assert.Equal("event", response.EventType);
        Assert.Equal("Launch details", response.EventDescription);
        Assert.Equal(createdAt, response.CreatedAt);
        Assert.Equal(updatedAt, response.UpdatedAt);
        Assert.Equal("token", response.ConcurrencyToken);
    }

    [Fact]
    public void GetEventDetailsResponse_DefaultsOptionalScheduleAndLocationToNull()
    {
        var response = new GetEventDetailsResponse(
            Guid.NewGuid(),
            "Product launch",
            new DateOnly(2026, 6, 1),
            "event",
            null,
            new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 5, 17, 10, 0, 0, TimeSpan.Zero),
            "token");

        Assert.Null(response.Location);
        Assert.Null(response.TimeZoneId);
        Assert.Null(response.EventStartTime);
        Assert.Null(response.EventEndTime);
    }

    [Fact]
    public void GetEventDetailsResponse_ExposesLocationAndSchedule()
    {
        var eventDate = new DateOnly(2026, 7, 5);
        var startTime = new TimeOnly(14, 0);
        var endTime = new TimeOnly(18, 0);
        var location = new EventLocationDto("The Backyard", "414 Maple Street", "Side gate unlocked");

        var response = new GetEventDetailsResponse(
            Guid.NewGuid(),
            "Mateo turns five",
            eventDate,
            "birthday",
            "Backyard party",
            new DateTimeOffset(2026, 6, 25, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 7, 3, 9, 0, 0, TimeSpan.Zero),
            "token",
            location,
            "America/Los_Angeles",
            startTime,
            endTime);

        Assert.Equal(location, response.Location);
        Assert.Equal("America/Los_Angeles", response.TimeZoneId);
        Assert.Equal(startTime, response.EventStartTime);
        Assert.Equal(endTime, response.EventEndTime);
    }
}
