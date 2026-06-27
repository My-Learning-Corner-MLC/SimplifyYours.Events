using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace EventService.Api.Security;

internal static class AuthorizationExtensions
{
    public const string PermissionClaimType = "permissions";

    public static IServiceCollection AddPermissionPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(Permissions.EventsCreate, policy =>
                policy.RequireClaim(PermissionClaimType, Permissions.EventsCreate))
            .AddPolicy(Permissions.EventsView, policy =>
                policy.RequireClaim(PermissionClaimType, Permissions.EventsView))
            .AddPolicy(Permissions.EventsUpdate, policy =>
                policy.RequireClaim(PermissionClaimType, Permissions.EventsUpdate));

        services.AddSingleton<IAuthorizationMiddlewareResultHandler, PermissionDeniedResultHandler>();

        return services;
    }
}
