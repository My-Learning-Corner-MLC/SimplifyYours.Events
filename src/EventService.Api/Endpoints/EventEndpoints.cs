using EventService.Application.Events.CreateEvent;
using EventService.Application.Events.GetEventDetails;
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
            var errors = exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray());

            return Results.ValidationProblem(errors);
        }
    }
}
