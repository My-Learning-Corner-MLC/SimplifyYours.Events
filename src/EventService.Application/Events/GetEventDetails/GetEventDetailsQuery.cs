using EventService.Application.Authorization;
using MediatR;

namespace EventService.Application.Events.GetEventDetails;

public sealed record GetEventDetailsQuery(Guid EventId) : BaseQuery, IRequest<GetEventDetailsResult?>;
