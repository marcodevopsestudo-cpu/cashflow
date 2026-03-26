using ConsolidationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Configures the database mapping for the <see cref="DailyBatch"/> entity.
/// </summary>
public sealed class DailyBatchConfiguration : IEntityTypeConfiguration<DailyBatch>
{
    /// <summary>
    /// Configures the entity properties, keys, and column mappings for <see cref="DailyBatch"/>.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<DailyBatch> builder)
    {
        builder.ToTable("daily_batch", "transaction");

        builder.HasKey(x => x.BatchId);

        builder.Property(x => x.BatchId)
            .HasColumnName("batch_id");

        builder.Property(x => x.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.TransactionCount)
            .HasColumnName("transaction_count")
            .IsRequired();

        builder.Property(x => x.RetryCount)
            .HasColumnName("retry_count")
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasColumnName("last_error")
            .HasColumnType("text");

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.StartedAtUtc)
            .HasColumnName("started_at_utc");

        builder.Property(x => x.CompletedAtUtc)
            .HasColumnName("completed_at_utc");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_daily_batch_status");
    }
}
