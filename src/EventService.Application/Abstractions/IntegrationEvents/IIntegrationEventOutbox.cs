using SimplifyYours.Event.Abstractions;

namespace EventService.Application.Abstractions.IntegrationEvents;

public interface IIntegrationEventOutbox
{
    Task AddAsync(IntegrationEventEnvelope integrationEvent, CancellationToken cancellationToken);
}
