using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventService.Infrastructure.Persistence.Outbox;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(message => message.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(message => message.OccurredAt)
            .HasColumnName("occurred_at")
            .IsRequired();

        builder.Property(message => message.CorrelationId)
            .HasColumnName("correlation_id")
            .IsRequired();

        builder.Property(message => message.CausationId)
            .HasColumnName("causation_id");

        builder.Property(message => message.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(message => message.Version)
            .HasColumnName("version")
            .IsRequired();

        builder.Property(message => message.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(message => message.ProcessedAt)
            .HasColumnName("processed_at");

        builder.Property(message => message.RetryCount)
            .HasColumnName("retry_count")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(message => message.Error)
            .HasColumnName("error");

        builder.HasIndex(message => new { message.ProcessedAt, message.CreatedAt })
            .HasDatabaseName("ix_outbox_messages_processed_at_created_at");
    }
}
