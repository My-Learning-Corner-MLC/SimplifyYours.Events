using EventService.Application.Abstractions.Events;
using EventService.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventService.Application.Events.GetEventDetails;

public sealed class GetEventDetailsQueryHandler(
    IEventRepository eventRepository,
    ILogger<GetEventDetailsQueryHandler> logger)
    : IRequestHandler<GetEventDetailsQuery, GetEventDetailsResult?>
{
    public async Task<GetEventDetailsResult?> Handle(
        GetEventDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var currentUser = request.CurrentUser;

        var plannedEvent = await eventRepository.GetByIdAsync(
            request.EventId,
            currentUser.TenantId,
            cancellationToken);

        if (plannedEvent is null)
        {
            logger.LogWarning("Event details requested but event was not found. EventId: {EventId}.", request.EventId);
            return null;
        }

        logger.LogInformation("Event details retrieved. EventId: {EventId}.", request.EventId);

        return new GetEventDetailsResult(EventDetails.From(plannedEvent));
    }
}
