using EventService.Api.Endpoints;
using EventService.Api.Middleware;
using EventService.Api.Observability;
using EventService.Api.Responses;
using EventService.Api.Security;
using EventService.Application;
using EventService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceObservability("event-service");
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseFriendlyErrorResponses();
app.UseRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapPingEndpoints();
app.MapEventEndpoints();

app.Run();
