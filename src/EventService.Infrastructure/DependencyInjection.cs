using EventService.Application.Abstractions.Events;
using EventService.Infrastructure.Persistence;
using EventService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        return services;
    }
}
