using EventService.Application.Abstractions.Events;
using EventService.Domain.Events;

namespace EventService.Application.Events.GetEventList;

public static class EventListQueryBuilder
{
    public static IQueryable<PlannedEvent> ApplyFilters(
        IQueryable<PlannedEvent> query,
        EventListQueryOptions options)
    {
        query = query.Where(plannedEvent => !plannedEvent.IsDeleted);
        query = query.Where(plannedEvent => plannedEvent.TenantId == options.TenantId);
        query = ApplySearch(query, options.Search);
        query = ApplyEventTypeFilter(query, options.EventType);
        query = ApplyTimeFilter(query, options.TimeFilter, DateOnly.FromDateTime(options.CurrentUtcDateTime.UtcDateTime));

        return query;
    }

    public static IOrderedQueryable<PlannedEvent> ApplySorting(
        IQueryable<PlannedEvent> query,
        EventSortField sortBy,
        SortDirection sortDirection)
    {
        return (sortBy, sortDirection) switch
        {
            (EventSortField.UpdatedAt, SortDirection.Asc) => query
                .OrderBy(plannedEvent => plannedEvent.UpdatedAt)
                .ThenBy(plannedEvent => plannedEvent.Id),
            (EventSortField.UpdatedAt, SortDirection.Desc) => query
                .OrderByDescending(plannedEvent => plannedEvent.UpdatedAt)
                .ThenBy(plannedEvent => plannedEvent.Id),
            (EventSortField.CreatedAt, SortDirection.Asc) => query
                .OrderBy(plannedEvent => plannedEvent.CreatedAt)
                .ThenBy(plannedEvent => plannedEvent.Id),
            _ => query
                .OrderByDescending(plannedEvent => plannedEvent.CreatedAt)
                .ThenBy(plannedEvent => plannedEvent.Id)
        };
    }

    private static IQueryable<PlannedEvent> ApplySearch(IQueryable<PlannedEvent> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var normalizedSearch = search.Trim().ToLowerInvariant();

        return query.Where(plannedEvent =>
            plannedEvent.Name.ToLower().Contains(normalizedSearch)
            || (plannedEvent.Description != null
                && plannedEvent.Description.ToLower().Contains(normalizedSearch)));
    }

    private static IQueryable<PlannedEvent> ApplyEventTypeFilter(
        IQueryable<PlannedEvent> query,
        EventType? eventType)
    {
        return eventType.HasValue
            ? query.Where(plannedEvent => plannedEvent.Type == eventType.Value)
            : query;
    }

    private static IQueryable<PlannedEvent> ApplyTimeFilter(
        IQueryable<PlannedEvent> query,
        EventTimeFilter timeFilter,
        DateOnly today)
    {
        return timeFilter switch
        {
            EventTimeFilter.Upcoming => query.Where(plannedEvent => plannedEvent.EventDate >= today),
            EventTimeFilter.Past => query.Where(plannedEvent => plannedEvent.EventDate < today),
            _ => query
        };
    }
}
