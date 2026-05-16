using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EventService.Infrastructure.Persistence;

public sealed class EventServiceDbContextFactory : IDesignTimeDbContextFactory<EventServiceDbContext>
{
    public EventServiceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EventServiceDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__EventServiceDb")
            ?? "Host=localhost;Database=simplify_yours_event_service_design_time";

        optionsBuilder.UseNpgsql(connectionString);

        return new EventServiceDbContext(optionsBuilder.Options);
    }
}
