namespace EventService.Contracts.Ping;

public sealed record PingStatusResponse(
    string Message,
    DateTimeOffset CurrentGmtDateTime);
