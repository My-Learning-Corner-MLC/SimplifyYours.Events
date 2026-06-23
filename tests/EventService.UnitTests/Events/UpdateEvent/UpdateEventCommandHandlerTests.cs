using EventService.Application.Abstractions.Events;
using EventService.Application.Abstractions.IntegrationEvents;
using EventService.Application.Authorization;
using EventService.Application.Events.UpdateEvent;
using EventService.Domain.Events;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SimplifyYours.Event.Abstractions;

namespace EventService.UnitTests.Events.UpdateEvent;

public sealed class UpdateEventCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenEventExists_UpdatesAndSavesEvent()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var eventId = Guid.NewGuid();
        var currentUser = new CurrentUser(Guid.NewGuid(), Guid.NewGuid(), new[] { "events.update" });
        var plannedEvent = PlannedEvent.Create(
            eventId,
            currentUser.TenantId,
            "Launch plan",
            now.AddDays(1),
            EventType.Event,
            "Old details",
            now);
        var expectedToken = plannedEvent.ConcurrencyToken;
        var expectedTokenText = Convert.ToBase64String(expectedToken);
        var newEventTime = now.AddDays(2);
        var repository = new Mock<IEventRepository>();
        repository
            .Setup(repo => repo.GetByIdAsync(eventId, currentUser.TenantId, It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(plannedEvent);
        repository
            .Setup(repo => repo.UpdateAsync(
                plannedEvent,
                It.Is<byte[]>(token => token.SequenceEqual(expectedToken)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var timeProvider = new Mock<TimeProvider>();
        timeProvider.Setup(provider => provider.GetUtcNow()).Returns(now.AddMinutes(10));
        var outbox = new Mock<IIntegrationEventOutbox>();
        var handler = new UpdateEventCommandHandler(
            repository.Object,
            outbox.Object,
            timeProvider.Object,
            NullLogger<UpdateEventCommandHandler>.Instance);

        var result = await handler.Handle(
            new UpdateEventCommand(
                eventId,
                " Updated launch ",
                newEventTime.ToString("O"),
                " Updated details ",
                expectedTokenText,
                currentUser),
            CancellationToken.None);

        Assert.Equal(UpdateEventStatus.Updated, result.Status);
        Assert.NotNull(result.Event);
        Assert.Equal(eventId, result.Event.Id);
        Assert.Equal("Updated launch", result.Event.EventName);
        Assert.Equal(newEventTime, result.Event.EventTime);
        Assert.Equal("event", result.Event.EventType);
        Assert.Equal("Updated details", result.Event.EventDescription);
        Assert.Equal(now, result.Event.CreatedAt);
        Assert.Equal(now.AddMinutes(10), result.Event.UpdatedAt);
        Assert.False(string.IsNullOrWhiteSpace(result.Event.ConcurrencyToken));
        Assert.NotEqual(expectedTokenText, result.Event.ConcurrencyToken);
        repository.Verify(
            repo => repo.GetByIdAsync(eventId, currentUser.TenantId, It.IsAny<CancellationToken>(), false),
            Times.Once);
        repository.Verify(
            repo => repo.UpdateAsync(
                plannedEvent,
                It.Is<byte[]>(token => token.SequenceEqual(expectedToken)),
                It.IsAny<CancellationToken>()),
            Times.Once);
        outbox.Verify(
            publisher => publisher.AddAsync(
                It.Is<IntegrationEventEnvelope>(message => message.EventType == "EventUpdated"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEventDoesNotExist_ReturnsNotFound()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var eventId = Guid.NewGuid();
        var currentUser = new CurrentUser(Guid.NewGuid(), Guid.NewGuid(), new[] { "events.update" });
        var repository = new Mock<IEventRepository>();
        repository
            .Setup(repo => repo.GetByIdAsync(eventId, currentUser.TenantId, It.IsAny<CancellationToken>(), false))
            .ReturnsAsync((PlannedEvent?)null);
        var timeProvider = new Mock<TimeProvider>();
        timeProvider.Setup(provider => provider.GetUtcNow()).Returns(now);
        var outbox = new Mock<IIntegrationEventOutbox>();
        var handler = new UpdateEventCommandHandler(
            repository.Object,
            outbox.Object,
            timeProvider.Object,
            NullLogger<UpdateEventCommandHandler>.Instance);

        var result = await handler.Handle(
            new UpdateEventCommand(
                eventId,
                "Updated launch",
                now.AddDays(1).ToString("O"),
                null,
                Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                currentUser),
            CancellationToken.None);

        Assert.Equal(UpdateEventStatus.NotFound, result.Status);
        repository.Verify(
            repo => repo.UpdateAsync(
                It.IsAny<PlannedEvent>(),
                It.IsAny<byte[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        outbox.Verify(
            publisher => publisher.AddAsync(It.IsAny<IntegrationEventEnvelope>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenConcurrencyTokenIsStale_ReturnsConflict()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var eventId = Guid.NewGuid();
        var currentUser = new CurrentUser(Guid.NewGuid(), Guid.NewGuid(), new[] { "events.update" });
        var plannedEvent = PlannedEvent.Create(
            eventId,
            currentUser.TenantId,
            "Launch plan",
            now.AddDays(1),
            EventType.Event,
            null,
            now);
        var expectedToken = plannedEvent.ConcurrencyToken;
        var repository = new Mock<IEventRepository>();
        repository
            .Setup(repo => repo.GetByIdAsync(eventId, currentUser.TenantId, It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(plannedEvent);
        repository
            .Setup(repo => repo.UpdateAsync(
                plannedEvent,
                It.Is<byte[]>(token => token.SequenceEqual(expectedToken)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var timeProvider = new Mock<TimeProvider>();
        timeProvider.Setup(provider => provider.GetUtcNow()).Returns(now.AddMinutes(10));
        var outbox = new Mock<IIntegrationEventOutbox>();
        var handler = new UpdateEventCommandHandler(
            repository.Object,
            outbox.Object,
            timeProvider.Object,
            NullLogger<UpdateEventCommandHandler>.Instance);

        var result = await handler.Handle(
            new UpdateEventCommand(
                eventId,
                "Updated launch",
                now.AddDays(2).ToString("O"),
                null,
                Convert.ToBase64String(expectedToken),
                currentUser),
            CancellationToken.None);

        Assert.Equal(UpdateEventStatus.Conflict, result.Status);
        outbox.Verify(
            publisher => publisher.AddAsync(It.IsAny<IntegrationEventEnvelope>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCallerLacksUpdatePermission_Throws()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var currentUser = new CurrentUser(Guid.NewGuid(), Guid.NewGuid(), Array.Empty<string>());
        var repository = new Mock<IEventRepository>();
        var timeProvider = new Mock<TimeProvider>();
        timeProvider.Setup(provider => provider.GetUtcNow()).Returns(now);
        var outbox = new Mock<IIntegrationEventOutbox>();
        var handler = new UpdateEventCommandHandler(
            repository.Object,
            outbox.Object,
            timeProvider.Object,
            NullLogger<UpdateEventCommandHandler>.Instance);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(
            new UpdateEventCommand(
                Guid.NewGuid(),
                "Updated launch",
                now.AddDays(1).ToString("O"),
                null,
                Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                currentUser),
            CancellationToken.None));

        repository.Verify(
            repo => repo.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<bool>()),
            Times.Never);
    }
}
