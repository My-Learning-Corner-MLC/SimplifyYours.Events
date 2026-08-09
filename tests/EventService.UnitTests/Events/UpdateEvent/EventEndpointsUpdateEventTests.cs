using EventService.Api.Endpoints;
using EventService.Application.Events;
using EventService.Application.Events.UpdateEvent;
using EventService.Contracts.Events;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventService.UnitTests.Events.UpdateEvent;

public sealed class EventEndpointsUpdateEventTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 6, 25, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task UpdateEventAsync_WhenLocationAndTimeZoneArePresent_ForwardsAndEchoesThem()
    {
        var eventId = Guid.NewGuid();
        var eventDetails = CreateEventDetails(
            eventId,
            new EventLocationDetails("The Riverside Room", "20 Riverside Drive", "Parking behind the building."),
            "Europe/Berlin");
        var sender = new Mock<ISender>();
        sender
            .Setup(s => s.Send(
                It.Is<UpdateEventCommand>(command =>
                    command.Location != null
                    && command.Location.VenueName == "The Riverside Room"
                    && command.Location.Address == "20 Riverside Drive"
                    && command.Location.Notes == "Parking behind the building."
                    && command.TimeZoneId == "Europe/Berlin"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(UpdateEventResult.Updated(eventDetails));

        var result = await EventEndpoints.UpdateEventAsync(
            eventId,
            new UpdateEventRequest(
                "Launch plan",
                "2026-07-05",
                null,
                "dG9rZW4=",
                new EventLocationDto("The Riverside Room", "20 Riverside Drive", "Parking behind the building."),
                "Europe/Berlin"),
            new DefaultHttpContext(),
            sender.Object,
            CancellationToken.None);

        var ok = Assert.IsType<Ok<UpdateEventResponse>>(result);
        var response = ok.Value!;
        Assert.Equal(eventId, response.Id);
        Assert.NotNull(response.Location);
        Assert.Equal("The Riverside Room", response.Location!.VenueName);
        Assert.Equal("20 Riverside Drive", response.Location.Address);
        Assert.Equal("Parking behind the building.", response.Location.Notes);
        Assert.Equal("Europe/Berlin", response.TimeZoneId);
    }

    [Fact]
    public async Task UpdateEventAsync_WhenLocationAndTimeZoneAreOmitted_SendsNullsAndEchoesNulls()
    {
        var eventId = Guid.NewGuid();
        var sender = new Mock<ISender>();
        sender
            .Setup(s => s.Send(
                It.Is<UpdateEventCommand>(command => command.Location == null && command.TimeZoneId == null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(UpdateEventResult.Updated(CreateEventDetails(eventId, null, null)));

        var result = await EventEndpoints.UpdateEventAsync(
            eventId,
            new UpdateEventRequest("Launch plan", "2026-07-05", null, "dG9rZW4="),
            new DefaultHttpContext(),
            sender.Object,
            CancellationToken.None);

        var ok = Assert.IsType<Ok<UpdateEventResponse>>(result);
        Assert.Null(ok.Value!.Location);
        Assert.Null(ok.Value.TimeZoneId);
    }

    [Fact]
    public async Task UpdateEventAsync_WhenValidationFails_ReturnsFieldKeyedProblemDetails()
    {
        var sender = new Mock<ISender>();
        sender
            .Setup(s => s.Send(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(
            [
                new ValidationFailure(
                    nameof(UpdateEventCommand.TimeZoneId),
                    "Time zone must be a valid IANA time zone id.")
            ]));

        var result = await EventEndpoints.UpdateEventAsync(
            Guid.NewGuid(),
            new UpdateEventRequest("Launch plan", "2026-07-05", null, "dG9rZW4=", null, "Not/AZone"),
            new DefaultHttpContext(),
            sender.Object,
            CancellationToken.None);

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        Assert.Equal("application/problem+json", problem.ContentType);
        var details = Assert.IsType<HttpValidationProblemDetails>(problem.ProblemDetails);
        Assert.Equal(
            "Time zone must be a valid IANA time zone id.",
            Assert.Single(details.Errors[nameof(UpdateEventCommand.TimeZoneId)]));
        Assert.True(details.Extensions.ContainsKey("correlationId"));
    }

    [Fact]
    public async Task UpdateEventAsync_WhenConcurrencyTokenIsStale_ReturnsConflict()
    {
        var sender = new Mock<ISender>();
        sender
            .Setup(s => s.Send(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UpdateEventResult.Conflict());

        var result = await EventEndpoints.UpdateEventAsync(
            Guid.NewGuid(),
            new UpdateEventRequest("Launch plan", "2026-07-05", null, "dG9rZW4="),
            new DefaultHttpContext(),
            sender.Object,
            CancellationToken.None);

        var problem = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, problem.StatusCode!.Value);
    }

    [Fact]
    public async Task UpdateEventAsync_WhenEventDoesNotExist_ReturnsNotFound()
    {
        var sender = new Mock<ISender>();
        sender
            .Setup(s => s.Send(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UpdateEventResult.NotFound());

        var result = await EventEndpoints.UpdateEventAsync(
            Guid.NewGuid(),
            new UpdateEventRequest("Launch plan", "2026-07-05", null, "dG9rZW4="),
            new DefaultHttpContext(),
            sender.Object,
            CancellationToken.None);

        var problem = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problem.StatusCode!.Value);
    }

    private static EventDetails CreateEventDetails(
        Guid eventId,
        EventLocationDetails? location,
        string? timeZoneId)
    {
        return new EventDetails(
            eventId,
            "Launch plan",
            new DateOnly(2026, 7, 5),
            "event",
            null,
            CreatedAt,
            CreatedAt.AddMinutes(5),
            "dG9rZW4=",
            location,
            timeZoneId);
    }
}
