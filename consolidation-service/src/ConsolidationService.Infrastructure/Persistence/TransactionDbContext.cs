using ConsolidationService.Application.Abstractions;
using ConsolidationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
    /// Gets the DailyBatch entries set.
    /// </summary>
    public DbSet<DailyBatch> DailyBatch => Set<DailyBatch>();

    /// <summary>
    /// Gets the daily balances set.
    /// </summary>
    public DbSet<DailyBalance> DailyBalances => Set<DailyBalance>();



    /// <summary>
    /// Gets the daily TransactionProcessingError set.
    /// </summary>
    public DbSet<TransactionProcessingError> TransactionProcessingError => Set<TransactionProcessingError>();

    
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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TransactionDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
