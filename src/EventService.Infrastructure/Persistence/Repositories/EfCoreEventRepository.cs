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

    public async Task<PlannedEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken)
    {
        return await dbContext.Events
            .AsNoTracking()
            .SingleOrDefaultAsync(
                plannedEvent => plannedEvent.Id == eventId && !plannedEvent.IsDeleted,
                cancellationToken);
    }
}
