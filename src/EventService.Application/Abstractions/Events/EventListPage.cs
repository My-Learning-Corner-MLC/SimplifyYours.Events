using EventService.Domain.Events;

namespace EventService.Application.Abstractions.Events;

public sealed record EventListPage(
    IReadOnlyCollection<PlannedEvent> Items,
    int PageNumber,
    int PageSize,
    int TotalCount);
