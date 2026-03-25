using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Domain.Entities;

/// <summary>
/// Configures the database mapping for the <see cref="OutboxMessage"/> entity.
/// </summary>
public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    /// <summary>
    /// Configures the entity properties, keys, and indexes for <see cref="OutboxMessage"/>.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages", "transaction");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("outbox_message_id");

        builder.Property(x => x.EventName)
            .HasColumnName("event_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.EventVersion)
            .HasColumnName("event_version")
            .IsRequired();

        builder.Property(x => x.AggregateId)
            .HasColumnName("aggregate_id")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.OccurredOnUtc)
            .HasColumnName("occurred_on_utc")
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.ProcessedOnUtc)
            .HasColumnName("processed_on_utc");

        builder.Property(x => x.Error)
            .HasColumnName("error")
            .HasMaxLength(4000);

        builder.Property(x => x.RetryCount)
            .HasColumnName("retry_count")
            .IsRequired();

        builder.HasIndex(x => x.ProcessedOnUtc)
            .HasDatabaseName("ix_outbox_messages_processed_on_utc");

        builder.HasIndex(x => x.CreatedAtUtc)
            .HasDatabaseName("ix_outbox_messages_created_at_utc");
    }
}
