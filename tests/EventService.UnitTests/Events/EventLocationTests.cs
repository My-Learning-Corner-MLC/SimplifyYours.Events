using EventService.Domain.Events;

namespace EventService.UnitTests.Events;

public sealed class EventLocationTests
{
    [Fact]
    public void Create_TrimsAndStoresAllFields()
    {
        var location = EventLocation.Create(
            " The Backyard ",
            " 414 Maple Street, Brooklyn, NY 11215 ",
            " Park on Maple; side gate unlocked from 1:30. ");

        Assert.NotNull(location);
        Assert.Equal("The Backyard", location.VenueName);
        Assert.Equal("414 Maple Street, Brooklyn, NY 11215", location.Address);
        Assert.Equal("Park on Maple; side gate unlocked from 1:30.", location.Notes);
    }

    [Fact]
    public void Create_NormalizesBlankOptionalFieldsToNull()
    {
        var location = EventLocation.Create("The Backyard", "  ", string.Empty);

        Assert.NotNull(location);
        Assert.Equal("The Backyard", location.VenueName);
        Assert.Null(location.Address);
        Assert.Null(location.Notes);
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData("", "  ", "   ")]
    public void Create_WhenAllFieldsBlank_ReturnsNull(string? venueName, string? address, string? notes)
    {
        var location = EventLocation.Create(venueName, address, notes);

        Assert.Null(location);
    }

    [Fact]
    public void Create_WhenVenueNameTooLong_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => EventLocation.Create(
            new string('v', 201),
            null,
            null));

        Assert.Equal("venueName", exception.ParamName);
    }

    [Fact]
    public void Create_WhenAddressTooLong_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => EventLocation.Create(
            null,
            new string('a', 501),
            null));

        Assert.Equal("address", exception.ParamName);
    }

    [Fact]
    public void Create_WhenNotesTooLong_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => EventLocation.Create(
            null,
            null,
            new string('n', 2001)));

        Assert.Equal("notes", exception.ParamName);
    }
}
