namespace EventService.Application.Authorization;

public interface IAuthenticatedRequest
{
    CurrentUser CurrentUser { get; set; }
}
