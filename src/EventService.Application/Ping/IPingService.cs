using EventService.Contracts.Ping;

namespace EventService.Application.Ping;

public interface IPingService
{
    PingStatusResponse GetStatus();
}
