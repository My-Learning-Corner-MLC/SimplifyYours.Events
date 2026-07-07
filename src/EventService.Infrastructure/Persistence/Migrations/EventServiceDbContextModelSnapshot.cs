using System;
using EventService.Domain.Events;
using EventService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EventService.Infrastructure.Persistence.Migrations;

[DbContext(typeof(EventServiceDbContext))]
partial class EventServiceDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.10")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("EventService.Domain.Events.PlannedEvent", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedNever()
                .HasColumnType("uuid")
                .HasColumnName("id");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");

            b.Property<byte[]>("ConcurrencyToken")
                .IsConcurrencyToken()
                .IsRequired()
                .HasColumnType("bytea")
                .HasColumnName("concurrency_token");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("deleted_at");

            b.Property<string>("Description")
                .HasColumnType("text")
                .HasColumnName("description");

            b.Property<DateTimeOffset?>("EventEndTime")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("event_end_time");

            b.Property<DateTimeOffset?>("EventStartTime")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("event_start_time");

            b.Property<DateTimeOffset>("EventTime")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("event_time");

            b.Property<bool>("IsDeleted")
                .ValueGeneratedOnAdd()
                .HasColumnType("boolean")
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("character varying(200)")
                .HasColumnName("name");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid")
                .HasColumnName("tenant_id");

            b.Property<string>("TimeZoneId")
                .HasMaxLength(64)
                .HasColumnType("character varying(64)")
                .HasColumnName("time_zone_id");

            b.Property<EventType>("Type")
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(32)
                .HasColumnType("character varying(32)")
                .HasColumnName("type");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("updated_at");

            b.HasKey("Id")
                .HasName("pk_events");

            b.HasIndex("IsDeleted", "EventTime")
                .HasDatabaseName("ix_events_is_deleted_event_time");

            b.HasIndex("TenantId", "IsDeleted", "EventTime")
                .HasDatabaseName("ix_events_tenant_id_is_deleted_event_time");

            b.ToTable("events", (string)null);
        });

        modelBuilder.Entity("EventService.Domain.Events.PlannedEvent", b =>
        {
            b.OwnsOne("EventService.Domain.Events.EventLocation", "Location", b1 =>
            {
                b1.Property<Guid>("PlannedEventId")
                    .HasColumnType("uuid")
                    .HasColumnName("id");

                b1.Property<string>("Address")
                    .HasMaxLength(500)
                    .HasColumnType("character varying(500)")
                    .HasColumnName("location_address");

                b1.Property<string>("Notes")
                    .HasMaxLength(2000)
                    .HasColumnType("character varying(2000)")
                    .HasColumnName("location_notes");

                b1.Property<string>("VenueName")
                    .HasMaxLength(200)
                    .HasColumnType("character varying(200)")
                    .HasColumnName("location_venue_name");

                b1.HasKey("PlannedEventId");

                b1.ToTable("events");

                b1.WithOwner()
                    .HasForeignKey("PlannedEventId");
            });

            b.Navigation("Location");
        });

        modelBuilder.Entity("EventService.Infrastructure.Persistence.Outbox.OutboxMessage", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedNever()
                .HasColumnType("uuid")
                .HasColumnName("id");

            b.Property<Guid?>("CausationId")
                .HasColumnType("uuid")
                .HasColumnName("causation_id");

            b.Property<Guid>("CorrelationId")
                .HasColumnType("uuid")
                .HasColumnName("correlation_id");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");

            b.Property<string>("Error")
                .HasColumnType("text")
                .HasColumnName("error");

            b.Property<string>("EventType")
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnType("character varying(128)")
                .HasColumnName("event_type");

            b.Property<DateTimeOffset>("OccurredAt")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("occurred_at");

            b.Property<string>("Payload")
                .IsRequired()
                .HasColumnType("jsonb")
                .HasColumnName("payload");

            b.Property<DateTimeOffset?>("ProcessedAt")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("processed_at");

            b.Property<int>("RetryCount")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasDefaultValue(0)
                .HasColumnName("retry_count");

            b.Property<int>("Version")
                .HasColumnType("integer")
                .HasColumnName("version");

            b.HasKey("Id")
                .HasName("pk_outbox_messages");

            b.HasIndex("ProcessedAt", "CreatedAt")
                .HasDatabaseName("ix_outbox_messages_processed_at_created_at");

            b.ToTable("outbox_messages", (string)null);
        });
#pragma warning restore 612, 618
    }
}
