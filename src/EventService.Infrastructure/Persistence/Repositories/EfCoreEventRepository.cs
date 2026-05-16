using EventService.Application.Abstractions.Events;
using EventService.Domain.Events;

namespace EventService.Infrastructure.Persistence.Repositories;

internal sealed class EfCoreEventRepository(EventServiceDbContext dbContext) : IEventRepository
{
    public async Task AddAsync(PlannedEvent plannedEvent, CancellationToken cancellationToken)
    {
        await dbContext.Events.AddAsync(plannedEvent, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
