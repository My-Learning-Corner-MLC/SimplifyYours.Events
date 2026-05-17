using MediatR;

namespace EventService.Application.Events.GetEventDetails;

public sealed record GetEventDetailsQuery(Guid EventId) : IRequest<GetEventDetailsResult?>;
