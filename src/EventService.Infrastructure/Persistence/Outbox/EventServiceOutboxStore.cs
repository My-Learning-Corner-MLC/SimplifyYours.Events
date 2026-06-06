using Microsoft.EntityFrameworkCore;
using SimplifyYours.Event.Abstractions;

namespace EventService.Infrastructure.Persistence.Outbox;

internal sealed class EventServiceOutboxStore(EventServiceDbContext dbContext) : IEventOutboxStore
{
    public async Task<IReadOnlyList<EventOutboxRecord>> GetPendingAsync(
        int batchSize,
        int maxPublishAttempts,
        CancellationToken cancellationToken)
    {
        return await dbContext.OutboxMessages
            .AsNoTracking()
            .Where(message => message.ProcessedAt == null && message.RetryCount < maxPublishAttempts)
            .OrderBy(message => message.CreatedAt)
            .Take(batchSize)
            .Select(message => new EventOutboxRecord(
                message.Id,
                message.EventType,
                message.OccurredAt,
                message.CorrelationId,
                message.CausationId,
                message.Payload,
                message.Version,
                message.RetryCount, null))
            .ToListAsync(cancellationToken);
    }

    public async Task MarkPublishedAsync(
        Guid messageId,
        DateTimeOffset publishedAt,
        CancellationToken cancellationToken)
    {
        var message = await GetMessageAsync(messageId, cancellationToken);
        message.MarkProcessed(publishedAt);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkPublishFailedAsync(
        Guid messageId,
        string error,
        bool terminal,
        CancellationToken cancellationToken)
    {
        var message = await GetMessageAsync(messageId, cancellationToken);
        message.MarkFailed(error);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<OutboxMessage> GetMessageAsync(Guid messageId, CancellationToken cancellationToken)
    {
        return await dbContext.OutboxMessages.SingleAsync(
            message => message.Id == messageId,
            cancellationToken);
    }
}
