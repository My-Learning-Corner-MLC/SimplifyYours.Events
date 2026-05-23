namespace EventService.Contracts.Events;

public sealed record QueryEventsResponse(
    IReadOnlyCollection<EventSummaryResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage);
