using EventService.Application.Events;
using EventService.Domain.Events;
using FluentValidation;

namespace EventService.Application.Events.CreateEvent;

public sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(command => command.EventName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(name => name.Trim().Length >= 3)
            .WithMessage("Event name must contain at least 3 characters.")
            .MaximumLength(200);

        RuleFor(command => command.EventDescription)
            .MaximumLength(5000);

        RuleFor(command => command.EventType)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must((command, eventType) => EventParsing.TryParseEventType(eventType, out _))
            .WithMessage("Event type must be one of: birthday, wedding, event, anniversary, launch, dinner, other.");

        RuleFor(command => command.EventTime)
            .Cascade(CascadeMode.Stop)
            .Must((command, eventTime) => string.IsNullOrWhiteSpace(eventTime)
                || EventParsing.TryParseEventTime(eventTime, out _))
            .WithMessage("Event time must be a valid date-time string.")
            .Must((command, eventTime) =>
            {
                if (string.IsNullOrWhiteSpace(eventTime))
                {
                    return true;
                }

                return EventParsing.TryParseEventTime(eventTime, out var parsedEventTime)
                    && parsedEventTime >= timeProvider.GetUtcNow();
            })
            .WithMessage("Event time must be now or in the future.");

        RuleFor(command => command.TimeZoneId)
            .Must(timeZoneId => string.IsNullOrWhiteSpace(timeZoneId)
                || TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId.Trim(), out _))
            .WithMessage("Time zone must be a valid IANA time zone id.");

        When(command => command.Location is not null, () =>
        {
            RuleFor(command => command.Location!.VenueName)
                .MaximumLength(EventLocation.VenueNameMaxLength)
                .WithMessage($"Venue name must not exceed {EventLocation.VenueNameMaxLength} characters.");

            RuleFor(command => command.Location!.Address)
                .MaximumLength(EventLocation.AddressMaxLength)
                .WithMessage($"Address must not exceed {EventLocation.AddressMaxLength} characters.");

            RuleFor(command => command.Location!.OnlineUrl)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(EventLocation.OnlineUrlMaxLength)
                .WithMessage($"Online link must not exceed {EventLocation.OnlineUrlMaxLength} characters.")
                .Must(onlineUrl => string.IsNullOrWhiteSpace(onlineUrl)
                    || EventLocation.IsAbsoluteHttpUri(onlineUrl.Trim()))
                .WithMessage("Online link must be an absolute http or https URL.");

            RuleFor(command => command.Location!.Notes)
                .MaximumLength(EventLocation.NotesMaxLength)
                .WithMessage($"Location notes must not exceed {EventLocation.NotesMaxLength} characters.");
        });
    }
}
