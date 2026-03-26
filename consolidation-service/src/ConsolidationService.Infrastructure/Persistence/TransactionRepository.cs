using ConsolidationService.Application.Abstractions;
using ConsolidationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using TransactionService.Infrastructure.Persistence;

namespace ConsolidationService.Infrastructure.Persistence;

/// <summary>
/// Provides PostgreSQL persistence operations for transaction retrieval and consolidation state transitions.
/// </summary>
public sealed class TransactionRepository : ITransactionRepository
{
    private readonly TransactionDbContext _dbContext;

    public TransactionRepository(TransactionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Retrieves transactions eligible for consolidation filtered by the specified identifiers.
    /// </summary>
    public async Task<IReadOnlyCollection<Transaction>> GetPublishedByIdsAsync(
        IReadOnlyCollection<Guid> transactionIds,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<Transaction>()
            .AsNoTracking()
            .Where(x =>
                    transactionIds.Contains(x.TransactionId) &&
                    (x.ConsolidationStatus == null || x.ConsolidationStatus == ConsolidationStatus.NotStarted));

        var result = await query.ToListAsync(cancellationToken);

        Console.WriteLine($"Query executada {query.ToQueryString()}");

        var sql = query.ToQueryString();
        return result;
    }

    /// <summary>
    /// Marks the specified transactions as successfully consolidated.
    /// </summary>
    public async Task MarkAsConsolidatedAsync(
        IReadOnlyCollection<Guid> transactionIds,
        Guid batchId,
        DateTime consolidatedAtUtc,
        CancellationToken cancellationToken)
    {
        var transactions = await _dbContext.Set<Transaction>()
            .Where(x => transactionIds.Contains(x.TransactionId))
            .ToListAsync(cancellationToken);

        foreach (var tx in transactions)
        {
            tx.ConsolidationStatus = ConsolidationStatus.Consolidated;
            tx.ConsolidatedAtUtc = consolidatedAtUtc;
            tx.LastConsolidationBatchId = batchId;
            tx.ConsolidationAttemptCount += 1;
        }
    }

    /// <summary>
    /// Marks the specified transactions as failed during consolidation.
    /// </summary>
    public async Task MarkAsFailedAsync(
        IReadOnlyCollection<Guid> transactionIds,
        Guid batchId,
        int attemptCount,
        ConsolidationStatus status,
        CancellationToken cancellationToken)
    {
        var transactions = await _dbContext.Set<Transaction>()
            .Where(x => transactionIds.Contains(x.TransactionId))
            .ToListAsync(cancellationToken);

        foreach (var tx in transactions)
        {
            tx.ConsolidationStatus = status;
            tx.LastConsolidationBatchId = batchId;
            tx.ConsolidationAttemptCount = attemptCount;
        }
    }
}
