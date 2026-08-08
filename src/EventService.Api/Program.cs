using EventService.Api.Endpoints;
using EventService.Api.Middleware;
using EventService.Api.Observability;
using EventService.Api.Responses;
using EventService.Api.Security;
using EventService.Application;
using EventService.Application.Authorization;
using EventService.Infrastructure;
using EventService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceObservability("event-service");
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddPermissionPolicies();
builder.Services.AddScoped<CurrentUserAccessor>();
builder.Services.AddScoped<ICurrentUserAccessor>(sp => sp.GetRequiredService<CurrentUserAccessor>());
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Creates the database (if missing) and applies pending migrations. Idempotent,
// so safe on every startup -- needed because containerized environments have no
// separate "run dotnet ef database update" step before the app starts.
using (var migrationScope = app.Services.CreateScope())
{
    migrationScope.ServiceProvider.GetRequiredService<EventServiceDbContext>().Database.Migrate();
}

app.UseFriendlyErrorResponses();
app.UseRequestLogging();
app.UseAuthentication();
app.UseCurrentUser();
app.UseAuthorization();

app.MapPingEndpoints();
app.MapEventEndpoints();

app.Run();
