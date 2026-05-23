namespace EventService.Contracts.Events;

public sealed record QueryEventsRequest(
    int? PageNumber,
    int? PageSize,
    string? Search,
    string? EventType,
    string? TimeFilter,
    string? SortBy,
    string? SortDirection);
