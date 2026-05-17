using EventService.Application.Events.CreateEvent;
using EventService.Application.Events.GetEventDetails;
using EventService.Application.Events.UpdateEvent;
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
            .MapGet("{eventId:guid}", GetEventDetailsAsync)
            .WithName("GetEventDetails");

        group
            .MapPut("{id:guid}", UpdateEventAsync)
            .WithName("UpdateEvent");

        return endpoints;
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
                result.Event.Id,
                result.Event.EventName,
                result.Event.EventTime,
                result.Event.EventType,
                result.Event.EventDescription,
                result.Event.CreatedAt,
                result.Event.UpdatedAt,
                result.Event.ConcurrencyToken);

            return Results.Created($"/events/{response.Id}", response);
        }
        catch (ValidationException exception)
        {
            var errors = exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray());

            return Results.ValidationProblem(errors);
        }
    }

    private static async Task<IResult> UpdateEventAsync(
        Guid id,
        UpdateEventRequest request,
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
                UpdateEventStatus.NotFound => Results.NotFound(),
                UpdateEventStatus.Conflict => Results.Conflict(),
                _ => Results.Problem("Unexpected update event result.")
            };
        }
        catch (ValidationException exception)
        {
            var errors = exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray());

            return Results.ValidationProblem(errors);
        }
    }
}
