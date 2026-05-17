using EventService.Application.Events.CreateEvent;
using EventService.Application.Events.GetEventDetails;
using EventService.Application.Events.GetEventList;
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
            .WithName("CreateEvent");

        group
            .MapPost("/query", QueryEventsAsync)
            .WithName("QueryEvents");

        group
            .MapGet("{eventId:guid}", GetEventDetailsAsync)
            .WithName("GetEventDetails");

        return endpoints;
    }

    private static async Task<IResult> QueryEventsAsync(
        QueryEventsRequest request,
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
            return Results.ValidationProblem(ToValidationErrors(exception));
        }
    }

    private static async Task<IResult> GetEventDetailsAsync(
        Guid eventId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEventDetailsQuery(eventId), cancellationToken);

        if (result is null)
        {
            return Results.NotFound();
        }

        var response = new GetEventDetailsResponse(
            result.Id,
            result.EventName,
            result.EventTime,
            result.EventType,
            result.EventDescription,
            result.CreatedAt,
            result.UpdatedAt);

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateEventAsync(
        CreateEventRequest request,
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
                    request.EventDescription),
                cancellationToken);

            var response = new CreateEventResponse(
                result.Id,
                result.EventName,
                result.EventTime,
                result.EventType,
                result.EventDescription,
                result.CreatedAt,
                result.UpdatedAt);

            return Results.Created($"/events/{response.Id}", response);
        }
        catch (ValidationException exception)
        {
            return Results.ValidationProblem(ToValidationErrors(exception));
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
