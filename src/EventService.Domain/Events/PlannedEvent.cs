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
        DateTimeOffset eventTime,
        EventType type,
        string? description,
        DateTimeOffset createdAt)
    {
        Id = id;
        TenantId = tenantId;
        Name = name;
        EventTime = eventTime;
        Type = type;
        Description = NormalizeOptionalText(description);
        IsDeleted = false;
        DeletedAt = null;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        ConcurrencyToken = CreateConcurrencyToken();
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DateTimeOffset EventTime { get; private set; }

    public EventType Type { get; private set; }

    public string? Description { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public byte[] ConcurrencyToken { get; private set; } = [];

    public static PlannedEvent Create(
        Guid id,
        Guid tenantId,
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

        return new PlannedEvent(
            id,
            tenantId,
            normalizedName,
            eventTime.ToUniversalTime(),
            type,
            description,
            createdAt.ToUniversalTime());
    }

    public void UpdateDetails(
        string name,
        DateTimeOffset eventTime,
        string? description,
        DateTimeOffset updatedAt)
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
        EventTime = eventTime.ToUniversalTime();
        Description = NormalizeOptionalText(description);
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
