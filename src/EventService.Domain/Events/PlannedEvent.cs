namespace EventService.Domain.Events;

public sealed class PlannedEvent
{
    private PlannedEvent()
    {
    }

    private PlannedEvent(
        Guid id,
        Guid tenantId,
        string name,
        DateOnly eventDate,
        EventType type,
        string? description,
        DateTimeOffset createdAt,
        EventLocation? location,
        string? timeZoneId,
        TimeOnly? eventStartTime,
        TimeOnly? eventEndTime)
    {
        Id = id;
        TenantId = tenantId;
        Name = name;
        EventDate = eventDate;
        EventStartTime = eventStartTime;
        EventEndTime = eventEndTime;
        Type = type;
        Description = NormalizeOptionalText(description);
        Location = location;
        TimeZoneId = NormalizeOptionalText(timeZoneId);
        IsDeleted = false;
        DeletedAt = null;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        ConcurrencyToken = CreateConcurrencyToken();
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DateOnly EventDate { get; private set; }

    public TimeOnly? EventStartTime { get; private set; }

    public TimeOnly? EventEndTime { get; private set; }

    public EventType Type { get; private set; }

    public string? Description { get; private set; }

    public EventLocation? Location { get; private set; }

    public string? TimeZoneId { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public byte[] ConcurrencyToken { get; private set; } = [];

    public static PlannedEvent Create(
        Guid id,
        Guid tenantId,
        string name,
        DateOnly eventDate,
        EventType type,
        string? description,
        DateTimeOffset createdAt,
        EventLocation? location = null,
        string? timeZoneId = null,
        TimeOnly? eventStartTime = null,
        TimeOnly? eventEndTime = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Event id must not be empty.", nameof(id));
        }

        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id must not be empty.", nameof(tenantId));
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

        if (eventEndTime is not null && eventStartTime is not null && eventEndTime < eventStartTime)
        {
            throw new ArgumentException(
                "Event end time must be at or after the start time.",
                nameof(eventEndTime));
        }

        return new PlannedEvent(
            id,
            tenantId,
            normalizedName,
            eventDate,
            type,
            description,
            createdAt.ToUniversalTime(),
            location,
            timeZoneId,
            eventStartTime,
            eventEndTime);
    }

    public void UpdateDetails(
        string name,
        DateOnly eventDate,
        string? description,
        DateTimeOffset updatedAt)
    {
        UpdateDetails(name, eventDate, description, updatedAt, Location, TimeZoneId);
    }

    public void UpdateDetails(
        string name,
        DateOnly eventDate,
        string? description,
        DateTimeOffset updatedAt,
        EventLocation? location,
        string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Event name must contain at least 3 characters.", nameof(name));
        }

        var normalizedName = name.Trim();

        if (normalizedName.Length < 3)
        {
            throw new ArgumentException("Event name must contain at least 3 characters.", nameof(name));
        }

        Name = normalizedName;
        EventDate = eventDate;
        Description = NormalizeOptionalText(description);
        Location = location;
        TimeZoneId = NormalizeOptionalText(timeZoneId);
        UpdatedAt = updatedAt.ToUniversalTime();
        ConcurrencyToken = CreateConcurrencyToken();
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
        ConcurrencyToken = CreateConcurrencyToken();
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
        ConcurrencyToken = CreateConcurrencyToken();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static byte[] CreateConcurrencyToken()
    {
        return Guid.NewGuid().ToByteArray();
    }
}
