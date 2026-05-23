namespace EventService.Application.Events.GetEventList;

public sealed record GetEventListResult(
    IReadOnlyCollection<EventSummaryResult> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage);
