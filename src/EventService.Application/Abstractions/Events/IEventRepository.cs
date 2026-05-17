using EventService.Domain.Events;

namespace EventService.Application.Abstractions.Events;

public interface IEventRepository
{
    Task AddAsync(PlannedEvent plannedEvent, CancellationToken cancellationToken);

    Task<EventListPage> ListAsync(EventListQueryOptions options, CancellationToken cancellationToken);

    Task<PlannedEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken);
}
