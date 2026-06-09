using EventService.Application.Abstractions.Common;
using EventService.Application.Abstractions.Events;
using EventService.Application.Abstractions.IntegrationEvents;
using EventService.Application.Events.CreateEvent;
using EventService.Domain.Events;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SimplifyYours.Event.Abstractions;

namespace EventService.UnitTests.Events.CreateEvent;

public sealed class CreateEventCommandHandlerTests
{
    [Fact]
    public async Task Handle_CreatesAndSavesEvent()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var eventTime = now.AddDays(3);
        PlannedEvent? savedEvent = null;
        var repository = new Mock<IEventRepository>();
        repository
            .Setup(repo => repo.AddAsync(It.IsAny<PlannedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<PlannedEvent, CancellationToken>((plannedEvent, _) => savedEvent = plannedEvent)
            .Returns(Task.CompletedTask);
        var timeProvider = new Mock<TimeProvider>();
        timeProvider.Setup(provider => provider.GetUtcNow()).Returns(now);
        var outbox = new Mock<IIntegrationEventOutbox>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new CreateEventCommandHandler(
            repository.Object,
            outbox.Object,
            unitOfWork.Object,
            timeProvider.Object,
            NullLogger<CreateEventCommandHandler>.Instance);

        var result = await handler.Handle(
            new CreateEventCommand("Wedding plan", eventTime.ToString("O"), "wedding", "Details"),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Event.Id);
        Assert.Equal("Wedding plan", result.Event.EventName);
        Assert.Equal(eventTime, result.Event.EventTime);
        Assert.Equal("wedding", result.Event.EventType);
        Assert.Equal("Details", result.Event.EventDescription);
        Assert.Equal(now, result.Event.CreatedAt);
        Assert.Equal(now, result.Event.UpdatedAt);
        Assert.False(string.IsNullOrWhiteSpace(result.Event.ConcurrencyToken));
        Assert.NotNull(savedEvent);
        Assert.Equal(result.Event.Id, savedEvent.Id);
        Assert.False(savedEvent.IsDeleted);
        repository.Verify(
            repo => repo.AddAsync(It.IsAny<PlannedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
        outbox.Verify(
            publisher => publisher.AddAsync(
                It.Is<IntegrationEventEnvelope>(message => message.EventType == "EventCreated"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        timeProvider.Verify(provider => provider.GetUtcNow(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEventTimeIsOmitted_DefaultsToNow()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        PlannedEvent? savedEvent = null;
        var repository = new Mock<IEventRepository>();
        repository
            .Setup(repo => repo.AddAsync(It.IsAny<PlannedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<PlannedEvent, CancellationToken>((plannedEvent, _) => savedEvent = plannedEvent)
            .Returns(Task.CompletedTask);
        var timeProvider = new Mock<TimeProvider>();
        timeProvider.Setup(provider => provider.GetUtcNow()).Returns(now);
        var outbox = new Mock<IIntegrationEventOutbox>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new CreateEventCommandHandler(
            repository.Object,
            outbox.Object,
            unitOfWork.Object,
            timeProvider.Object,
            NullLogger<CreateEventCommandHandler>.Instance);

        var result = await handler.Handle(
            new CreateEventCommand("Birthday plan", null, "birthday", null),
            CancellationToken.None);

        Assert.Equal(now, result.Event.EventTime);
        Assert.Equal(now, savedEvent?.EventTime);
        repository.Verify(
            repo => repo.AddAsync(It.IsAny<PlannedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
        outbox.Verify(
            publisher => publisher.AddAsync(It.IsAny<IntegrationEventEnvelope>(), It.IsAny<CancellationToken>()),
            Times.Once);
        unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEventTimeIsInvalid_Throws()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var repository = new Mock<IEventRepository>();
        var timeProvider = new Mock<TimeProvider>();
        timeProvider.Setup(provider => provider.GetUtcNow()).Returns(now);
        var outbox = new Mock<IIntegrationEventOutbox>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new CreateEventCommandHandler(
            repository.Object,
            outbox.Object,
            unitOfWork.Object,
            timeProvider.Object,
            NullLogger<CreateEventCommandHandler>.Instance);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(
            new CreateEventCommand("Birthday plan", "not-a-date", "birthday", null),
            CancellationToken.None));

        repository.Verify(
            repo => repo.AddAsync(It.IsAny<PlannedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
        outbox.Verify(
            publisher => publisher.AddAsync(It.IsAny<IntegrationEventEnvelope>(), It.IsAny<CancellationToken>()),
            Times.Never);
        unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenEventTypeIsInvalid_Throws()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var repository = new Mock<IEventRepository>();
        var timeProvider = new Mock<TimeProvider>();
        timeProvider.Setup(provider => provider.GetUtcNow()).Returns(now);
        var outbox = new Mock<IIntegrationEventOutbox>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new CreateEventCommandHandler(
            repository.Object,
            outbox.Object,
            unitOfWork.Object,
            timeProvider.Object,
            NullLogger<CreateEventCommandHandler>.Instance);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(
            new CreateEventCommand("Birthday plan", null, "conference", null),
            CancellationToken.None));

        repository.Verify(
            repo => repo.AddAsync(It.IsAny<PlannedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
        outbox.Verify(
            publisher => publisher.AddAsync(It.IsAny<IntegrationEventEnvelope>(), It.IsAny<CancellationToken>()),
            Times.Never);
        unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
