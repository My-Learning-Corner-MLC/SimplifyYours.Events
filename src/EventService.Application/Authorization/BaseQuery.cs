namespace EventService.Application.Authorization;

public abstract record BaseQuery : IAuthenticatedRequest
{
    public CurrentUser CurrentUser { get; set; } = null!;
}
