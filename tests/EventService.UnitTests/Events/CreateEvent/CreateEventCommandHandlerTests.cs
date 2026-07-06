using EventService.Application.Abstractions.Common;
using EventService.Application.Abstractions.Events;
using EventService.Application.Abstractions.IntegrationEvents;
using EventService.Application.Authorization;
using EventService.Application.Events.CreateEvent;
using EventService.Domain.Events;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SimplifyYours.Event.Abstractions;

namespace EventService.UnitTests.Events.CreateEvent;

public sealed class CreateEventCommandHandlerTests
{
    private static readonly CurrentUser TestUser = new(
        Guid.Parse("1ed66a76-8c3e-4cef-b53f-3b6acb318b45"),
        Guid.Parse("2c1e22fb-7c11-44a4-9fc8-3e2c4f9d8a01"));

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
            new CreateEventCommand("Wedding plan", eventTime.ToString("O"), "wedding", "Details")
            {
                CurrentUser = TestUser
            },
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
        Assert.Equal(TestUser.TenantId, savedEvent.TenantId);
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
    public async Task Handle_WithLocationAndTimeZone_CreatesEventWithBoth()
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
            new CreateEventCommand(
                "Mateo turns five",
                null,
                "birthday",
                null,
                new CreateEventLocation(
                    " The Backyard ",
                    "414 Maple Street, Brooklyn, NY 11215",
                    "https://meet.example.com/party",
                    "Side gate unlocked from 1:30."),
                "America/Los_Angeles")
            {
                CurrentUser = TestUser
            },
            CancellationToken.None);

        Assert.NotNull(result.Event.Location);
        Assert.Equal("The Backyard", result.Event.Location.VenueName);
        Assert.Equal("414 Maple Street, Brooklyn, NY 11215", result.Event.Location.Address);
        Assert.Equal("https://meet.example.com/party", result.Event.Location.OnlineUrl);
        Assert.Equal("Side gate unlocked from 1:30.", result.Event.Location.Notes);
        Assert.Equal("America/Los_Angeles", result.Event.TimeZoneId);
        Assert.NotNull(savedEvent);
        Assert.NotNull(savedEvent.Location);
        Assert.Equal("The Backyard", savedEvent.Location.VenueName);
        Assert.Equal("America/Los_Angeles", savedEvent.TimeZoneId);
        repository.Verify(
            repo => repo.AddAsync(It.IsAny<PlannedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
        outbox.Verify(
            publisher => publisher.AddAsync(
                It.Is<IntegrationEventEnvelope>(message => message.EventType == "EventCreated"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutLocationAndTimeZone_CreatesEventWithoutThem()
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
            new CreateEventCommand("Anniversary dinner", null, "anniversary", null)
            {
                CurrentUser = TestUser
            },
            CancellationToken.None);

        Assert.Null(result.Event.Location);
        Assert.Null(result.Event.TimeZoneId);
        Assert.Equal("anniversary", result.Event.EventType);
        Assert.Null(savedEvent?.Location);
        Assert.Null(savedEvent?.TimeZoneId);
        unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenLocationIsAllBlank_CreatesEventWithNullLocation()
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

        var result = await handler.Handle(
            new CreateEventCommand(
                "Dinner party",
                null,
                "dinner",
                null,
                new CreateEventLocation("  ", null, string.Empty, "   "),
                null)
            {
                CurrentUser = TestUser
            },
            CancellationToken.None);

        Assert.Null(result.Event.Location);
    }

    [Fact]
    public async Task Handle_WithEndTime_StoresEndTime()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var start = now.AddDays(1);
        var end = start.AddHours(4);
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
            new CreateEventCommand(
                "Dinner party",
                start.ToString("O"),
                "dinner",
                null,
                null,
                null,
                end.ToString("O"))
            {
                CurrentUser = TestUser
            },
            CancellationToken.None);

        Assert.Equal(end, result.Event.EventEndTime);
        Assert.Equal(end, savedEvent?.EventEndTime);
        unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
            new CreateEventCommand("Birthday plan", null, "birthday", null)
            {
                CurrentUser = TestUser
            },
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
            new CreateEventCommand("Birthday plan", "not-a-date", "birthday", null)
            {
                CurrentUser = TestUser
            },
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
            new CreateEventCommand("Birthday plan", null, "conference", null)
            {
                CurrentUser = TestUser
            },
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
