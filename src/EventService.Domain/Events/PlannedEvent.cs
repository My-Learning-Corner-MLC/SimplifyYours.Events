namespace EventService.Domain.Events;

public sealed class PlannedEvent
{
    private PlannedEvent()
    {
    }

    private PlannedEvent(
        Guid id,
        string name,
        DateTimeOffset eventTime,
        EventType type,
        string? description,
        DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        EventTime = eventTime;
        Type = type;
        Description = NormalizeOptionalText(description);
        IsDeleted = false;
        DeletedAt = null;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DateTimeOffset EventTime { get; private set; }

    public EventType Type { get; private set; }

    public string? Description { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static PlannedEvent Create(
        Guid id,
        string name,
        DateTimeOffset eventTime,
        EventType type,
        string? description,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Event id must not be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Event name must contain at least 3 characters.", nameof(name));
        }

        var normalizedName = name.Trim();

        if (normalizedName.Length < 3)
        {
            throw new ArgumentException("Event name must contain at least 3 characters.", nameof(name));
        }

        return new PlannedEvent(id, normalizedName, eventTime.ToUniversalTime(), type, description, createdAt.ToUniversalTime());
    }

    public void SoftDelete(DateTimeOffset deletedAt)
    {
        if (IsDeleted)
        {
            return;
        }

        var utcDeletedAt = deletedAt.ToUniversalTime();
        IsDeleted = true;
        DeletedAt = utcDeletedAt;
        UpdatedAt = utcDeletedAt;
    }

    public void Restore(DateTimeOffset restoredAt)
    {
        if (!IsDeleted)
        {
            return;
        }

        IsDeleted = false;
        DeletedAt = null;
        UpdatedAt = restoredAt.ToUniversalTime();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
