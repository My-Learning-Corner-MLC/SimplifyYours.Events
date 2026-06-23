using EventService.Application.Authorization;
using MediatR;

namespace EventService.Application.Events.GetEventList;

public sealed record GetEventListQuery(
    int? PageNumber,
    int? PageSize,
    string? Search,
    string? EventType,
    string? TimeFilter,
    string? SortBy,
    string? SortDirection,
    CurrentUser? CurrentUser = null) : IRequest<GetEventListResult>;
