using EventService.Api.Endpoints;
using EventService.Application;
using EventService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapPingEndpoints();
app.MapEventEndpoints();

app.Run();
