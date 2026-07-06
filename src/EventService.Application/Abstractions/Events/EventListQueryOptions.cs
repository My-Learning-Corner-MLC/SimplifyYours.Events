using EventService.Domain.Events;

namespace EventService.Application.Abstractions.Events;

public sealed record EventListQueryOptions(
    Guid TenantId,
    int PageNumber,
    int PageSize,
    string? Search,
    EventType? EventType,
    EventTimeFilter TimeFilter,
    EventSortField SortBy,
    SortDirection SortDirection,
    DateTimeOffset CurrentUtcDateTime);
