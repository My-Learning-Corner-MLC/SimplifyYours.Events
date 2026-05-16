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
            _now.AddHours(1).ToString("O"),
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
    public async Task Validate_WhenEventTimeIsInPast_Fails()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand(
            "Wedding plan",
            _now.AddSeconds(-1).ToString("O"),
            "wedding",
            null);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateEventCommand.EventTime));
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
    public async Task Validate_WhenEventTimeIsInvalid_Fails()
    {
        var validator = CreateValidator();
        var command = new CreateEventCommand("Launch plan", "not-a-date", "event", null);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateEventCommand.EventTime));
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
    public async Task Validate_WhenEventTimeIsOmitted_Passes()
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
