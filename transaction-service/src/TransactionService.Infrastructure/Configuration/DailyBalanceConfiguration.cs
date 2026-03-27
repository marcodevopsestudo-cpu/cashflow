using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Domain.Entities;

/// <summary>
/// Configures the database mapping for the <see cref="OutboxMessage"/> entity.
/// </summary>
public class DailyBalanceConfiguration : IEntityTypeConfiguration<DailyBalance>
{
    /// <summary>
    /// Configures the entity properties, keys, and indexes for <see cref="DailyBalance"/>.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<DailyBalance> builder)
    {

        builder.ToTable("daily_balance","transaction");

        builder.HasKey(x => x.BalanceDate);

        builder.Property(x => x.BalanceDate)
            .HasColumnName("balance_date");

        builder.Property(x => x.TotalCredits)
            .HasColumnName("total_credits");

        builder.Property(x => x.TotalDebits)
            .HasColumnName("total_debits");

        builder.Property(x => x.Balance)
            .HasColumnName("balance");


        builder.Property(x => x.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");
        
    }
}
