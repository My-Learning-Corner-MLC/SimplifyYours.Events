using EventService.Application.Events.GetEventList;

namespace EventService.UnitTests.Events.GetEventList;

public sealed class GetEventListQueryValidatorTests
{
    [Fact]
    public async Task Validate_WhenQueryUsesDefaults_Passes()
    {
        var validator = new GetEventListQueryValidator();
        var query = new GetEventListQuery(null, null, null, null, null, null, null);

        var result = await validator.ValidateAsync(query);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_WhenPageNumberIsInvalid_Fails(int pageNumber)
    {
        var validator = new GetEventListQueryValidator();
        var query = new GetEventListQuery(pageNumber, null, null, null, null, null, null);

        var result = await validator.ValidateAsync(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetEventListQuery.PageNumber));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task Validate_WhenPageSizeIsInvalid_Fails(int pageSize)
    {
        var validator = new GetEventListQueryValidator();
        var query = new GetEventListQuery(null, pageSize, null, null, null, null, null);

        var result = await validator.ValidateAsync(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetEventListQuery.PageSize));
    }

    [Fact]
    public async Task Validate_WhenEventTypeIsInvalid_Fails()
    {
        var validator = new GetEventListQueryValidator();
        var query = new GetEventListQuery(null, null, null, "conference", null, null, null);

        var result = await validator.ValidateAsync(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetEventListQuery.EventType));
    }

    [Fact]
    public async Task Validate_WhenTimeFilterIsInvalid_Fails()
    {
        var validator = new GetEventListQueryValidator();
        var query = new GetEventListQuery(null, null, null, null, "future", null, null);

        var result = await validator.ValidateAsync(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetEventListQuery.TimeFilter));
    }

    [Fact]
    public async Task Validate_WhenSortByIsInvalid_Fails()
    {
        var validator = new GetEventListQueryValidator();
        var query = new GetEventListQuery(null, null, null, null, null, "eventTime", null);

        var result = await validator.ValidateAsync(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetEventListQuery.SortBy));
    }

    [Fact]
    public async Task Validate_WhenSortDirectionIsInvalid_Fails()
    {
        var validator = new GetEventListQueryValidator();
        var query = new GetEventListQuery(null, null, null, null, null, null, "latest");

        var result = await validator.ValidateAsync(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetEventListQuery.SortDirection));
    }

    [Fact]
    public async Task Validate_WhenValuesUseDifferentCasing_Passes()
    {
        var validator = new GetEventListQueryValidator();
        var query = new GetEventListQuery(1, 100, null, "Birthday", "Upcoming", "CreatedAt", "DESC");

        var result = await validator.ValidateAsync(query);

        Assert.True(result.IsValid);
    }
}
