using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;
using TransactionService.Domain.Entities;

public class IdempotencyEntryConfiguration : IEntityTypeConfiguration<IdempotencyEntry>
{
    public void Configure(EntityTypeBuilder<IdempotencyEntry> builder)
    {
        builder.ToTable("idempotency_entries", "transaction");
        builder.HasKey(x => x.IdempotencyKey);
        builder.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(120).IsRequired();
        builder.Property(x => x.RequestHash).HasColumnName("request_hash").HasMaxLength(200).IsRequired();
        builder.Property(x => x.TransactionId).HasColumnName("transaction_id");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.HasIndex(x => x.IdempotencyKey).IsUnique().HasDatabaseName("ux_idempotency_entries_key");
        builder.HasIndex(x => x.TransactionId).HasDatabaseName("ix_idempotency_entries_transaction_id");

    }
}
