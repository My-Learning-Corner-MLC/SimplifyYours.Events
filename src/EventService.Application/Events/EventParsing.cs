using System.Globalization;
using EventService.Domain.Events;

namespace EventService.Application.Events;

public static class EventParsing
{
    public static bool TryParseEventDate(string? value, out DateOnly eventDate)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            eventDate = default;
            return false;
        }

        return DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out eventDate);
    }

    public static bool TryParseEventType(string? value, out EventType eventType)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            eventType = default;
            return false;
        }

        return Enum.TryParse(value, ignoreCase: true, out eventType)
            && Enum.IsDefined(eventType);
    }
}
