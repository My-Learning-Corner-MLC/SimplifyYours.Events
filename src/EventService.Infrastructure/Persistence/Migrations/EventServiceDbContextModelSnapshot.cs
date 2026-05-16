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

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("deleted_at");

            b.Property<string>("Description")
                .HasColumnType("text")
                .HasColumnName("description");

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

            b.ToTable("events", (string)null);
        });
#pragma warning restore 612, 618
    }
}
