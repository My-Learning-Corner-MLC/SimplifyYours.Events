using EventService.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure.Persistence;

public sealed class EventServiceDbContext(DbContextOptions<EventServiceDbContext> options)
    : DbContext(options)
{
    public DbSet<PlannedEvent> Events => Set<PlannedEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventServiceDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
