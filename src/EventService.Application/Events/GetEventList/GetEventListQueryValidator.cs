using EventService.Domain.Events;
using FluentValidation;

namespace EventService.Application.Events.GetEventList;

public sealed class GetEventListQueryValidator : AbstractValidator<GetEventListQuery>
{
    private static readonly HashSet<string> TimeFilters = new(StringComparer.OrdinalIgnoreCase)
    {
        "all",
        "upcoming",
        "past"
    };

    private static readonly HashSet<string> SortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "createdAt",
        "updatedAt"
    };

    private static readonly HashSet<string> SortDirections = new(StringComparer.OrdinalIgnoreCase)
    {
        "asc",
        "desc"
    };

    public GetEventListQueryValidator()
    {
        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1)
            .When(query => query.PageNumber.HasValue);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, GetEventListQueryDefaults.MaxPageSize)
            .When(query => query.PageSize.HasValue);

        RuleFor(query => query.EventType)
            .Must(BeKnownEventType)
            .WithMessage("Event type must be one of: birthday, wedding, event.");

        RuleFor(query => query.TimeFilter)
            .Must(value => BeEmptyOrOneOf(value, TimeFilters))
            .WithMessage("Time filter must be one of: all, upcoming, past.");

        RuleFor(query => query.SortBy)
            .Must(value => BeEmptyOrOneOf(value, SortFields))
            .WithMessage("Sort field must be one of: createdAt, updatedAt.");

        RuleFor(query => query.SortDirection)
            .Must(value => BeEmptyOrOneOf(value, SortDirections))
            .WithMessage("Sort direction must be one of: asc, desc.");
    }

    private static bool BeKnownEventType(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            || Enum.TryParse(value, ignoreCase: true, out EventType eventType)
            && Enum.IsDefined(eventType);
    }

    private static bool BeEmptyOrOneOf(string? value, HashSet<string> acceptedValues)
    {
        return string.IsNullOrWhiteSpace(value) || acceptedValues.Contains(value.Trim());
    }
}
