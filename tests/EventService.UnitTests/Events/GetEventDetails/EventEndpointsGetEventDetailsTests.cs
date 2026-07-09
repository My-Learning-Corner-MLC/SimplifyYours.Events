using EventService.Api.Endpoints;
using EventService.Application.Events;
using EventService.Application.Events.GetEventDetails;
using EventService.Contracts.Events;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace EventService.UnitTests.Events.GetEventDetails;

public sealed class EventEndpointsGetEventDetailsTests
{
    [Fact]
    public async Task GetEventDetailsAsync_WhenLocationAndScheduleArePresent_MapsThemOntoTheResponse()
    {
        var eventId = Guid.NewGuid();
        var createdAt = new DateTimeOffset(2026, 6, 25, 9, 0, 0, TimeSpan.Zero);
        var eventDetails = new EventDetails(
            eventId,
            "Mateo turns five",
            new DateOnly(2026, 7, 5),
            "birthday",
            "Backyard party",
            createdAt,
            createdAt,
            "token",
            new EventLocationDetails("The Backyard", "414 Maple Street", "Side gate unlocked"),
            "America/Los_Angeles",
            new TimeOnly(14, 0),
            new TimeOnly(18, 0));
        var sender = new Mock<ISender>();
        sender
            .Setup(s => s.Send(It.Is<GetEventDetailsQuery>(q => q.EventId == eventId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetEventDetailsResult(eventDetails));

        var result = await EventEndpoints.GetEventDetailsAsync(
            eventId,
            new DefaultHttpContext(),
            sender.Object,
            CancellationToken.None);

        var ok = Assert.IsType<Ok<GetEventDetailsResponse>>(result);
        var response = ok.Value!;
        Assert.Equal(eventId, response.Id);
        Assert.NotNull(response.Location);
        Assert.Equal("The Backyard", response.Location!.VenueName);
        Assert.Equal("414 Maple Street", response.Location.Address);
        Assert.Equal("Side gate unlocked", response.Location.Notes);
        Assert.Equal("America/Los_Angeles", response.TimeZoneId);
        Assert.Equal(new TimeOnly(14, 0), response.EventStartTime);
        Assert.Equal(new TimeOnly(18, 0), response.EventEndTime);
    }

    [Fact]
    public async Task GetEventDetailsAsync_WhenLocationAndScheduleAreAbsent_ReturnsNullFieldsOnTheResponse()
    {
        var eventId = Guid.NewGuid();
        var createdAt = new DateTimeOffset(2026, 6, 25, 9, 0, 0, TimeSpan.Zero);
        var eventDetails = new EventDetails(
            eventId,
            "Product launch",
            new DateOnly(2026, 7, 5),
            "event",
            null,
            createdAt,
            createdAt,
            "token");
        var sender = new Mock<ISender>();
        sender
            .Setup(s => s.Send(It.IsAny<GetEventDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetEventDetailsResult(eventDetails));

        var result = await EventEndpoints.GetEventDetailsAsync(
            eventId,
            new DefaultHttpContext(),
            sender.Object,
            CancellationToken.None);

        var ok = Assert.IsType<Ok<GetEventDetailsResponse>>(result);
        var response = ok.Value!;
        Assert.Null(response.Location);
        Assert.Null(response.TimeZoneId);
        Assert.Null(response.EventStartTime);
        Assert.Null(response.EventEndTime);
    }

    [Fact]
    public async Task GetEventDetailsAsync_WhenEventDoesNotExist_ReturnsNotFound()
    {
        var eventId = Guid.NewGuid();
        var sender = new Mock<ISender>();
        sender
            .Setup(s => s.Send(It.IsAny<GetEventDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetEventDetailsResult?)null);

        var result = await EventEndpoints.GetEventDetailsAsync(
            eventId,
            new DefaultHttpContext(),
            sender.Object,
            CancellationToken.None);

        var problem = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problem.StatusCode!.Value);
    }
}
