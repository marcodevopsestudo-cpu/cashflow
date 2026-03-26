using ConsolidationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Configures the database mapping for the <see cref="TransactionProcessingError"/> entity.
/// </summary>
public sealed class TransactionProcessingErrorConfiguration : IEntityTypeConfiguration<TransactionProcessingError>
{
    /// <summary>
    /// Configures the entity properties, keys, and column mappings for <see cref="TransactionProcessingError"/>.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<TransactionProcessingError> builder)
    {
        builder.ToTable("transaction_processing_error", "transaction");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.BatchId)
            .HasColumnName("batch_id")
            .IsRequired();

        builder.Property(x => x.TransactionId)
            .HasColumnName("transaction_id");

        builder.Property(x => x.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.ErrorCode)
            .HasColumnName("error_code")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasColumnName("error_message")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.StackTrace)
            .HasColumnName("stack_trace")
            .HasColumnType("text");

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.RetryCount)
            .HasColumnName("retry_count")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(x => x.BatchId)
            .HasDatabaseName("ix_transaction_processing_error_batch_id");

        builder.HasIndex(x => x.TransactionId)
            .HasDatabaseName("ix_transaction_processing_error_transaction_id");
    }
}
