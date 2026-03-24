using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Domain.Entities;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions", "transaction");

        builder.HasKey(x => x.TransactionId);

        builder.Property(x => x.TransactionId)
            .HasColumnName("transaction_id");

        builder.Property(x => x.AccountId)
            .HasColumnName("account_id");

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasConversion<string>(); 

        builder.Property(x => x.Amount)
            .HasColumnName("amount");

        builder.Property(x => x.Currency)
            .HasColumnName("currency");

        builder.Property(x => x.TransactionDateUtc)
            .HasColumnName("transaction_date_utc");

        builder.Property(x => x.Description)
            .HasColumnName("description");

        builder.Property(x => x.CorrelationId)
            .HasColumnName("correlation_id");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(); 

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc");

        builder.Property(x => x.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");
    }
}
