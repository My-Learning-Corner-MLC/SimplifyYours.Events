using EventService.Application.Abstractions.Events;
using EventService.Application.Events.GetEventDetails;
using EventService.Domain.Events;
using Moq;

namespace EventService.UnitTests.Events.GetEventDetails;

public sealed class GetEventDetailsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenEventExists_ReturnsEventDetails()
    {
        var eventId = Guid.NewGuid();
        var createdAt = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var eventTime = createdAt.AddDays(7);
        var plannedEvent = PlannedEvent.Create(
            eventId,
            "Product launch",
            eventTime,
            EventType.Event,
            "Launch details",
            createdAt);
        var repository = new Mock<IEventRepository>();
        repository
            .Setup(repo => repo.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plannedEvent);
        var handler = new GetEventDetailsQueryHandler(repository.Object);

        var result = await handler.Handle(new GetEventDetailsQuery(eventId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(eventId, result.Id);
        Assert.Equal("Product launch", result.EventName);
        Assert.Equal(eventTime, result.EventTime);
        Assert.Equal("event", result.EventType);
        Assert.Equal("Launch details", result.EventDescription);
        Assert.Equal(createdAt, result.CreatedAt);
        Assert.Equal(createdAt, result.UpdatedAt);
        repository.Verify(
            repo => repo.GetByIdAsync(eventId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEventDoesNotExist_ReturnsNull()
    {
        var eventId = Guid.NewGuid();
        var repository = new Mock<IEventRepository>();
        repository
            .Setup(repo => repo.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlannedEvent?)null);
        var handler = new GetEventDetailsQueryHandler(repository.Object);

        var result = await handler.Handle(new GetEventDetailsQuery(eventId), CancellationToken.None);

        Assert.Null(result);
        repository.Verify(
            repo => repo.GetByIdAsync(eventId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
