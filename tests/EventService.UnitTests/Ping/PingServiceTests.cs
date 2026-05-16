using EventService.Application.Ping;

namespace EventService.UnitTests.Ping;

public sealed class PingServiceTests
{
    [Fact]
    public void GetStatus_ReturnsServiceUpMessageWithCurrentGmtDateTime()
    {
        var fixedDateTime = new DateTimeOffset(2026, 5, 15, 8, 30, 45, TimeSpan.Zero);
        var timeProvider = new FixedTimeProvider(fixedDateTime);
        var service = new PingService(timeProvider);

        var response = service.GetStatus();

        Assert.Equal("Event service is up.", response.Message);
        Assert.Equal(fixedDateTime, response.CurrentGmtDateTime);
        Assert.Equal(TimeSpan.Zero, response.CurrentGmtDateTime.Offset);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return utcNow;
        }
    }
}
