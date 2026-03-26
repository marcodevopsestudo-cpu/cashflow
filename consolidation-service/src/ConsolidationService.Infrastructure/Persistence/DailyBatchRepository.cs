using ConsolidationService.Application.Abstractions;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TransactionService.Infrastructure.Persistence;

namespace ConsolidationService.Infrastructure.Persistence;

/// <summary>
/// Provides PostgreSQL persistence operations for managing the lifecycle of consolidation batches.
/// </summary>
public sealed class DailyBatchRepository : IDailyBatchRepository
{
    private readonly TransactionDbContext _dbContext;

    public DailyBatchRepository(TransactionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Retrieves a batch by its identifier.
    /// </summary>
    public async Task<DailyBatch?> GetAsync(Guid batchId, CancellationToken cancellationToken)
    {
        return await _dbContext.Set<DailyBatch>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BatchId == batchId, cancellationToken);
    }

    /// <summary>
    /// Inserts a new batch in pending state or updates an existing one.
    /// </summary>
    public async Task<DailyBatch> UpsertPendingAsync(Guid batchId, string correlationId, int transactionCount, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Set<DailyBatch>()
            .FirstOrDefaultAsync(x => x.BatchId == batchId, cancellationToken);

        if (entity is null)
        {
            entity = new DailyBatch
            {
                BatchId = batchId,
                CorrelationId = correlationId,
                TransactionCount = transactionCount,
                Status = BatchStatus.Pending,
                RetryCount = 0,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _dbContext.AddAsync(entity, cancellationToken);
        }
        else
        {
            entity.CorrelationId = correlationId;
            entity.TransactionCount = transactionCount;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity;
    }

    /// <summary>
    /// Marks the batch as processing.
    /// </summary>
    public async Task MarkAsProcessingAsync(Guid batchId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Set<DailyBatch>()
            .FirstOrDefaultAsync(x => x.BatchId == batchId, cancellationToken);

        if (entity is null) return;

        entity.Status = BatchStatus.Processing;
        entity.StartedAtUtc ??= DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Marks the batch as successfully processed.
    /// </summary>
    public async Task MarkAsSucceededAsync(Guid batchId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Set<DailyBatch>()
            .FirstOrDefaultAsync(x => x.BatchId == batchId, cancellationToken);

        if (entity is null) return;

        entity.Status = BatchStatus.Succeeded;
        entity.CompletedAtUtc = DateTime.UtcNow;
        entity.LastError = null;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Marks the batch as failed and updates retry information.
    /// </summary>
    public async Task MarkAsFailedAsync(Guid batchId, string errorMessage, int retryCount, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Set<DailyBatch>()
            .FirstOrDefaultAsync(x => x.BatchId == batchId, cancellationToken);

        if (entity is null) return;

        entity.Status = BatchStatus.Failed;
        entity.RetryCount = retryCount;
        entity.CompletedAtUtc = DateTime.UtcNow;
        entity.LastError = errorMessage;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
