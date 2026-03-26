using ConsolidationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Configures the database mapping for the <see cref="Transaction"/> entity.
/// </summary>
public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    /// <summary>
    /// Configures the entity properties, keys, and column mappings for <see cref="Transaction"/>.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions", "transaction");

        builder.HasKey(x => x.TransactionId);

        builder.Property(x => x.TransactionId)
            .HasColumnName("transaction_id");

        builder.Property(x => x.AccountId)
            .HasColumnName("account_id")
            .IsRequired();

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.TransactionDateUtc)
            .HasColumnName("transaction_date_utc")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(x => x.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(x => x.ConsolidationStatus)
            .HasColumnName("consolidation_status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.ConsolidatedAtUtc)
            .HasColumnName("consolidated_at_utc");

        builder.Property(x => x.LastConsolidationBatchId)
            .HasColumnName("last_consolidation_batch_id");

        builder.Property(x => x.ConsolidationAttemptCount)
            .HasColumnName("consolidation_attempt_count")
            .IsRequired();

        builder.HasIndex(x => x.ConsolidationStatus)
            .HasDatabaseName("ix_transactions_processing_status");
    }
}
