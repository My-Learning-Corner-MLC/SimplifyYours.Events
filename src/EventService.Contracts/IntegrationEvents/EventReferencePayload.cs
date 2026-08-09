namespace EventService.Contracts.IntegrationEvents;

public sealed record EventReferencePayload(
    Guid EventId,
    string EventName,
    Guid TenantId,
    string EventType,
    DateOnly? EventDate = null,
    TimeOnly? EventStartTime = null,
    TimeOnly? EventEndTime = null,
    string? TimeZoneId = null,
    string? EventDescription = null,
    EventReferenceLocationPayload? Location = null);

public sealed record EventReferenceLocationPayload(
    string? VenueName,
    string? Address,
    string? Notes);
