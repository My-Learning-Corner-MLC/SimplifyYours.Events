using EventService.Application.Ping;

namespace EventService.Api.Endpoints;

internal static class PingEndpoints
{
    public static IEndpointRouteBuilder MapPingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/ping", (IPingService pingService, ILoggerFactory loggerFactory) =>
            {
                var response = pingService.GetStatus();

                loggerFactory
                    .CreateLogger("EventService.Ping")
                    .LogInformation(
                        "Event service is up. Current GMT datetime: {CurrentGmtDateTime}",
                        response.CurrentGmtDateTime);

                return Results.Ok(response);
            })
            .WithName("Ping");

        return endpoints;
    }
}
