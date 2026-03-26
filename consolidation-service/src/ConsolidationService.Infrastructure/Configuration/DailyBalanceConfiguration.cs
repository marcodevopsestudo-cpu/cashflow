using ConsolidationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Configures the database mapping for the <see cref="DailyBalance"/> entity.
/// </summary>
public sealed class DailyBalanceConfiguration : IEntityTypeConfiguration<DailyBalance>
{
    /// <summary>
    /// Configures the entity properties, keys, and column mappings for <see cref="DailyBalance"/>.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<DailyBalance> builder)
    {
        builder.ToTable("daily_balance");

        // PK
        builder.HasKey(x => x.BalanceDate);

        builder.Property(x => x.BalanceDate)
            .HasColumnName("balance_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.TotalCredits)
            .HasColumnName("total_credits")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.TotalDebits)
            .HasColumnName("total_debits")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.Balance)
            .HasColumnName("balance")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();
    }
}
