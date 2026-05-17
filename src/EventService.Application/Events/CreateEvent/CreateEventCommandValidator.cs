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
            .Must((command, eventType) => CreateEventParsing.TryParseEventType(eventType, out _))
            .WithMessage("Event type must be one of: birthday, wedding, event.");

        RuleFor(command => command.EventTime)
            .Cascade(CascadeMode.Stop)
            .Must((command, eventTime) => string.IsNullOrWhiteSpace(eventTime)
                || CreateEventParsing.TryParseEventTime(eventTime, out _))
            .WithMessage("Event time must be a valid date-time string.")
            .Must((command, eventTime) =>
            {
                if (string.IsNullOrWhiteSpace(eventTime))
                {
                    return true;
                }

                return CreateEventParsing.TryParseEventTime(eventTime, out var parsedEventTime)
                    && parsedEventTime >= timeProvider.GetUtcNow();
            })
            .WithMessage("Event time must be now or in the future.");
    }
}
