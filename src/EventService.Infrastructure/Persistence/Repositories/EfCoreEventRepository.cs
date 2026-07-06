using EventService.Application.Abstractions.Events;
using EventService.Application.Events.GetEventList;
using EventService.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventService.Infrastructure.Persistence.Repositories;

internal sealed class EfCoreEventRepository(
    EventServiceDbContext dbContext,
    ILogger<EfCoreEventRepository> logger) : IEventRepository
{
    public async Task AddAsync(PlannedEvent plannedEvent, CancellationToken cancellationToken)
    {
        await dbContext.Events.AddAsync(plannedEvent, cancellationToken);
        logger.LogInformation("Event persisted. EventId: {EventId}.", plannedEvent.Id);
    }

    public async Task<EventListPage> ListAsync(EventListQueryOptions options, CancellationToken cancellationToken)
    {
        var query = EventListQueryBuilder.ApplyFilters(
            dbContext.Events.AsNoTracking(),
            options);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await EventListQueryBuilder.ApplySorting(query, options.SortBy, options.SortDirection)
            .Skip((options.PageNumber - 1) * options.PageSize)
            .Take(options.PageSize)
            .ToListAsync(cancellationToken);

        logger.LogInformation(
            "Event query executed. PageNumber: {PageNumber}. PageSize: {PageSize}. ReturnedCount: {ReturnedCount}. TotalCount: {TotalCount}.",
            options.PageNumber,
            options.PageSize,
            items.Count,
            totalCount);

        return new EventListPage(items, options.PageNumber, options.PageSize, totalCount);
    }

    public async Task<PlannedEvent?> GetByIdAsync(
        Guid eventId,
        Guid tenantId,
        CancellationToken cancellationToken,
        bool asNoTracking = true)
    {
        var query = dbContext.Events
            .Where(plannedEvent => !plannedEvent.IsDeleted)
            .Where(plannedEvent => plannedEvent.TenantId == tenantId);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query
            .SingleOrDefaultAsync(plannedEvent => plannedEvent.Id == eventId, cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        PlannedEvent plannedEvent,
        byte[] expectedConcurrencyToken,
        CancellationToken cancellationToken)
    {
        dbContext.Entry(plannedEvent)
            .Property(currentEvent => currentEvent.ConcurrencyToken)
            .OriginalValue = expectedConcurrencyToken;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogWarning("Event persistence concurrency conflict. EventId: {EventId}.", plannedEvent.Id);
            return false;
        }
    }

}
