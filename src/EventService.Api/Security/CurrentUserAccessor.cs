using EventService.Application.Authorization;

namespace EventService.Api.Security;

internal sealed class CurrentUserAccessor : ICurrentUserAccessor
{
    public CurrentUser? User { get; set; }
}
