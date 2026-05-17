using EventService.Domain.Events;

namespace EventService.Application.Abstractions.Events;

public interface IEventRepository
{
    Task AddAsync(PlannedEvent plannedEvent, CancellationToken cancellationToken);

    Task<PlannedEvent?> GetByIdAsync(
        Guid eventId,
        CancellationToken cancellationToken,
        bool asNoTracking = true);

    Task<bool> UpdateAsync(
        PlannedEvent plannedEvent,
        byte[] expectedConcurrencyToken,
        CancellationToken cancellationToken);
}
