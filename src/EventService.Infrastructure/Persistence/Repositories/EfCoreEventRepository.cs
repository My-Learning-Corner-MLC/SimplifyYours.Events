using EventService.Application.Abstractions.Events;
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

    public async Task<PlannedEvent?> GetByIdAsync(
        Guid eventId,
        CancellationToken cancellationToken,
        bool asNoTracking = true)
    {
        var query = dbContext.Events
            .Where(plannedEvent => !plannedEvent.IsDeleted);

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
            return false;
        }
    }

}
