using EventService.Application.Abstractions.Events;
using EventService.Application.Authorization;
using EventService.Application.Events.GetEventDetails;
using EventService.Domain.Events;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EventService.UnitTests.Events.GetEventDetails;

public sealed class GetEventDetailsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenEventExists_ReturnsEventDetails()
    {
        var eventId = Guid.NewGuid();
        var createdAt = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var eventDate = DateOnly.FromDateTime(createdAt.DateTime).AddDays(7);
        var currentUser = new CurrentUser(Guid.NewGuid(), Guid.NewGuid());
        var plannedEvent = PlannedEvent.Create(
            eventId,
            currentUser.TenantId,
            "Product launch",
            eventDate,
            EventType.Event,
            "Launch details",
            createdAt);
        var repository = new Mock<IEventRepository>();
        repository
            .Setup(repo => repo.GetByIdAsync(eventId, currentUser.TenantId, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(plannedEvent);
        var handler = new GetEventDetailsQueryHandler(
            repository.Object,
            NullLogger<GetEventDetailsQueryHandler>.Instance);

        var result = await handler.Handle(
            new GetEventDetailsQuery(eventId) { CurrentUser = currentUser },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(eventId, result.Event.Id);
        Assert.Equal("Product launch", result.Event.EventName);
        Assert.Equal(eventDate, result.Event.EventDate);
        Assert.Equal("event", result.Event.EventType);
        Assert.Equal("Launch details", result.Event.EventDescription);
        Assert.Equal(createdAt, result.Event.CreatedAt);
        Assert.Equal(createdAt, result.Event.UpdatedAt);
        Assert.False(string.IsNullOrWhiteSpace(result.Event.ConcurrencyToken));
        repository.Verify(
            repo => repo.GetByIdAsync(eventId, currentUser.TenantId, It.IsAny<CancellationToken>(), true),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MapsLocationAndScheduleWhenPresent()
    {
        var eventId = Guid.NewGuid();
        var createdAt = new DateTimeOffset(2026, 6, 25, 9, 0, 0, TimeSpan.Zero);
        var eventDate = new DateOnly(2026, 7, 5);
        var startTime = new TimeOnly(14, 0);
        var endTime = new TimeOnly(18, 0);
        var location = EventLocation.Create("The Backyard", "414 Maple Street", "Side gate unlocked");
        var currentUser = new CurrentUser(Guid.NewGuid(), Guid.NewGuid());
        var plannedEvent = PlannedEvent.Create(
            eventId,
            currentUser.TenantId,
            "Mateo turns five",
            eventDate,
            EventType.Birthday,
            "Backyard party",
            createdAt,
            location,
            "America/Los_Angeles",
            startTime,
            endTime);
        var repository = new Mock<IEventRepository>();
        repository
            .Setup(repo => repo.GetByIdAsync(eventId, currentUser.TenantId, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(plannedEvent);
        var handler = new GetEventDetailsQueryHandler(
            repository.Object,
            NullLogger<GetEventDetailsQueryHandler>.Instance);

        var result = await handler.Handle(
            new GetEventDetailsQuery(eventId) { CurrentUser = currentUser },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Event.Location);
        Assert.Equal("The Backyard", result.Event.Location!.VenueName);
        Assert.Equal("414 Maple Street", result.Event.Location.Address);
        Assert.Equal("Side gate unlocked", result.Event.Location.Notes);
        Assert.Equal("America/Los_Angeles", result.Event.TimeZoneId);
        Assert.Equal(startTime, result.Event.EventStartTime);
        Assert.Equal(endTime, result.Event.EventEndTime);
    }

    [Fact]
    public async Task Handle_WhenEventDoesNotExist_ReturnsNull()
    {
        var eventId = Guid.NewGuid();
        var currentUser = new CurrentUser(Guid.NewGuid(), Guid.NewGuid());
        var repository = new Mock<IEventRepository>();
        repository
            .Setup(repo => repo.GetByIdAsync(eventId, currentUser.TenantId, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync((PlannedEvent?)null);
        var handler = new GetEventDetailsQueryHandler(
            repository.Object,
            NullLogger<GetEventDetailsQueryHandler>.Instance);

        var result = await handler.Handle(
            new GetEventDetailsQuery(eventId) { CurrentUser = currentUser },
            CancellationToken.None);

        Assert.Null(result);
        repository.Verify(
            repo => repo.GetByIdAsync(eventId, currentUser.TenantId, It.IsAny<CancellationToken>(), true),
            Times.Once);
    }
}
