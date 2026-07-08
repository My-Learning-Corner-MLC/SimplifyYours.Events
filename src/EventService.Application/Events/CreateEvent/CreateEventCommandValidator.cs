using EventService.Application.Events;
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
            .WithMessage("Event type must be one of: birthday, wedding, event.");

        RuleFor(command => command.EventDate)
            .Cascade(CascadeMode.Stop)
            .Must((command, eventDate) => string.IsNullOrWhiteSpace(eventDate)
                || EventParsing.TryParseEventDate(eventDate, out _))
            .WithMessage("Event date must be a valid date string.")
            .Must((command, eventDate) =>
            {
                if (string.IsNullOrWhiteSpace(eventDate))
                {
                    return true;
                }

                return EventParsing.TryParseEventDate(eventDate, out var parsedEventDate)
                    && parsedEventDate >= DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
            })
            .WithMessage("Event date must be today or in the future.");
    }
}
