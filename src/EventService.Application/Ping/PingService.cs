using EventService.Contracts.Ping;

namespace EventService.Application.Ping;

public sealed class PingService(TimeProvider timeProvider) : IPingService
{
    private const string ServiceUpMessage = "Event service is up.";

    public PingStatusResponse GetStatus()
    {
        var currentGmtDateTime = timeProvider.GetUtcNow();

        return new PingStatusResponse(ServiceUpMessage, currentGmtDateTime);
    }
}
