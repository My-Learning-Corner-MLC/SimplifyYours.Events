using EventService.Domain.Events;

namespace EventService.UnitTests.Events;

public sealed class PlannedEventTests
{
    [Fact]
    public void Create_InitializesAuditAndSoftDeleteFields()
    {
        var eventId = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var eventTime = now.AddDays(1);

        var plannedEvent = PlannedEvent.Create(
            eventId,
            " Product launch ",
            eventTime,
            EventType.Event,
            " Launch details ",
            now);

        Assert.Equal(eventId, plannedEvent.Id);
        Assert.Equal("Product launch", plannedEvent.Name);
        Assert.Equal(eventTime, plannedEvent.EventTime);
        Assert.Equal(EventType.Event, plannedEvent.Type);
        Assert.Equal("Launch details", plannedEvent.Description);
        Assert.False(plannedEvent.IsDeleted);
        Assert.Null(plannedEvent.DeletedAt);
        Assert.Equal(now, plannedEvent.CreatedAt);
        Assert.Equal(now, plannedEvent.UpdatedAt);
        Assert.Equal(16, plannedEvent.ConcurrencyToken.Length);
    }

    [Fact]
    public void UpdateDetails_UpdatesEditableFieldsAndConcurrencyToken()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var plannedEvent = PlannedEvent.Create(
            Guid.NewGuid(),
            "Launch plan",
            now.AddDays(1),
            EventType.Event,
            "Old details",
            now);
        var originalToken = plannedEvent.ConcurrencyToken;
        var updatedAt = now.AddMinutes(5);
        var newEventTime = now.AddDays(2);

        plannedEvent.UpdateDetails(
            " Updated launch ",
            newEventTime,
            " Updated details ",
            updatedAt);

        Assert.Equal("Updated launch", plannedEvent.Name);
        Assert.Equal(newEventTime, plannedEvent.EventTime);
        Assert.Equal("Updated details", plannedEvent.Description);
        Assert.Equal(updatedAt, plannedEvent.UpdatedAt);
        Assert.Equal(EventType.Event, plannedEvent.Type);
        Assert.Equal(now, plannedEvent.CreatedAt);
        Assert.NotEqual(originalToken, plannedEvent.ConcurrencyToken);
        Assert.Equal(16, plannedEvent.ConcurrencyToken.Length);
    }

    [Fact]
    public void UpdateDetails_WhenDescriptionIsBlank_StoresNull()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var plannedEvent = PlannedEvent.Create(
            Guid.NewGuid(),
            "Launch plan",
            now.AddDays(1),
            EventType.Event,
            "Old details",
            now);

        plannedEvent.UpdateDetails("Launch plan", now.AddDays(2), "  ", now.AddMinutes(5));

        Assert.Null(plannedEvent.Description);
    }

    [Fact]
    public void UpdateDetails_WhenNameIsBlank_Throws()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var plannedEvent = PlannedEvent.Create(
            Guid.NewGuid(),
            "Launch plan",
            now.AddDays(1),
            EventType.Event,
            null,
            now);

        var exception = Assert.Throws<ArgumentException>(() => plannedEvent.UpdateDetails(
            "  ",
            now.AddDays(2),
            null,
            now.AddMinutes(5)));

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void SoftDeleteAndRestore_UpdateRestorableDeleteState()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var plannedEvent = PlannedEvent.Create(
            Guid.NewGuid(),
            "Wedding plan",
            now.AddDays(10),
            EventType.Wedding,
            null,
            now);
        var originalToken = plannedEvent.ConcurrencyToken;

        var deletedAt = now.AddMinutes(5);
        plannedEvent.SoftDelete(deletedAt);

        Assert.True(plannedEvent.IsDeleted);
        Assert.Equal(deletedAt, plannedEvent.DeletedAt);
        Assert.Equal(deletedAt, plannedEvent.UpdatedAt);
        Assert.NotEqual(originalToken, plannedEvent.ConcurrencyToken);
        var deletedToken = plannedEvent.ConcurrencyToken;

        var restoredAt = now.AddMinutes(10);
        plannedEvent.Restore(restoredAt);

        Assert.False(plannedEvent.IsDeleted);
        Assert.Null(plannedEvent.DeletedAt);
        Assert.Equal(restoredAt, plannedEvent.UpdatedAt);
        Assert.NotEqual(deletedToken, plannedEvent.ConcurrencyToken);
    }

    [Fact]
    public void Create_WhenIdIsEmpty_Throws()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);

        var exception = Assert.Throws<ArgumentException>(() => PlannedEvent.Create(
            Guid.Empty,
            "Launch plan",
            now.AddDays(1),
            EventType.Event,
            null,
            now));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Create_WhenNameIsBlank_Throws()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);

        var exception = Assert.Throws<ArgumentException>(() => PlannedEvent.Create(
            Guid.NewGuid(),
            "  ",
            now.AddDays(1),
            EventType.Event,
            null,
            now));

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Create_WhenDescriptionIsBlank_StoresNull()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);

        var plannedEvent = PlannedEvent.Create(
            Guid.NewGuid(),
            "Launch plan",
            now.AddDays(1),
            EventType.Event,
            "  ",
            now);

        Assert.Null(plannedEvent.Description);
    }

    [Fact]
    public void SoftDelete_WhenAlreadyDeleted_DoesNotChangeState()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var plannedEvent = PlannedEvent.Create(
            Guid.NewGuid(),
            "Launch plan",
            now.AddDays(1),
            EventType.Event,
            null,
            now);
        var deletedAt = now.AddMinutes(1);
        plannedEvent.SoftDelete(deletedAt);
        var deletedToken = plannedEvent.ConcurrencyToken;

        plannedEvent.SoftDelete(now.AddMinutes(2));

        Assert.True(plannedEvent.IsDeleted);
        Assert.Equal(deletedAt, plannedEvent.DeletedAt);
        Assert.Equal(deletedAt, plannedEvent.UpdatedAt);
        Assert.Equal(deletedToken, plannedEvent.ConcurrencyToken);
    }

    [Fact]
    public void Restore_WhenNotDeleted_DoesNotChangeState()
    {
        var now = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
        var plannedEvent = PlannedEvent.Create(
            Guid.NewGuid(),
            "Launch plan",
            now.AddDays(1),
            EventType.Event,
            null,
            now);
        var originalToken = plannedEvent.ConcurrencyToken;

        plannedEvent.Restore(now.AddMinutes(1));

        Assert.False(plannedEvent.IsDeleted);
        Assert.Null(plannedEvent.DeletedAt);
        Assert.Equal(now, plannedEvent.UpdatedAt);
        Assert.Equal(originalToken, plannedEvent.ConcurrencyToken);
    }
}
