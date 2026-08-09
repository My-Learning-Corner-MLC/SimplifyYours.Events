using EventService.Contracts.Events;

namespace EventService.UnitTests.Events.Contracts;

public sealed class UpdateEventContractTests
{
    [Fact]
    public void UpdateEventRequest_WhenLocationAndTimeZoneOmitted_ExposesNulls()
    {
        var request = new UpdateEventRequest(
            "Launch plan",
            "2026-05-16",
            "Details",
            "dG9rZW4=");

        Assert.Equal("Launch plan", request.EventName);
        Assert.Equal("2026-05-16", request.EventDate);
        Assert.Equal("Details", request.EventDescription);
        Assert.Equal("dG9rZW4=", request.ConcurrencyToken);
        Assert.Null(request.Location);
        Assert.Null(request.TimeZoneId);
    }

    [Fact]
    public void UpdateEventRequest_ExposesLocationAndTimeZoneValues()
    {
        var location = new EventLocationDto(
            "The Riverside Room",
            "20 Riverside Drive",
            "Parking behind the building.");

        var request = new UpdateEventRequest(
            "Launch plan",
            "2026-05-16",
            null,
            "dG9rZW4=",
            location,
            "Europe/Berlin");

        Assert.Same(location, request.Location);
        Assert.Equal("Europe/Berlin", request.TimeZoneId);
    }

    [Fact]
    public void UpdateEventResponse_EchoesLocationAndTimeZoneValues()
    {
        var createdAt = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var location = new EventLocationDto("The Riverside Room", "20 Riverside Drive", null);

        var response = new UpdateEventResponse(
            Guid.NewGuid(),
            "Launch plan",
            new DateOnly(2026, 5, 17),
            "event",
            null,
            createdAt,
            createdAt.AddMinutes(5),
            "dG9rZW4=",
            location,
            "Europe/Berlin");

        Assert.Same(location, response.Location);
        Assert.Equal("Europe/Berlin", response.TimeZoneId);
    }

    [Fact]
    public void UpdateEventResponse_WhenLocationAndTimeZoneOmitted_ExposesNulls()
    {
        var createdAt = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);

        var response = new UpdateEventResponse(
            Guid.NewGuid(),
            "Launch plan",
            new DateOnly(2026, 5, 17),
            "event",
            null,
            createdAt,
            createdAt.AddMinutes(5),
            "dG9rZW4=");

        Assert.Null(response.Location);
        Assert.Null(response.TimeZoneId);
    }
}
