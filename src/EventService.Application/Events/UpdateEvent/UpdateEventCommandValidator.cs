using EventService.Application.Events;
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

        RuleFor(command => command.EventTime)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must((command, eventTime) => EventParsing.TryParseEventTime(eventTime, out _))
            .WithMessage("Event time must be a valid date-time string.")
            .Must((command, eventTime) =>
                EventParsing.TryParseEventTime(eventTime, out var parsedEventTime)
                && parsedEventTime >= timeProvider.GetUtcNow())
            .WithMessage("Event time must be now or in the future.");

        RuleFor(command => command.ConcurrencyToken)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(TryDecodeConcurrencyToken)
            .WithMessage("Concurrency token must be a valid Base64 value.");
    }

    private static bool TryDecodeConcurrencyToken(string value)
    {
        Span<byte> token = stackalloc byte[16];
        return Convert.TryFromBase64String(value, token, out var bytesWritten)
            && bytesWritten == token.Length;
    }
}
