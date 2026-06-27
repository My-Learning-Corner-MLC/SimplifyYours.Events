using System.Security.Claims;
using EventService.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EventService.UnitTests.Security;

public class AuthorizationPolicyRegistrationTests
{
    [Theory]
    [InlineData(Permissions.EventsCreate)]
    [InlineData(Permissions.EventsView)]
    [InlineData(Permissions.EventsUpdate)]
    public async Task AddPermissionPolicies_registers_policy_for_each_permission(string permission)
    {
        var provider = BuildPolicyProvider();

        var policy = await provider.GetPolicyAsync(permission);

        Assert.NotNull(policy);
        var claimRequirement = Assert.Single(policy!.Requirements.OfType<ClaimsAuthorizationRequirement>());
        Assert.Equal(Permissions.ClaimType, claimRequirement.ClaimType);
        Assert.NotNull(claimRequirement.AllowedValues);
        Assert.Contains(permission, claimRequirement.AllowedValues!);
    }

    [Fact]
    public async Task AddPermissionPolicies_does_not_register_unknown_permission()
    {
        var provider = BuildPolicyProvider();

        var policy = await provider.GetPolicyAsync("events.delete");

        Assert.Null(policy);
    }

    [Fact]
    public async Task Authorize_succeeds_when_principal_has_multiple_permission_claims_and_one_matches()
    {
        var services = BuildServices();
        var authorizationService = services.GetRequiredService<IAuthorizationService>();

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(Permissions.ClaimType, Permissions.EventsView),
                new Claim(Permissions.ClaimType, Permissions.EventsCreate)
            },
            authenticationType: "TestAuth"));

        var result = await authorizationService.AuthorizeAsync(principal, Permissions.EventsCreate);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Authorize_fails_when_principal_has_permission_claims_but_none_match()
    {
        var services = BuildServices();
        var authorizationService = services.GetRequiredService<IAuthorizationService>();

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(Permissions.ClaimType, Permissions.EventsView)
            },
            authenticationType: "TestAuth"));

        var result = await authorizationService.AuthorizeAsync(principal, Permissions.EventsCreate);

        Assert.False(result.Succeeded);
    }

    private static IAuthorizationPolicyProvider BuildPolicyProvider()
        => BuildServices().GetRequiredService<IAuthorizationPolicyProvider>();

    private static IServiceProvider BuildServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization();
        services.AddPermissionPolicies();
        return services.BuildServiceProvider();
    }
}
