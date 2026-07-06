namespace EventService.Domain.Events;

public sealed class EventLocation
{
    public const int VenueNameMaxLength = 200;
    public const int AddressMaxLength = 500;
    public const int OnlineUrlMaxLength = 2048;
    public const int NotesMaxLength = 2000;

    private EventLocation()
    {
    }

    private EventLocation(string? venueName, string? address, string? onlineUrl, string? notes)
    {
        VenueName = venueName;
        Address = address;
        OnlineUrl = onlineUrl;
        Notes = notes;
    }

    public string? VenueName { get; private set; }

    public string? Address { get; private set; }

    public string? OnlineUrl { get; private set; }

    public string? Notes { get; private set; }

    public static EventLocation? Create(string? venueName, string? address, string? onlineUrl, string? notes)
    {
        var normalizedVenueName = NormalizeOptionalText(venueName);
        var normalizedAddress = NormalizeOptionalText(address);
        var normalizedOnlineUrl = NormalizeOptionalText(onlineUrl);
        var normalizedNotes = NormalizeOptionalText(notes);

        if (normalizedVenueName is null
            && normalizedAddress is null
            && normalizedOnlineUrl is null
            && normalizedNotes is null)
        {
            return null;
        }

        if (normalizedVenueName?.Length > VenueNameMaxLength)
        {
            throw new ArgumentException(
                $"Venue name must not exceed {VenueNameMaxLength} characters.",
                nameof(venueName));
        }

        if (normalizedAddress?.Length > AddressMaxLength)
        {
            throw new ArgumentException(
                $"Address must not exceed {AddressMaxLength} characters.",
                nameof(address));
        }

        if (normalizedOnlineUrl is not null)
        {
            if (normalizedOnlineUrl.Length > OnlineUrlMaxLength)
            {
                throw new ArgumentException(
                    $"Online link must not exceed {OnlineUrlMaxLength} characters.",
                    nameof(onlineUrl));
            }

            if (!IsAbsoluteHttpUri(normalizedOnlineUrl))
            {
                throw new ArgumentException(
                    "Online link must be an absolute http or https URL.",
                    nameof(onlineUrl));
            }
        }

        if (normalizedNotes?.Length > NotesMaxLength)
        {
            throw new ArgumentException(
                $"Location notes must not exceed {NotesMaxLength} characters.",
                nameof(notes));
        }

        return new EventLocation(normalizedVenueName, normalizedAddress, normalizedOnlineUrl, normalizedNotes);
    }

    public static bool IsAbsoluteHttpUri(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
