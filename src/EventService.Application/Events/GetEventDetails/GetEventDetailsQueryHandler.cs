using EventService.Application.Abstractions.Events;
using MediatR;

namespace EventService.Application.Events.GetEventDetails;

public sealed class GetEventDetailsQueryHandler(IEventRepository eventRepository)
    : IRequestHandler<GetEventDetailsQuery, GetEventDetailsResult?>
{
    public async Task<GetEventDetailsResult?> Handle(
        GetEventDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var plannedEvent = await eventRepository.GetByIdAsync(request.EventId, cancellationToken);

        if (plannedEvent is null)
        {
            return null;
        }

        return new GetEventDetailsResult(
            plannedEvent.Id,
            plannedEvent.Name,
            plannedEvent.EventTime,
            plannedEvent.Type.ToString().ToLowerInvariant(),
            plannedEvent.Description,
            plannedEvent.CreatedAt,
            plannedEvent.UpdatedAt);
    }
}
