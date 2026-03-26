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
        builder.ToTable("transaction_processing_error");

        // ⚠️ EF precisa de uma PK — vamos usar composta
        builder.HasKey(x => new { x.TransactionId, x.BatchId, x.OccurredOnUtc });

        builder.Property(x => x.TransactionId)
            .HasColumnName("transaction_id")
            .IsRequired();

        builder.Property(x => x.BatchId)
            .HasColumnName("batch_id")
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.OccurredOnUtc)
            .HasColumnName("occurred_on_utc")
            .IsRequired();
    }
}
