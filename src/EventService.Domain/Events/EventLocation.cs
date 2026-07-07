namespace EventService.Domain.Events;

public sealed class EventLocation
{
    public const int VenueNameMaxLength = 200;
    public const int AddressMaxLength = 500;
    public const int NotesMaxLength = 2000;

    private EventLocation()
    {
    }

    private EventLocation(string? venueName, string? address, string? notes)
    {
        VenueName = venueName;
        Address = address;
        Notes = notes;
    }

    public string? VenueName { get; private set; }

    public string? Address { get; private set; }

    public string? Notes { get; private set; }

    public static EventLocation? Create(string? venueName, string? address, string? notes)
    {
        var normalizedVenueName = NormalizeOptionalText(venueName);
        var normalizedAddress = NormalizeOptionalText(address);
        var normalizedNotes = NormalizeOptionalText(notes);

        if (normalizedVenueName is null && normalizedAddress is null && normalizedNotes is null)
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

        if (normalizedNotes?.Length > NotesMaxLength)
        {
            throw new ArgumentException(
                $"Location notes must not exceed {NotesMaxLength} characters.",
                nameof(notes));
        }

        return new EventLocation(normalizedVenueName, normalizedAddress, normalizedNotes);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
