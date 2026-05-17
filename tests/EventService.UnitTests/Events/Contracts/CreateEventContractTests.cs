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
            updatedAt);

        Assert.Equal(id, response.Id);
        Assert.Equal("Launch plan", response.EventName);
        Assert.Equal(eventTime, response.EventTime);
        Assert.Equal("event", response.EventType);
        Assert.Equal("Details", response.EventDescription);
        Assert.Equal(createdAt, response.CreatedAt);
        Assert.Equal(updatedAt, response.UpdatedAt);
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
            updatedAt);

        Assert.Equal(id, response.Id);
        Assert.Equal("Launch plan", response.EventName);
        Assert.Equal(eventTime, response.EventTime);
        Assert.Equal("event", response.EventType);
        Assert.Equal("Details", response.EventDescription);
        Assert.Equal(createdAt, response.CreatedAt);
        Assert.Equal(updatedAt, response.UpdatedAt);
    }
}
