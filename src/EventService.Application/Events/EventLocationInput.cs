namespace EventService.Application.Events;

public sealed record EventLocationInput(
    string? VenueName,
    string? Address,
    string? Notes);
