using EventService.Contracts.Events;

namespace EventService.UnitTests.Events.Contracts;

public sealed class CreateEventContractTests
{
    [Fact]
    public void CreateEventRequest_ExposesRequestValues()
    {
        var request = new CreateEventRequest(
            "Launch plan",
            "2026-05-16T10:00:00Z",
            "event",
            "Details");

        Assert.Equal("Launch plan", request.EventName);
        Assert.Equal("2026-05-16T10:00:00Z", request.EventTime);
        Assert.Equal("event", request.EventType);
        Assert.Equal("Details", request.EventDescription);
        Assert.Null(request.Location);
        Assert.Null(request.TimeZoneId);
    }

    [Fact]
    public void CreateEventRequest_ExposesLocationAndTimeZoneValues()
    {
        var location = new EventLocationDto(
            "The Backyard",
            "414 Maple Street, Brooklyn, NY 11215",
            "Side gate unlocked from 1:30.");

        var request = new CreateEventRequest(
            "Mateo turns five",
            "2026-05-17T14:00:00Z",
            "birthday",
            null,
            location,
            "America/Los_Angeles");

        Assert.Same(location, request.Location);
        Assert.Equal("The Backyard", request.Location.VenueName);
        Assert.Equal("414 Maple Street, Brooklyn, NY 11215", request.Location.Address);
        Assert.Equal("Side gate unlocked from 1:30.", request.Location.Notes);
        Assert.Equal("America/Los_Angeles", request.TimeZoneId);
    }

    [Fact]
    public void CreateEventRequest_ExposesStartAndEndTime()
    {
        var request = new CreateEventRequest(
            "Dinner party",
            "2026-05-17T00:00:00Z",
            "dinner",
            null,
            EventStartTime: "2026-05-17T14:00:00Z",
            EventEndTime: "2026-05-17T18:00:00Z");

        Assert.Equal("2026-05-17T14:00:00Z", request.EventStartTime);
        Assert.Equal("2026-05-17T18:00:00Z", request.EventEndTime);
    }

    [Fact]
    public void CreateEventResponse_ExposesResponseValues()
    {
        var id = Guid.NewGuid();
        var eventTime = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var createdAt = eventTime.AddMinutes(-5);
        var updatedAt = eventTime.AddMinutes(-4);

        var response = new CreateEventResponse(
            id,
            "Launch plan",
            eventTime,
            "event",
            "Details",
            createdAt,
            updatedAt,
            "token");

        Assert.Equal(id, response.Id);
        Assert.Equal("Launch plan", response.EventName);
        Assert.Equal(eventTime, response.EventTime);
        Assert.Equal("event", response.EventType);
        Assert.Equal("Details", response.EventDescription);
        Assert.Equal(createdAt, response.CreatedAt);
        Assert.Equal(updatedAt, response.UpdatedAt);
        Assert.Equal("token", response.ConcurrencyToken);
        Assert.Null(response.Location);
        Assert.Null(response.TimeZoneId);
    }

    [Fact]
    public void CreateEventResponse_ExposesLocationAndTimeZoneValues()
    {
        var location = new EventLocationDto(
            "The Backyard",
            "414 Maple Street, Brooklyn, NY 11215",
            null);

        var response = new CreateEventResponse(
            Guid.NewGuid(),
            "Mateo turns five",
            new DateTimeOffset(2026, 5, 17, 14, 0, 0, TimeSpan.Zero),
            "birthday",
            null,
            new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero),
            "token",
            location,
            "America/Los_Angeles");

        Assert.Same(location, response.Location);
        Assert.Equal("America/Los_Angeles", response.TimeZoneId);
    }

    [Fact]
    public void UpdateEventRequest_ExposesRequestValues()
    {
        var request = new UpdateEventRequest(
            "Launch plan",
            "2026-05-16T10:00:00Z",
            "Details",
            "token");

        Assert.Equal("Launch plan", request.EventName);
        Assert.Equal("2026-05-16T10:00:00Z", request.EventTime);
        Assert.Equal("Details", request.EventDescription);
        Assert.Equal("token", request.ConcurrencyToken);
    }

    [Fact]
    public void UpdateEventResponse_ExposesResponseValues()
    {
        var id = Guid.NewGuid();
        var eventTime = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var createdAt = eventTime.AddMinutes(-5);
        var updatedAt = eventTime.AddMinutes(-4);

        var response = new UpdateEventResponse(
            id,
            "Launch plan",
            eventTime,
            "event",
            "Details",
            createdAt,
            updatedAt,
            "token");

        Assert.Equal(id, response.Id);
        Assert.Equal("Launch plan", response.EventName);
        Assert.Equal(eventTime, response.EventTime);
        Assert.Equal("event", response.EventType);
        Assert.Equal("Details", response.EventDescription);
        Assert.Equal(createdAt, response.CreatedAt);
        Assert.Equal(updatedAt, response.UpdatedAt);
        Assert.Equal("token", response.ConcurrencyToken);
    }

    [Fact]
    public void GetEventDetailsResponse_ExposesResponseValues()
    {
        var id = Guid.NewGuid();
        var eventTime = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var createdAt = eventTime.AddMinutes(-5);
        var updatedAt = eventTime.AddMinutes(-4);

        var response = new GetEventDetailsResponse(
            id,
            "Launch plan",
            eventTime,
            "event",
            "Details",
            createdAt,
            updatedAt,
            "token");

        Assert.Equal(id, response.Id);
        Assert.Equal("Launch plan", response.EventName);
        Assert.Equal(eventTime, response.EventTime);
        Assert.Equal("event", response.EventType);
        Assert.Equal("Details", response.EventDescription);
        Assert.Equal(createdAt, response.CreatedAt);
        Assert.Equal(updatedAt, response.UpdatedAt);
        Assert.Equal("token", response.ConcurrencyToken);
    }
}
