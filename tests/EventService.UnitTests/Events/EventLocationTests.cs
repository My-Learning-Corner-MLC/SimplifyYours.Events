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
            " https://meet.example.com/mateo-turns-five ",
            " Park on Maple; side gate unlocked from 1:30. ");

        Assert.NotNull(location);
        Assert.Equal("The Backyard", location.VenueName);
        Assert.Equal("414 Maple Street, Brooklyn, NY 11215", location.Address);
        Assert.Equal("https://meet.example.com/mateo-turns-five", location.OnlineUrl);
        Assert.Equal("Park on Maple; side gate unlocked from 1:30.", location.Notes);
    }

    [Fact]
    public void Create_NormalizesBlankOptionalFieldsToNull()
    {
        var location = EventLocation.Create("The Backyard", "  ", null, string.Empty);

        Assert.NotNull(location);
        Assert.Equal("The Backyard", location.VenueName);
        Assert.Null(location.Address);
        Assert.Null(location.OnlineUrl);
        Assert.Null(location.Notes);
    }

    [Theory]
    [InlineData(null, null, null, null)]
    [InlineData("", "  ", "", "   ")]
    public void Create_WhenAllFieldsBlank_ReturnsNull(
        string? venueName,
        string? address,
        string? onlineUrl,
        string? notes)
    {
        var location = EventLocation.Create(venueName, address, onlineUrl, notes);

        Assert.Null(location);
    }

    [Fact]
    public void Create_WhenVenueNameTooLong_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => EventLocation.Create(
            new string('v', 201),
            null,
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
            null,
            null));

        Assert.Equal("address", exception.ParamName);
    }

    [Fact]
    public void Create_WhenOnlineUrlTooLong_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => EventLocation.Create(
            null,
            null,
            "https://example.com/" + new string('u', 2048),
            null));

        Assert.Equal("onlineUrl", exception.ParamName);
    }

    [Fact]
    public void Create_WhenNotesTooLong_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => EventLocation.Create(
            null,
            null,
            null,
            new string('n', 2001)));

        Assert.Equal("notes", exception.ParamName);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/file")]
    [InlineData("/relative/path")]
    [InlineData("javascript:alert(1)")]
    public void Create_WhenOnlineUrlIsNotAbsoluteHttpUri_Throws(string onlineUrl)
    {
        var exception = Assert.Throws<ArgumentException>(() => EventLocation.Create(
            null,
            null,
            onlineUrl,
            null));

        Assert.Equal("onlineUrl", exception.ParamName);
    }

    [Theory]
    [InlineData("http://example.com/party")]
    [InlineData("https://meet.example.com/room")]
    public void Create_AcceptsAbsoluteHttpAndHttpsUrls(string onlineUrl)
    {
        var location = EventLocation.Create(null, null, onlineUrl, null);

        Assert.NotNull(location);
        Assert.Equal(onlineUrl, location.OnlineUrl);
    }
}
