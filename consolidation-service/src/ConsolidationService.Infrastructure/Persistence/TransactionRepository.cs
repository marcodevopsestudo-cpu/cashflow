using ConsolidationService.Application.Abstractions;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TransactionService.Infrastructure.Persistence;

namespace ConsolidationService.Infrastructure.Persistence;

/// <summary>
/// Provides PostgreSQL persistence operations for transaction retrieval and state transitions.
/// </summary>
public sealed class TransactionRepository : ITransactionRepository
{
    private readonly TransactionDbContext _dbContext;

    public TransactionRepository(TransactionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Retrieves pending transactions filtered by the specified identifiers.
    /// </summary>
    public async Task<IReadOnlyCollection<Transaction>> GetPendingByIdsAsync(
        IReadOnlyCollection<Guid> transactionIds,
        CancellationToken cancellationToken)
    {
        var result = await _dbContext.Set<Transaction>()
            .AsNoTracking()
            .Where(x =>
                transactionIds.Contains(x.Id) &&
                x.ProcessingStatus == TransactionProcessingStatus.Pending)
            .ToListAsync(cancellationToken);

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
            .Where(x => transactionIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        foreach (var tx in transactions)
        {
            tx.ProcessingStatus = TransactionProcessingStatus.Consolidated;
            tx.LastBatchId = batchId;
            tx.ConsolidatedAtUtc = consolidatedAtUtc;
            tx.ProcessingAttemptCount++;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Marks the specified transactions as failed during processing.
    /// </summary>
    public async Task MarkAsFailedAsync(
        IReadOnlyCollection<Guid> transactionIds,
        Guid batchId,
        int attemptCount,
        TransactionProcessingStatus status,
        CancellationToken cancellationToken)
    {
        var transactions = await _dbContext.Set<Transaction>()
            .Where(x => transactionIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        foreach (var tx in transactions)
        {
            tx.ProcessingStatus = status;
            tx.LastBatchId = batchId;
            tx.ProcessingAttemptCount = attemptCount;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
