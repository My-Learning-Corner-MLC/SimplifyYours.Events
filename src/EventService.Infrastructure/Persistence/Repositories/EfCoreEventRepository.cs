using EventService.Application.Abstractions.Events;
using EventService.Application.Events.GetEventList;
using EventService.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure.Persistence.Repositories;

internal sealed class EfCoreEventRepository(EventServiceDbContext dbContext) : IEventRepository
{
    public async Task AddAsync(PlannedEvent plannedEvent, CancellationToken cancellationToken)
    {
        await dbContext.Events.AddAsync(plannedEvent, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
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

        return new EventListPage(items, options.PageNumber, options.PageSize, totalCount);
    }

    public async Task<PlannedEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken)
    {
        return await dbContext.Events
            .AsNoTracking()
            .SingleOrDefaultAsync(
                plannedEvent => plannedEvent.Id == eventId && !plannedEvent.IsDeleted,
                cancellationToken);
    }
}
