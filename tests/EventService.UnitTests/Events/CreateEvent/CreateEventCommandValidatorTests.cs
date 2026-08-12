using EventService.Application.Events;
using EventService.Application.Events.CreateEvent;
using Moq;

namespace EventService.UnitTests.Events.CreateEvent;

public sealed class CreateEventCommandValidatorTests
{
    private readonly DateTimeOffset _now = new(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Validate_WhenCommandIsValid_Passes()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand(
            "Birthday party",
            DateOnly.FromDateTime(_now.DateTime).AddDays(1).ToString("yyyy-MM-dd"),
            "birthday",
            null);

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenEventNameIsTooShort_Fails()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand("ab", null, "event", null);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateEventCommand.EventName));
    }

    [Fact]
    public async Task Validate_WhenEventDateIsInThePast_Fails()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand(
            "Wedding plan",
            DateOnly.FromDateTime(_now.DateTime).AddDays(-1).ToString("yyyy-MM-dd"),
            "wedding",
            null);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateEventCommand.EventDate));
    }

    [Fact]
    public async Task Validate_WhenEventDateIsToday_Passes()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand(
            "Wedding plan",
            DateOnly.FromDateTime(_now.DateTime).ToString("yyyy-MM-dd"),
            "wedding",
            null);

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenEventTypeIsInvalid_Fails()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand("Launch plan", null, "conference", null);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateEventCommand.EventType));
    }

    [Fact]
    public async Task Validate_WhenEventDateIsInvalid_Fails()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand("Launch plan", "not-a-date", "event", null);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateEventCommand.EventDate));
    }

    [Fact]
    public async Task Validate_WhenDescriptionIsTooLong_Fails()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand("Launch plan", null, "event", new string('a', 5001));

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateEventCommand.EventDescription));
    }

    [Fact]
    public async Task Validate_WhenEventTypeUsesDifferentCasing_Passes()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand("Launch plan", null, "Wedding", null);

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenEventDateIsOmitted_Passes()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand("Launch plan", null, "event", null);

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithValidLocationAndTimeZone_Passes()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand(
            "Mateo turns five",
            null,
            "birthday",
            null,
            new EventLocationInput(
                "The Backyard",
                "414 Maple Street, Brooklyn, NY 11215",
                "Side gate unlocked from 1:30."),
            "America/Los_Angeles");

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("anniversary")]
    [InlineData("launch")]
    [InlineData("dinner")]
    [InlineData("other")]
    public async Task Validate_WithExpandedEventTypes_Passes(string eventType)
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand("Launch plan", null, eventType, null);

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenVenueNameIsTooLong_FailsOnLocationVenueName()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand(
            "Launch plan",
            null,
            "launch",
            null,
            new EventLocationInput(new string('v', 201), null, null));

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Location.VenueName");
    }

    [Fact]
    public async Task Validate_WhenAddressIsTooLong_FailsOnLocationAddress()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand(
            "Launch plan",
            null,
            "launch",
            null,
            new EventLocationInput(null, new string('a', 501), null));

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Location.Address");
    }

    [Fact]
    public async Task Validate_WhenNotesAreTooLong_FailsOnLocationNotes()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand(
            "Launch plan",
            null,
            "launch",
            null,
            new EventLocationInput(null, null, new string('n', 2001)));

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Location.Notes");
    }

    [Fact]
    public async Task Validate_WhenTimeZoneIdIsInvalid_FailsOnTimeZoneId()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand(
            "Launch plan",
            null,
            "launch",
            null,
            null,
            "Mars/Olympus_Mons");

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateEventCommand.TimeZoneId));
    }

    [Fact]
    public async Task Validate_WithValidStartAndEndTime_Passes()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand(
            "Dinner party",
            _now.ToString("yyyy-MM-dd"),
            "dinner",
            null,
            EventStartTime: "14:00",
            EventEndTime: "18:00");

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenStartTimeIsInvalid_FailsOnEventStartTime()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand(
            "Dinner party",
            _now.ToString("yyyy-MM-dd"),
            "dinner",
            null,
            EventStartTime: "not-a-time");

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateEventCommand.EventStartTime));
    }

    [Fact]
    public async Task Validate_WhenEndTimeIsInvalid_FailsOnEventEndTime()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand(
            "Dinner party",
            _now.ToString("yyyy-MM-dd"),
            "dinner",
            null,
            EventEndTime: "not-a-time");

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateEventCommand.EventEndTime));
    }

    [Fact]
    public async Task Validate_WhenEndTimeBeforeStartTime_FailsOnEventEndTime()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand(
            "Dinner party",
            _now.ToString("yyyy-MM-dd"),
            "dinner",
            null,
            EventStartTime: "18:00",
            EventEndTime: "14:00");

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateEventCommand.EventEndTime));
    }

    [Fact]
    public async Task Validate_WhenStartAndEndTimeOmitted_Passes()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand("Dinner party", _now.ToString("yyyy-MM-dd"), "dinner", null);

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenTimeZoneIdIsOmitted_Passes()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand("Launch plan", null, "launch", null);

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    private CreateEventCommandValidator CreateValidator()
    {
        var timeProvider = new Mock<TimeProvider>();
        timeProvider.Setup(provider => provider.GetUtcNow()).Returns(_now);

        return new CreateEventCommandValidator(timeProvider.Object);
    }
}
