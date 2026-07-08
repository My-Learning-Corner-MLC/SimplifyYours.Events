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

    private UpdateEventCommandValidator CreateValidator()
    {
        var timeProvider = new Mock<TimeProvider>();
        timeProvider.Setup(provider => provider.GetUtcNow()).Returns(_now);

        return new UpdateEventCommandValidator(timeProvider.Object);
    }
}
