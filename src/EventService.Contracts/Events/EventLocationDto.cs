namespace EventService.Contracts.Events;

public sealed record EventLocationDto(
    string? VenueName,
    string? Address,
    string? Notes);
