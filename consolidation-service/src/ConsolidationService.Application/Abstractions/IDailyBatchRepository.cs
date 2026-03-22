using ConsolidationService.Domain.Entities;

namespace ConsolidationService.Application.Abstractions;

/// <summary>
/// Provides persistence operations for controlling the lifecycle of daily batch processing.
/// </summary>
public interface IDailyBatchRepository
{
    /// <summary>
    /// Retrieves a daily batch by its unique identifier.
    /// </summary>
    /// <param name="batchId">The unique identifier of the batch.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// The <see cref="DailyBatch"/> if found; otherwise, <c>null</c>.
    /// </returns>
    Task<DailyBatch?> GetAsync(Guid batchId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new batch in a pending state or updates an existing one.
    /// </summary>
    /// <param name="batchId">The unique identifier of the batch.</param>
    /// <param name="correlationId">The correlation identifier used to trace the batch across systems.</param>
    /// <param name="transactionCount">The number of transactions associated with the batch.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// The created or updated <see cref="DailyBatch"/> in pending state.
    /// </returns>
    Task<DailyBatch> UpsertPendingAsync(Guid batchId, string correlationId, int transactionCount, CancellationToken cancellationToken);

    /// <summary>
    /// Marks the specified batch as being processed.
    /// </summary>
    /// <param name="batchId">The unique identifier of the batch.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task MarkAsProcessingAsync(Guid batchId, CancellationToken cancellationToken);

    /// <summary>
    /// Marks the specified batch as successfully processed.
    /// </summary>
    /// <param name="batchId">The unique identifier of the batch.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task MarkAsSucceededAsync(Guid batchId, CancellationToken cancellationToken);

    /// <summary>
    /// Marks the specified batch as failed.
    /// </summary>
    /// <param name="batchId">The unique identifier of the batch.</param>
    /// <param name="errorMessage">A description of the failure reason.</param>
    /// <param name="retryCount">The number of retry attempts performed.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task MarkAsFailedAsync(Guid batchId, string errorMessage, int retryCount, CancellationToken cancellationToken);
}
