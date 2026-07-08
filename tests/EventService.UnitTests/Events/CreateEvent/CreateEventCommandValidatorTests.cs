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

    private CreateEventCommandValidator CreateValidator()
    {
        var timeProvider = new Mock<TimeProvider>();
        timeProvider.Setup(provider => provider.GetUtcNow()).Returns(_now);

        return new CreateEventCommandValidator(timeProvider.Object);
    }
}
