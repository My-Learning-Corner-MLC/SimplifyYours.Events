using EventService.Application.Events.UpdateEvent;
using Moq;

namespace EventService.UnitTests.Events.UpdateEvent;

public sealed class UpdateEventCommandValidatorTests
{
    private readonly DateTimeOffset _now = new(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
    private readonly string _token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

    [Fact]
    public async Task Validate_WhenCommandIsValid_Passes()
    {
        var validator = CreateValidator();
        var command = new UpdateEventCommand(
            Guid.NewGuid(),
            "Birthday party",
            DateOnly.FromDateTime(_now.DateTime).AddDays(1).ToString("yyyy-MM-dd"),
            null,
            _token);

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenEventIdIsEmpty_Fails()
    {
        var validator = CreateValidator();
        var command = new UpdateEventCommand(
            Guid.Empty,
            "Birthday party",
            DateOnly.FromDateTime(_now.DateTime).AddDays(1).ToString("yyyy-MM-dd"),
            null,
            _token);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateEventCommand.EventId));
    }

    [Fact]
    public async Task Validate_WhenEventNameIsTooShort_Fails()
    {
        var validator = CreateValidator();
        var command = new UpdateEventCommand(
            Guid.NewGuid(),
            "ab",
            DateOnly.FromDateTime(_now.DateTime).AddDays(1).ToString("yyyy-MM-dd"),
            null,
            _token);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateEventCommand.EventName));
    }

    [Fact]
    public async Task Validate_WhenDescriptionIsTooLong_Fails()
    {
        var validator = CreateValidator();
        var command = new UpdateEventCommand(
            Guid.NewGuid(),
            "Launch plan",
            DateOnly.FromDateTime(_now.DateTime).AddDays(1).ToString("yyyy-MM-dd"),
            new string('a', 5001),
            _token);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateEventCommand.EventDescription));
    }

    [Fact]
    public async Task Validate_WhenEventDateIsInvalid_Fails()
    {
        var validator = CreateValidator();
        var command = new UpdateEventCommand(
            Guid.NewGuid(),
            "Launch plan",
            "not-a-date",
            null,
            _token);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateEventCommand.EventDate));
    }

    [Fact]
    public async Task Validate_WhenEventDateIsInThePast_Fails()
    {
        var validator = CreateValidator();
        var command = new UpdateEventCommand(
            Guid.NewGuid(),
            "Launch plan",
            DateOnly.FromDateTime(_now.DateTime).AddDays(-1).ToString("yyyy-MM-dd"),
            null,
            _token);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateEventCommand.EventDate));
    }

    [Fact]
    public async Task Validate_WhenEventDateIsToday_Passes()
    {
        var validator = CreateValidator();
        var command = new UpdateEventCommand(
            Guid.NewGuid(),
            "Launch plan",
            DateOnly.FromDateTime(_now.DateTime).ToString("yyyy-MM-dd"),
            null,
            _token);

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenConcurrencyTokenIsMalformed_Fails()
    {
        var validator = CreateValidator();
        var command = new UpdateEventCommand(
            Guid.NewGuid(),
            "Launch plan",
            DateOnly.FromDateTime(_now.DateTime).AddDays(1).ToString("yyyy-MM-dd"),
            null,
            "not-base64");

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateEventCommand.ConcurrencyToken));
    }

    [Fact]
    public async Task Validate_WhenLocationAndTimeZoneAreValid_Passes()
    {
        var validator = CreateValidator();
        var command = new UpdateEventCommand(
            Guid.NewGuid(),
            "Launch plan",
            DateOnly.FromDateTime(_now.DateTime).AddDays(1).ToString("yyyy-MM-dd"),
            null,
            _token,
            new UpdateEventLocation("The Riverside Room", "20 Riverside Drive", "Parking behind the building."),
            "Europe/Berlin");

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenLocationAndTimeZoneAreOmitted_Passes()
    {
        var validator = CreateValidator();
        var command = new UpdateEventCommand(
            Guid.NewGuid(),
            "Launch plan",
            DateOnly.FromDateTime(_now.DateTime).AddDays(1).ToString("yyyy-MM-dd"),
            null,
            _token);

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
        Assert.Null(command.Location);
        Assert.Null(command.TimeZoneId);
    }

    [Fact]
    public async Task Validate_WhenTimeZoneIsUnknown_Fails()
    {
        var validator = CreateValidator();
        var command = CreateCommandWithTimeZone("Not/AZone");

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateEventCommand.TimeZoneId)
                && error.ErrorMessage == "Time zone must be a valid IANA time zone id.");
    }

    [Fact]
    public async Task Validate_WhenTimeZoneIsBlank_Passes()
    {
        var validator = CreateValidator();
        var command = CreateCommandWithTimeZone("  ");

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenVenueNameIsTooLong_Fails()
    {
        var validator = CreateValidator();
        var command = CreateCommandWithLocation(new UpdateEventLocation(new string('a', 201), null, null));

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage == "Venue name must not exceed 200 characters.");
    }

    [Fact]
    public async Task Validate_WhenAddressIsTooLong_Fails()
    {
        var validator = CreateValidator();
        var command = CreateCommandWithLocation(new UpdateEventLocation(null, new string('a', 501), null));

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage == "Address must not exceed 500 characters.");
    }

    [Fact]
    public async Task Validate_WhenLocationNotesAreTooLong_Fails()
    {
        var validator = CreateValidator();
        var command = CreateCommandWithLocation(new UpdateEventLocation(null, null, new string('a', 2001)));

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage == "Location notes must not exceed 2000 characters.");
    }

    private UpdateEventCommand CreateCommandWithLocation(UpdateEventLocation location)
    {
        return new UpdateEventCommand(
            Guid.NewGuid(),
            "Launch plan",
            DateOnly.FromDateTime(_now.DateTime).AddDays(1).ToString("yyyy-MM-dd"),
            null,
            _token,
            location);
    }

    private UpdateEventCommand CreateCommandWithTimeZone(string timeZoneId)
    {
        return new UpdateEventCommand(
            Guid.NewGuid(),
            "Launch plan",
            DateOnly.FromDateTime(_now.DateTime).AddDays(1).ToString("yyyy-MM-dd"),
            null,
            _token,
            TimeZoneId: timeZoneId);
    }

    private UpdateEventCommandValidator CreateValidator()
    {
        var timeProvider = new Mock<TimeProvider>();
        timeProvider.Setup(provider => provider.GetUtcNow()).Returns(_now);

        return new UpdateEventCommandValidator(timeProvider.Object);
    }
}
