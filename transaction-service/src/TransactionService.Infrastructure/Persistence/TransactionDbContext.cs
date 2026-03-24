using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Abstractions.Persistence;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Persistence;

/// <summary>
/// Represents the PostgreSQL database context.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TransactionDbContext"/> class.
/// </remarks>
/// <param name="options">The options.</param>
public sealed class TransactionDbContext(DbContextOptions<TransactionDbContext> options)
    : DbContext(options), IUnitOfWork
{
    /// <summary>
    /// Gets the transactions set.
    /// </summary>
    public DbSet<Transaction> Transactions => Set<Transaction>();

    /// <summary>
    /// Gets the outbox messages set.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>
    /// Gets the idempotency entries set.
    /// </summary>
    public DbSet<IdempotencyEntry> IdempotencyEntries => Set<IdempotencyEntry>();

    /// <summary>
    /// Persists pending changes.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of affected rows.</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Configures the EF model.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(builder =>
        {
            builder.ToTable("transactions", "transaction");
            builder.HasKey(x => x.TransactionId);
            builder.Property(x => x.TransactionId).HasColumnName("transaction_id");
            builder.Property(x => x.AccountId).HasColumnName("account_id").HasMaxLength(100).IsRequired();
            builder.Property(x => x.Kind).HasColumnName("kind").HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(10).IsRequired();
            builder.Property(x => x.TransactionDateUtc).HasColumnName("transaction_date_utc").IsRequired();
            builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
            builder.Property(x => x.CorrelationId).HasColumnName("correlation_id").HasMaxLength(120).IsRequired();
            builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(40).IsRequired();
            builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
            builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
            builder.HasIndex(x => x.AccountId).HasDatabaseName("ix_transactions_account_id");
            builder.HasIndex(x => x.CorrelationId).HasDatabaseName("ix_transactions_correlation_id");
        });

        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable("outbox_messages", "transaction");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("outbox_message_id");
            builder.Property(x => x.EventName).HasColumnName("event_name").HasMaxLength(200).IsRequired();
            builder.Property(x => x.EventVersion).HasColumnName("event_version").IsRequired();
            builder.Property(x => x.AggregateId).HasColumnName("aggregate_id").HasMaxLength(120).IsRequired();
            builder.Property(x => x.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
            builder.Property(x => x.CorrelationId).HasColumnName("correlation_id").HasMaxLength(120).IsRequired();
            builder.Property(x => x.OccurredOnUtc).HasColumnName("occurred_on_utc").IsRequired();
            builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
            builder.Property(x => x.ProcessedOnUtc).HasColumnName("processed_on_utc");
            builder.Property(x => x.Error).HasColumnName("error").HasMaxLength(4000);
            builder.Property(x => x.RetryCount).HasColumnName("retry_count").IsRequired();
            builder.HasIndex(x => x.ProcessedOnUtc).HasDatabaseName("ix_outbox_messages_processed_on_utc");
            builder.HasIndex(x => x.CreatedAtUtc).HasDatabaseName("ix_outbox_messages_created_at_utc");
        });

        modelBuilder.Entity<IdempotencyEntry>(builder =>
        {
            builder.ToTable("idempotency_entries", "transaction");
            builder.HasKey(x => x.IdempotencyKey);
            builder.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(120).IsRequired();
            builder.Property(x => x.RequestHash).HasColumnName("request_hash").HasMaxLength(200).IsRequired();
            builder.Property(x => x.TransactionId).HasColumnName("transaction_id");
            builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
            builder.HasIndex(x => x.IdempotencyKey).IsUnique().HasDatabaseName("ux_idempotency_entries_key");
            builder.HasIndex(x => x.TransactionId).HasDatabaseName("ix_idempotency_entries_transaction_id");
        });


        base.OnModelCreating(modelBuilder);
    }
}
