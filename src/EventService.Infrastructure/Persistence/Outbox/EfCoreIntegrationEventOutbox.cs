using EventService.Application.Abstractions.IntegrationEvents;
using SimplifyYours.Event.Abstractions;

namespace EventService.Infrastructure.Persistence.Outbox;

internal sealed class EfCoreIntegrationEventOutbox(EventServiceDbContext dbContext) : IIntegrationEventOutbox
{
    public async Task AddAsync(IntegrationEventEnvelope integrationEvent, CancellationToken cancellationToken)
    {
        await dbContext.OutboxMessages.AddAsync(OutboxMessage.From(integrationEvent), cancellationToken);
    }
}
