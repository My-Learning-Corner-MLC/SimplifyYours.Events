using EventService.Application.Abstractions.Common;
using EventService.Application.Abstractions.Events;
using EventService.Application.Abstractions.IntegrationEvents;
using EventService.Infrastructure.Persistence;
using EventService.Infrastructure.Persistence.Outbox;
using EventService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimplifyYours.Event.Abstractions;
using SimplifyYours.Event.Publisher;

namespace EventService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EventServiceDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("EventServiceDb");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'EventServiceDb' is required to use Event service persistence.");
            }

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IEventRepository, EfCoreEventRepository>();
        services.AddScoped<IIntegrationEventOutbox, EfCoreIntegrationEventOutbox>();
        services.AddScoped<IEventOutboxStore, EventServiceOutboxStore>();
        services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();
        services.AddSimplifyYoursEventPublisher(options =>
        {
            options.BootstrapServers = configuration["Kafka:BootstrapServers"];
            options.DefaultTopic = configuration["Kafka:EventReferenceTopic"];
            options.BatchSize = configuration.GetValue("Kafka:OutboxBatchSize", 25);
            options.PollingInterval = TimeSpan.FromSeconds(
                configuration.GetValue("Kafka:OutboxPollingIntervalSeconds", 5));
            options.MaxPublishAttempts = configuration.GetValue("Kafka:MaxPublishAttempts", 5);
        });

        return services;
    }
}
