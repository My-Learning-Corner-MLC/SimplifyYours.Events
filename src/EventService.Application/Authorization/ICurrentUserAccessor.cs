namespace EventService.Application.Authorization;

public interface ICurrentUserAccessor
{
    CurrentUser? User { get; }
}
