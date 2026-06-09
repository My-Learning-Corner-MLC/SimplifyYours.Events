using EventService.Application.Abstractions.Common;

namespace EventService.Infrastructure.Persistence;

internal sealed class EfCoreUnitOfWork(EventServiceDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
