using MediatR;

namespace EventService.Application.Events.GetEventList;

public sealed record GetEventListQuery(
    int? PageNumber,
    int? PageSize,
    string? Search,
    string? EventType,
    string? TimeFilter,
    string? SortBy,
    string? SortDirection) : IRequest<GetEventListResult>;
