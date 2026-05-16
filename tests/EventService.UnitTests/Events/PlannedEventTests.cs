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

        var deletedAt = now.AddMinutes(5);
        plannedEvent.SoftDelete(deletedAt);

        Assert.True(plannedEvent.IsDeleted);
        Assert.Equal(deletedAt, plannedEvent.DeletedAt);
        Assert.Equal(deletedAt, plannedEvent.UpdatedAt);

        var restoredAt = now.AddMinutes(10);
        plannedEvent.Restore(restoredAt);

        Assert.False(plannedEvent.IsDeleted);
        Assert.Null(plannedEvent.DeletedAt);
        Assert.Equal(restoredAt, plannedEvent.UpdatedAt);
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

        plannedEvent.SoftDelete(now.AddMinutes(2));

        Assert.True(plannedEvent.IsDeleted);
        Assert.Equal(deletedAt, plannedEvent.DeletedAt);
        Assert.Equal(deletedAt, plannedEvent.UpdatedAt);
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

        plannedEvent.Restore(now.AddMinutes(1));

        Assert.False(plannedEvent.IsDeleted);
        Assert.Null(plannedEvent.DeletedAt);
        Assert.Equal(now, plannedEvent.UpdatedAt);
    }
}
