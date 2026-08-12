using EventService.Application.Events;
using EventService.Domain.Events;
using FluentValidation;

namespace EventService.Application.Events.UpdateEvent;

public sealed class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(command => command.EventId)
            .NotEmpty();

        RuleFor(command => command.EventName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(name => name.Trim().Length >= 3)
            .WithMessage("Event name must contain at least 3 characters.")
            .MaximumLength(200);

        RuleFor(command => command.EventDescription)
            .MaximumLength(5000);

        RuleFor(command => command.EventDate)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must((command, eventDate) => EventParsing.TryParseEventDate(eventDate, out _))
            .WithMessage("Event date must be a valid date string.")
            .Must((command, eventDate) =>
                EventParsing.TryParseEventDate(eventDate, out var parsedEventDate)
                && parsedEventDate >= DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime))
            .WithMessage("Event date must be today or in the future.");

        RuleFor(command => command.ConcurrencyToken)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(TryDecodeConcurrencyToken)
            .WithMessage("Concurrency token must be a valid Base64 value.");

        RuleFor(command => command.TimeZoneId)
            .Must(timeZoneId => string.IsNullOrWhiteSpace(timeZoneId)
                || TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId.Trim(), out _))
            .WithMessage("Time zone must be a valid IANA time zone id.");

        RuleFor(command => command.EventStartTime)
            .Must(eventStartTime => eventStartTime is null
                || string.IsNullOrWhiteSpace(eventStartTime)
                || EventParsing.TryParseEventTime(eventStartTime, out _))
            .WithMessage("Event start time must be a valid time string.");

        RuleFor(command => command.EventEndTime)
            .Cascade(CascadeMode.Stop)
            .Must((command, eventEndTime) => eventEndTime is null
                || string.IsNullOrWhiteSpace(eventEndTime)
                || EventParsing.TryParseEventTime(eventEndTime, out _))
            .WithMessage("Event end time must be a valid time string.")
            .Must((command, eventEndTime) =>
            {
                if (string.IsNullOrWhiteSpace(eventEndTime)
                    || string.IsNullOrWhiteSpace(command.EventStartTime)
                    || !EventParsing.TryParseEventTime(eventEndTime, out var parsedEndTime)
                    || !EventParsing.TryParseEventTime(command.EventStartTime, out var parsedStartTime))
                {
                    return true;
                }

                return parsedEndTime >= parsedStartTime;
            })
            .WithMessage("Event end time must be at or after the start time.");

        When(command => command.Location is not null, () =>
        {
            RuleFor(command => command.Location!.VenueName)
                .MaximumLength(EventLocation.VenueNameMaxLength)
                .WithMessage($"Venue name must not exceed {EventLocation.VenueNameMaxLength} characters.");

            RuleFor(command => command.Location!.Address)
                .MaximumLength(EventLocation.AddressMaxLength)
                .WithMessage($"Address must not exceed {EventLocation.AddressMaxLength} characters.");

            RuleFor(command => command.Location!.Notes)
                .MaximumLength(EventLocation.NotesMaxLength)
                .WithMessage($"Location notes must not exceed {EventLocation.NotesMaxLength} characters.");
        });
    }

    private static bool TryDecodeConcurrencyToken(string value)
    {
        Span<byte> token = stackalloc byte[16];
        return Convert.TryFromBase64String(value, token, out var bytesWritten)
            && bytesWritten == token.Length;
    }
}
