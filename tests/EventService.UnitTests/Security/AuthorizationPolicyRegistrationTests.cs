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
        var provider = BuildProvider();

        var policy = await provider.GetPolicyAsync(permission);

        Assert.NotNull(policy);
        var claimRequirement = Assert.Single(policy!.Requirements.OfType<ClaimsAuthorizationRequirement>());
        Assert.Equal(AuthorizationExtensions.PermissionClaimType, claimRequirement.ClaimType);
        Assert.NotNull(claimRequirement.AllowedValues);
        Assert.Contains(permission, claimRequirement.AllowedValues!);
    }

    [Fact]
    public async Task AddPermissionPolicies_does_not_register_unknown_permission()
    {
        var provider = BuildProvider();

        var policy = await provider.GetPolicyAsync("events.delete");

        Assert.Null(policy);
    }

    private static IAuthorizationPolicyProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPermissionPolicies();
        return services.BuildServiceProvider().GetRequiredService<IAuthorizationPolicyProvider>();
    }
}
