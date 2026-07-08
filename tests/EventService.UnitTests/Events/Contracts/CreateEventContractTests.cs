using EventService.Contracts.Events;

namespace EventService.UnitTests.Events.Contracts;

public sealed class CreateEventContractTests
{
    [Fact]
    public void CreateEventRequest_ExposesRequestValues()
    {
        var request = new CreateEventRequest(
            "Launch plan",
            "2026-05-16",
            "event",
            "Details");

        Assert.Equal("Launch plan", request.EventName);
        Assert.Equal("2026-05-16", request.EventDate);
        Assert.Equal("event", request.EventType);
        Assert.Equal("Details", request.EventDescription);
    }

    [Fact]
    public void CreateEventResponse_ExposesResponseValues()
    {
        var id = Guid.NewGuid();
        var eventDate = new DateOnly(2026, 5, 16);
        var createdAt = new DateTimeOffset(2026, 5, 16, 9, 55, 0, TimeSpan.Zero);
        var updatedAt = createdAt.AddMinutes(1);

        var response = new CreateEventResponse(
            id,
            "Launch plan",
            eventDate,
            "event",
            "Details",
            createdAt,
            updatedAt,
            "token");

        Assert.Equal(id, response.Id);
        Assert.Equal("Launch plan", response.EventName);
        Assert.Equal(eventDate, response.EventDate);
        Assert.Equal("event", response.EventType);
        Assert.Equal("Details", response.EventDescription);
        Assert.Equal(createdAt, response.CreatedAt);
        Assert.Equal(updatedAt, response.UpdatedAt);
        Assert.Equal("token", response.ConcurrencyToken);
    }

    [Fact]
    public void UpdateEventRequest_ExposesRequestValues()
    {
        var request = new UpdateEventRequest(
            "Launch plan",
            "2026-05-16",
            "Details",
            "token");

        Assert.Equal("Launch plan", request.EventName);
        Assert.Equal("2026-05-16", request.EventDate);
        Assert.Equal("Details", request.EventDescription);
        Assert.Equal("token", request.ConcurrencyToken);
    }

    [Fact]
    public void UpdateEventResponse_ExposesResponseValues()
    {
        var id = Guid.NewGuid();
        var eventDate = new DateOnly(2026, 5, 16);
        var createdAt = new DateTimeOffset(2026, 5, 16, 9, 55, 0, TimeSpan.Zero);
        var updatedAt = createdAt.AddMinutes(1);

        var response = new UpdateEventResponse(
            id,
            "Launch plan",
            eventDate,
            "event",
            "Details",
            createdAt,
            updatedAt,
            "token");

        Assert.Equal(id, response.Id);
        Assert.Equal("Launch plan", response.EventName);
        Assert.Equal(eventDate, response.EventDate);
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
        var eventDate = new DateOnly(2026, 5, 16);
        var createdAt = new DateTimeOffset(2026, 5, 16, 9, 55, 0, TimeSpan.Zero);
        var updatedAt = createdAt.AddMinutes(1);

        var response = new GetEventDetailsResponse(
            id,
            "Launch plan",
            eventDate,
            "event",
            "Details",
            createdAt,
            updatedAt,
            "token");

        Assert.Equal(id, response.Id);
        Assert.Equal("Launch plan", response.EventName);
        Assert.Equal(eventDate, response.EventDate);
        Assert.Equal("event", response.EventType);
        Assert.Equal("Details", response.EventDescription);
        Assert.Equal(createdAt, response.CreatedAt);
        Assert.Equal(updatedAt, response.UpdatedAt);
        Assert.Equal("token", response.ConcurrencyToken);
    }
}
