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
        var currentUser = new CurrentUser(Guid.NewGuid(), Guid.NewGuid());
        var plannedEvent = PlannedEvent.Create(
            eventId,
            currentUser.TenantId,
            "Launch plan",
            DateOnly.FromDateTime(now.DateTime).AddDays(1),
            EventType.Event,
            "Old details",
            now);
        var expectedToken = plannedEvent.ConcurrencyToken;
        var expectedTokenText = Convert.ToBase64String(expectedToken);
        var newEventDate = DateOnly.FromDateTime(now.DateTime).AddDays(2);
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
                newEventDate.ToString("yyyy-MM-dd"),
                " Updated details ",
                expectedTokenText)
            {
                CurrentUser = currentUser
            },
            CancellationToken.None);

        Assert.Equal(UpdateEventStatus.Updated, result.Status);
        Assert.NotNull(result.Event);
        Assert.Equal(eventId, result.Event.Id);
        Assert.Equal("Updated launch", result.Event.EventName);
        Assert.Equal(newEventDate, result.Event.EventDate);
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
    public async Task Handle_WhenLocationAndTimeZoneProvided_ReplacesThem()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var eventId = Guid.NewGuid();
        var currentUser = new CurrentUser(Guid.NewGuid(), Guid.NewGuid());
        var plannedEvent = PlannedEvent.Create(
            eventId,
            currentUser.TenantId,
            "Launch plan",
            DateOnly.FromDateTime(now.DateTime).AddDays(1),
            EventType.Event,
            null,
            now,
            EventLocation.Create("Old Hall", "1 Old Street", null),
            "America/Los_Angeles");
        var expectedToken = plannedEvent.ConcurrencyToken;
        var repository = CreateRepository(eventId, currentUser, plannedEvent, expectedToken, updateResult: true);
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
                "Launch plan",
                DateOnly.FromDateTime(now.DateTime).AddDays(2).ToString("yyyy-MM-dd"),
                null,
                Convert.ToBase64String(expectedToken),
                new UpdateEventLocation(
                    " The Riverside Room ",
                    " 20 Riverside Drive ",
                    " Parking behind the building. "),
                "Europe/Berlin")
            {
                CurrentUser = currentUser
            },
            CancellationToken.None);

        Assert.Equal(UpdateEventStatus.Updated, result.Status);
        Assert.NotNull(result.Event);
        Assert.NotNull(result.Event.Location);
        Assert.Equal("The Riverside Room", result.Event.Location.VenueName);
        Assert.Equal("20 Riverside Drive", result.Event.Location.Address);
        Assert.Equal("Parking behind the building.", result.Event.Location.Notes);
        Assert.Equal("Europe/Berlin", result.Event.TimeZoneId);
        outbox.Verify(
            publisher => publisher.AddAsync(
                It.Is<IntegrationEventEnvelope>(message => message.EventType == "EventUpdated"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenLocationAndTimeZoneOmitted_LeavesStoredValuesUntouched()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var eventId = Guid.NewGuid();
        var currentUser = new CurrentUser(Guid.NewGuid(), Guid.NewGuid());
        var plannedEvent = PlannedEvent.Create(
            eventId,
            currentUser.TenantId,
            "Launch plan",
            DateOnly.FromDateTime(now.DateTime).AddDays(1),
            EventType.Event,
            null,
            now,
            EventLocation.Create("The Riverside Room", "20 Riverside Drive", "Parking behind the building."),
            "America/Los_Angeles");
        var expectedToken = plannedEvent.ConcurrencyToken;
        var repository = CreateRepository(eventId, currentUser, plannedEvent, expectedToken, updateResult: true);
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
                "Launch plan renamed",
                DateOnly.FromDateTime(now.DateTime).AddDays(2).ToString("yyyy-MM-dd"),
                null,
                Convert.ToBase64String(expectedToken))
            {
                CurrentUser = currentUser
            },
            CancellationToken.None);

        Assert.Equal(UpdateEventStatus.Updated, result.Status);
        Assert.NotNull(result.Event);
        Assert.Equal("Launch plan renamed", result.Event.EventName);
        Assert.NotNull(result.Event.Location);
        Assert.Equal("The Riverside Room", result.Event.Location.VenueName);
        Assert.Equal("20 Riverside Drive", result.Event.Location.Address);
        Assert.Equal("Parking behind the building.", result.Event.Location.Notes);
        Assert.Equal("America/Los_Angeles", result.Event.TimeZoneId);
    }

    [Fact]
    public async Task Handle_WhenLocationFieldsAreAllBlank_ClearsLocationAndTimeZone()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var eventId = Guid.NewGuid();
        var currentUser = new CurrentUser(Guid.NewGuid(), Guid.NewGuid());
        var plannedEvent = PlannedEvent.Create(
            eventId,
            currentUser.TenantId,
            "Launch plan",
            DateOnly.FromDateTime(now.DateTime).AddDays(1),
            EventType.Event,
            null,
            now,
            EventLocation.Create("The Riverside Room", "20 Riverside Drive", null),
            "America/Los_Angeles");
        var expectedToken = plannedEvent.ConcurrencyToken;
        var repository = CreateRepository(eventId, currentUser, plannedEvent, expectedToken, updateResult: true);
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
                "Launch plan",
                DateOnly.FromDateTime(now.DateTime).AddDays(2).ToString("yyyy-MM-dd"),
                null,
                Convert.ToBase64String(expectedToken),
                new UpdateEventLocation(null, null, "   "),
                "  ")
            {
                CurrentUser = currentUser
            },
            CancellationToken.None);

        Assert.Equal(UpdateEventStatus.Updated, result.Status);
        Assert.NotNull(result.Event);
        Assert.Null(result.Event.Location);
        Assert.Null(result.Event.TimeZoneId);
    }

    [Fact]
    public async Task Handle_WhenEventDoesNotExist_ReturnsNotFound()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var eventId = Guid.NewGuid();
        var currentUser = new CurrentUser(Guid.NewGuid(), Guid.NewGuid());
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
                DateOnly.FromDateTime(now.DateTime).AddDays(1).ToString("yyyy-MM-dd"),
                null,
                Convert.ToBase64String(Guid.NewGuid().ToByteArray()))
            {
                CurrentUser = currentUser
            },
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
        var currentUser = new CurrentUser(Guid.NewGuid(), Guid.NewGuid());
        var plannedEvent = PlannedEvent.Create(
            eventId,
            currentUser.TenantId,
            "Launch plan",
            DateOnly.FromDateTime(now.DateTime).AddDays(1),
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
                DateOnly.FromDateTime(now.DateTime).AddDays(2).ToString("yyyy-MM-dd"),
                null,
                Convert.ToBase64String(expectedToken))
            {
                CurrentUser = currentUser
            },
            CancellationToken.None);

        Assert.Equal(UpdateEventStatus.Conflict, result.Status);
        outbox.Verify(
            publisher => publisher.AddAsync(It.IsAny<IntegrationEventEnvelope>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenLocationChangesAndTokenIsStale_ReturnsConflict()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var eventId = Guid.NewGuid();
        var currentUser = new CurrentUser(Guid.NewGuid(), Guid.NewGuid());
        var plannedEvent = PlannedEvent.Create(
            eventId,
            currentUser.TenantId,
            "Launch plan",
            DateOnly.FromDateTime(now.DateTime).AddDays(1),
            EventType.Event,
            null,
            now,
            EventLocation.Create("Old Hall", "1 Old Street", null),
            "America/Los_Angeles");
        var expectedToken = plannedEvent.ConcurrencyToken;
        var repository = CreateRepository(eventId, currentUser, plannedEvent, expectedToken, updateResult: false);
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
                "Launch plan",
                DateOnly.FromDateTime(now.DateTime).AddDays(2).ToString("yyyy-MM-dd"),
                null,
                Convert.ToBase64String(expectedToken),
                new UpdateEventLocation("The Riverside Room", "20 Riverside Drive", null),
                "Europe/Berlin")
            {
                CurrentUser = currentUser
            },
            CancellationToken.None);

        Assert.Equal(UpdateEventStatus.Conflict, result.Status);
        Assert.Null(result.Event);
    }

    private static Mock<IEventRepository> CreateRepository(
        Guid eventId,
        CurrentUser currentUser,
        PlannedEvent plannedEvent,
        byte[] expectedToken,
        bool updateResult)
    {
        var repository = new Mock<IEventRepository>();
        repository
            .Setup(repo => repo.GetByIdAsync(eventId, currentUser.TenantId, It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(plannedEvent);
        repository
            .Setup(repo => repo.UpdateAsync(
                plannedEvent,
                It.Is<byte[]>(token => token.SequenceEqual(expectedToken)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updateResult);

        return repository;
    }
}
