using System.Globalization;
using EventService.Domain.Events;

namespace EventService.Application.Events.CreateEvent;

internal static class CreateEventParsing
{
    public static bool TryParseEventTime(string? value, out DateTimeOffset eventTime)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            eventTime = default;
            return false;
        }

        return DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out eventTime);
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
