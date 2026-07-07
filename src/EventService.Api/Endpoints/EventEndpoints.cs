using EventService.Application.Events.CreateEvent;
using EventService.Application.Events.GetEventDetails;
using EventService.Application.Events.GetEventList;
using EventService.Application.Events.UpdateEvent;
using EventService.Api.Responses;
using EventService.Api.Security;
using EventService.Contracts.Events;
using FluentValidation;
using MediatR;

namespace EventService.Api.Endpoints;

internal static class EventEndpoints
{
    public static IEndpointRouteBuilder MapEventEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/events").WithTags("Events");

        group
            .MapPost("", CreateEventAsync)
            .WithName("CreateEvent")
            .RequireAuthorization(Permissions.EventsCreate);

        group
            .MapPost("/query", QueryEventsAsync)
            .WithName("QueryEvents")
            .RequireAuthorization(Permissions.EventsView);

        group
            .MapGet("{eventId:guid}", GetEventDetailsAsync)
            .WithName("GetEventDetails")
            .RequireAuthorization(Permissions.EventsView);

        group
            .MapPut("{id:guid}", UpdateEventAsync)
            .WithName("UpdateEvent")
            .RequireAuthorization(Permissions.EventsUpdate);

        return endpoints;
    }

    private static async Task<IResult> QueryEventsAsync(
        QueryEventsRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(
                new GetEventListQuery(
                    request.PageNumber,
                    request.PageSize,
                    request.Search,
                    request.EventType,
                    request.TimeFilter,
                    request.SortBy,
                    request.SortDirection),
                cancellationToken);

            var response = new QueryEventsResponse(
                result.Items
                    .Select(item => new EventSummaryResponse(
                        item.Id,
                        item.EventName,
                        item.EventTime,
                        item.EventType,
                        item.EventDescription,
                        item.CreatedAt,
                        item.UpdatedAt))
                    .ToArray(),
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                result.TotalPages,
                result.HasPreviousPage,
                result.HasNextPage);

            return Results.Ok(response);
        }
        catch (ValidationException exception)
        {
            return ApiErrorResults.ValidationProblem(ToValidationErrors(exception), httpContext);
        }
    }

    private static async Task<IResult> GetEventDetailsAsync(
        Guid eventId,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEventDetailsQuery(eventId), cancellationToken);

        if (result is null)
        {
            return ApiErrorResults.NotFound(
                "The event was not found. It may have been deleted or the id may be incorrect.",
                httpContext);
        }

        var response = new GetEventDetailsResponse(
            result.Event.Id,
            result.Event.EventName,
            result.Event.EventTime,
            result.Event.EventType,
            result.Event.EventDescription,
            result.Event.CreatedAt,
            result.Event.UpdatedAt,
            result.Event.ConcurrencyToken);

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateEventAsync(
        CreateEventRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(
                new CreateEventCommand(
                    request.EventName,
                    request.EventTime,
                    request.EventType,
                    request.EventDescription,
                    request.Location is null
                        ? null
                        : new CreateEventLocation(
                            request.Location.VenueName,
                            request.Location.Address,
                            request.Location.Notes),
                    request.TimeZoneId,
                    request.EventStartTime,
                    request.EventEndTime),
                cancellationToken);

            var response = new CreateEventResponse(
                result.Event.Id,
                result.Event.EventName,
                result.Event.EventTime,
                result.Event.EventType,
                result.Event.EventDescription,
                result.Event.CreatedAt,
                result.Event.UpdatedAt,
                result.Event.ConcurrencyToken,
                result.Event.Location is null
                    ? null
                    : new EventLocationDto(
                        result.Event.Location.VenueName,
                        result.Event.Location.Address,
                        result.Event.Location.Notes),
                result.Event.TimeZoneId,
                result.Event.EventStartTime,
                result.Event.EventEndTime);

            return Results.Created($"/events/{response.Id}", response);
        }
        catch (ValidationException exception)
        {
            return ApiErrorResults.ValidationProblem(ToValidationErrors(exception), httpContext);
        }
    }

    private static async Task<IResult> UpdateEventAsync(
        Guid id,
        UpdateEventRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(
                new UpdateEventCommand(
                    id,
                    request.EventName,
                    request.EventTime,
                    request.EventDescription,
                    request.ConcurrencyToken),
                cancellationToken);

            return result.Status switch
            {
                UpdateEventStatus.Updated when result.Event is not null => Results.Ok(new UpdateEventResponse(
                    result.Event.Id,
                    result.Event.EventName,
                    result.Event.EventTime,
                    result.Event.EventType,
                    result.Event.EventDescription,
                    result.Event.CreatedAt,
                    result.Event.UpdatedAt,
                    result.Event.ConcurrencyToken)),
                UpdateEventStatus.NotFound => ApiErrorResults.NotFound(
                    "The event was not found. It may have been deleted or the id may be incorrect.",
                    httpContext),
                UpdateEventStatus.Conflict => ApiErrorResults.Conflict(
                    "This event was changed by someone else. Please refresh the event and try again.",
                    httpContext),
                _ => ApiErrorResults.Unexpected(
                    "The event could not be updated right now. Please try again later.",
                    httpContext)
            };
        }
        catch (ValidationException exception)
        {
            return ApiErrorResults.ValidationProblem(ToValidationErrors(exception), httpContext);
        }
    }

    private static Dictionary<string, string[]> ToValidationErrors(ValidationException exception)
    {
        return exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());
    }
}
