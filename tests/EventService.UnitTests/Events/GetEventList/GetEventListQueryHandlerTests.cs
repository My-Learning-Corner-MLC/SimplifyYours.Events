using EventService.Application.Abstractions.Events;
using EventService.Application.Authorization;
using EventService.Application.Events.GetEventList;
using EventService.Domain.Events;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EventService.UnitTests.Events.GetEventList;

public sealed class GetEventListQueryHandlerTests
{
    private static readonly CurrentUser TestUser = new(
        Guid.Parse("4927090e-0c70-45db-b0ba-080e121bf3f7"),
        Guid.Parse("9b4d8c79-7f4d-4d33-8efb-72f8a9b1d5e0"));
    private readonly DateTimeOffset _now = new(2026, 5, 17, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WhenOptionsAreOmitted_AppliesDefaults()
    {
        EventListQueryOptions? capturedOptions = null;
        var repository = new Mock<IEventRepository>();
        repository
            .Setup(repo => repo.ListAsync(It.IsAny<EventListQueryOptions>(), It.IsAny<CancellationToken>()))
            .Callback<EventListQueryOptions, CancellationToken>((options, _) => capturedOptions = options)
            .ReturnsAsync(new EventListPage(Array.Empty<PlannedEvent>(), 1, 20, 0));
        var handler = CreateHandler(repository.Object);

        var result = await handler.Handle(
            new GetEventListQuery(null, null, null, null, null, null, null) { CurrentUser = TestUser },
            CancellationToken.None);

        Assert.NotNull(capturedOptions);
        Assert.Equal(1, capturedOptions.PageNumber);
        Assert.Equal(20, capturedOptions.PageSize);
        Assert.Null(capturedOptions.Search);
        Assert.Equal(TestUser.TenantId, capturedOptions.TenantId);
        Assert.Null(capturedOptions.EventType);
        Assert.Equal(EventTimeFilter.All, capturedOptions.TimeFilter);
        Assert.Equal(EventSortField.CreatedAt, capturedOptions.SortBy);
        Assert.Equal(SortDirection.Desc, capturedOptions.SortDirection);
        Assert.Equal(_now, capturedOptions.CurrentUtcDateTime);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalPages);
        Assert.False(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public async Task Handle_WhenOptionsAreProvided_NormalizesAndPassesOptionsToRepository()
    {
        EventListQueryOptions? capturedOptions = null;
        var repository = new Mock<IEventRepository>();
        repository
            .Setup(repo => repo.ListAsync(It.IsAny<EventListQueryOptions>(), It.IsAny<CancellationToken>()))
            .Callback<EventListQueryOptions, CancellationToken>((options, _) => capturedOptions = options)
            .ReturnsAsync(new EventListPage(Array.Empty<PlannedEvent>(), 3, 15, 0));
        var handler = CreateHandler(repository.Object);

        await handler.Handle(
            new GetEventListQuery(3, 15, "  launch  ", "Wedding", "past", "updatedAt", "asc")
            {
                CurrentUser = TestUser
            },
            CancellationToken.None);

        Assert.NotNull(capturedOptions);
        Assert.Equal(3, capturedOptions.PageNumber);
        Assert.Equal(15, capturedOptions.PageSize);
        Assert.Equal("launch", capturedOptions.Search);
        Assert.Equal(EventType.Wedding, capturedOptions.EventType);
        Assert.Equal(EventTimeFilter.Past, capturedOptions.TimeFilter);
        Assert.Equal(EventSortField.UpdatedAt, capturedOptions.SortBy);
        Assert.Equal(SortDirection.Asc, capturedOptions.SortDirection);
    }

    [Fact]
    public async Task Handle_MapsRepositoryPageToResult()
    {
        var createdAt = _now.AddDays(-1);
        var plannedEvent = PlannedEvent.Create(
            Guid.Parse("30f4b3ce-b989-4d6b-9ea2-909d4118a45d"),
            Guid.NewGuid(),
            "Product launch",
            _now.AddDays(7),
            EventType.Event,
            "Launch details",
            createdAt);
        var repository = new Mock<IEventRepository>();
        repository
            .Setup(repo => repo.ListAsync(It.IsAny<EventListQueryOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventListPage(new[] { plannedEvent }, 2, 10, 25));
        var handler = CreateHandler(repository.Object);

        var result = await handler.Handle(
            new GetEventListQuery(2, 10, null, null, null, null, null) { CurrentUser = TestUser },
            CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal(plannedEvent.Id, item.Id);
        Assert.Equal("Product launch", item.EventName);
        Assert.Equal(plannedEvent.EventTime, item.EventTime);
        Assert.Equal("event", item.EventType);
        Assert.Equal("Launch details", item.EventDescription);
        Assert.Equal(createdAt, item.CreatedAt);
        Assert.Equal(createdAt, item.UpdatedAt);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        var repository = new Mock<IEventRepository>();
        repository
            .Setup(repo => repo.ListAsync(It.IsAny<EventListQueryOptions>(), cancellationTokenSource.Token))
            .ReturnsAsync(new EventListPage(Array.Empty<PlannedEvent>(), 1, 20, 0));
        var handler = CreateHandler(repository.Object);

        await handler.Handle(
            new GetEventListQuery(null, null, null, null, null, null, null) { CurrentUser = TestUser },
            cancellationTokenSource.Token);

        repository.Verify(
            repo => repo.ListAsync(It.IsAny<EventListQueryOptions>(), cancellationTokenSource.Token),
            Times.Once);
    }

    private TimeProvider CreateTimeProvider()
    {
        var timeProvider = new Mock<TimeProvider>();
        timeProvider.Setup(provider => provider.GetUtcNow()).Returns(_now);

        return timeProvider.Object;
    }

    private GetEventListQueryHandler CreateHandler(IEventRepository repository)
    {
        return new GetEventListQueryHandler(
            repository,
            CreateTimeProvider(),
            NullLogger<GetEventListQueryHandler>.Instance);
    }
}
