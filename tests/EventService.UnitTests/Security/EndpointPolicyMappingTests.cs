using EventService.Api.Endpoints;
using EventService.Api.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EventService.UnitTests.Security;

public class EndpointPolicyMappingTests
{
    [Theory]
    [InlineData("CreateEvent", Permissions.EventsCreate)]
    [InlineData("QueryEvents", Permissions.EventsView)]
    [InlineData("GetEventDetails", Permissions.EventsView)]
    [InlineData("UpdateEvent", Permissions.EventsUpdate)]
    public void Event_endpoint_requires_expected_permission_policy(string endpointName, string expectedPolicy)
    {
        var endpoints = MapEventEndpointsForTest();

        var endpoint = endpoints.SingleOrDefault(e =>
            string.Equals(e.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName, endpointName, StringComparison.Ordinal));

        Assert.NotNull(endpoint);

        var policies = endpoint!.Metadata
            .GetOrderedMetadata<IAuthorizeData>()
            .Select(data => data.Policy)
            .Where(policy => policy is not null)
            .ToArray();

        Assert.Contains(expectedPolicy, policies);
    }

    [Fact]
    public void Event_endpoints_cover_exactly_the_expected_policy_set()
    {
        var endpoints = MapEventEndpointsForTest();

        var policies = endpoints
            .SelectMany(e => e.Metadata.GetOrderedMetadata<IAuthorizeData>())
            .Select(data => data.Policy)
            .Where(policy => policy is not null)
            .OrderBy(policy => policy)
            .ToArray();

        Assert.Equal(
            new[]
            {
                Permissions.EventsCreate,
                Permissions.EventsUpdate,
                Permissions.EventsView,
                Permissions.EventsView
            }.OrderBy(p => p).ToArray(),
            policies);
    }

    private static IReadOnlyList<Endpoint> MapEventEndpointsForTest()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddRouting();
        builder.Services.AddPermissionPolicies();
        builder.Services.AddSingleton(Mock.Of<ISender>());

        var app = builder.Build();
        app.MapEventEndpoints();

        return ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(source => source.Endpoints)
            .ToList();
    }
}
