using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace EventService.Api.Security;

internal static class AuthorizationExtensions
{
    public static IServiceCollection AddPermissionPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(Permissions.EventsCreate, policy =>
                policy.RequireClaim(Permissions.ClaimType, Permissions.EventsCreate))
            .AddPolicy(Permissions.EventsView, policy =>
                policy.RequireClaim(Permissions.ClaimType, Permissions.EventsView))
            .AddPolicy(Permissions.EventsUpdate, policy =>
                policy.RequireClaim(Permissions.ClaimType, Permissions.EventsUpdate));

        services.AddSingleton<IAuthorizationMiddlewareResultHandler, PermissionDeniedResultHandler>();

        return services;
    }
}
