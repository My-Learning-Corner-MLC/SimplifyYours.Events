using EventService.Application.Abstractions.Events;
using EventService.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventService.Application.Events.GetEventList;

public sealed class GetEventListQueryHandler(
    IEventRepository eventRepository,
    TimeProvider timeProvider,
    ILogger<GetEventListQueryHandler> logger)
    : IRequestHandler<GetEventListQuery, GetEventListResult>
{
    public async Task<GetEventListResult> Handle(GetEventListQuery request, CancellationToken cancellationToken)
    {
        var currentUser = request.CurrentUser;

        var options = new EventListQueryOptions(
            currentUser.TenantId,
            request.PageNumber ?? GetEventListQueryDefaults.PageNumber,
            request.PageSize ?? GetEventListQueryDefaults.PageSize,
            NormalizeOptionalText(request.Search),
            ResolveEventType(request.EventType),
            ResolveTimeFilter(request.TimeFilter),
            ResolveSortBy(request.SortBy),
            ResolveSortDirection(request.SortDirection),
            timeProvider.GetUtcNow());

        var page = await eventRepository.ListAsync(options, cancellationToken);
        var totalPages = page.TotalCount == 0
            ? 0
            : (int)Math.Ceiling(page.TotalCount / (double)page.PageSize);

        var items = page.Items
            .Select(plannedEvent => new EventSummaryResult(
                plannedEvent.Id,
                plannedEvent.Name,
                plannedEvent.EventDate,
                plannedEvent.Type.ToString().ToLowerInvariant(),
                plannedEvent.Description,
                plannedEvent.CreatedAt,
                plannedEvent.UpdatedAt,
                EventLocationDetails.From(plannedEvent.Location),
                plannedEvent.EventStartTime,
                plannedEvent.EventEndTime))
            .ToArray();

        var result = new GetEventListResult(
            items,
            page.PageNumber,
            page.PageSize,
            page.TotalCount,
            totalPages,
            page.PageNumber > 1,
            page.PageNumber < totalPages);

        logger.LogInformation(
            "Event list retrieved. PageNumber: {PageNumber}. PageSize: {PageSize}. ReturnedCount: {ReturnedCount}. TotalCount: {TotalCount}.",
            result.PageNumber,
            result.PageSize,
            result.Items.Count,
            result.TotalCount);

        return result;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static EventType? ResolveEventType(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : Enum.Parse<EventType>(value, ignoreCase: true);
    }

    private static EventTimeFilter ResolveTimeFilter(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? EventTimeFilter.All
            : Enum.Parse<EventTimeFilter>(value, ignoreCase: true);
    }

    private static EventSortField ResolveSortBy(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? EventSortField.CreatedAt
            : value.Trim().ToLowerInvariant() switch
            {
                "createdat" => EventSortField.CreatedAt,
                "updatedat" => EventSortField.UpdatedAt,
                _ => throw new ArgumentException("Sort field must be one of: createdAt, updatedAt.", nameof(value))
            };
    }

    private static SortDirection ResolveSortDirection(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? SortDirection.Desc
            : Enum.Parse<SortDirection>(value, ignoreCase: true);
    }
}
