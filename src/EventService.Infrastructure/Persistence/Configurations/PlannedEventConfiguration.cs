using EventService.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventService.Infrastructure.Persistence.Configurations;

internal sealed class PlannedEventConfiguration : IEntityTypeConfiguration<PlannedEvent>
{
    public void Configure(EntityTypeBuilder<PlannedEvent> builder)
    {
        builder.ToTable("events");

        builder.HasKey(plannedEvent => plannedEvent.Id)
            .HasName("pk_events");

        builder.Property(plannedEvent => plannedEvent.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(plannedEvent => plannedEvent.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(plannedEvent => plannedEvent.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(plannedEvent => plannedEvent.EventTime)
            .HasColumnName("event_time")
            .IsRequired();

        builder.Property(plannedEvent => plannedEvent.EventStartTime)
            .HasColumnName("event_start_time");

        builder.Property(plannedEvent => plannedEvent.EventEndTime)
            .HasColumnName("event_end_time");

        builder.Property(plannedEvent => plannedEvent.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(plannedEvent => plannedEvent.Description)
            .HasColumnName("description");

        builder.OwnsOne(plannedEvent => plannedEvent.Location, location =>
        {
            location.Property(eventLocation => eventLocation.VenueName)
                .HasColumnName("location_venue_name")
                .HasMaxLength(EventLocation.VenueNameMaxLength);

            location.Property(eventLocation => eventLocation.Address)
                .HasColumnName("location_address")
                .HasMaxLength(EventLocation.AddressMaxLength);

            location.Property(eventLocation => eventLocation.Notes)
                .HasColumnName("location_notes")
                .HasMaxLength(EventLocation.NotesMaxLength);
        });

        builder.Property(plannedEvent => plannedEvent.TimeZoneId)
            .HasColumnName("time_zone_id")
            .HasMaxLength(64);

        builder.Property(plannedEvent => plannedEvent.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(plannedEvent => plannedEvent.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(plannedEvent => plannedEvent.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(plannedEvent => plannedEvent.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(plannedEvent => plannedEvent.ConcurrencyToken)
            .HasColumnName("concurrency_token")
            .HasColumnType("bytea")
            .IsConcurrencyToken()
            .IsRequired();

        builder.HasIndex(plannedEvent => new { plannedEvent.IsDeleted, plannedEvent.EventTime })
            .HasDatabaseName("ix_events_is_deleted_event_time");

        builder.HasIndex(plannedEvent => new { plannedEvent.TenantId, plannedEvent.IsDeleted, plannedEvent.EventTime })
            .HasDatabaseName("ix_events_tenant_id_is_deleted_event_time");
    }
}
