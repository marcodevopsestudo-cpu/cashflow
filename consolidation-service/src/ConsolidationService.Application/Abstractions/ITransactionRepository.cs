using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.Enums;

namespace ConsolidationService.Application.Abstractions;

/// <summary>
/// Provides data access operations for transaction records created by the Transaction Service,
/// including retrieval and state transitions during batch processing.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Retrieves a collection of transactions that are pending processing,
    /// filtered by the specified transaction identifiers.
    /// </summary>
    /// <param name="transactionIds">
    /// The collection of transaction identifiers to be retrieved.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A read-only collection of <see cref="Transaction"/> instances that are in a pending state.
    /// </returns>
    Task<IReadOnlyCollection<Transaction>> GetPendingByIdsAsync(IReadOnlyCollection<Guid> transactionIds, CancellationToken cancellationToken);

    /// <summary>
    /// Marks the specified transactions as successfully consolidated.
    /// </summary>
    /// <param name="transactionIds">
    /// The collection of transaction identifiers to update.
    /// </param>
    /// <param name="batchId">
    /// The identifier of the batch responsible for the consolidation.
    /// </param>
    /// <param name="consolidatedAtUtc">
    /// The UTC timestamp when the consolidation was completed.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous update operation.
    /// </returns>
    Task MarkAsConsolidatedAsync(IReadOnlyCollection<Guid> transactionIds, Guid batchId, DateTime consolidatedAtUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Marks the specified transactions as failed during processing.
    /// </summary>
    /// <param name="transactionIds">
    /// The collection of transaction identifiers to update.
    /// </param>
    /// <param name="batchId">
    /// The identifier of the batch associated with the processing attempt.
    /// </param>
    /// <param name="attemptCount">
    /// The number of processing attempts performed.
    /// </param>
    /// <param name="status">
    /// The resulting processing status indicating the failure outcome.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous update operation.
    /// </returns>
    /// <remarks>
    /// This method is typically used when transaction processing fails and requires retry
    /// or manual intervention, depending on the <paramref name="status"/>.
    /// </remarks>
    Task MarkAsFailedAsync(IReadOnlyCollection<Guid> transactionIds, Guid batchId, int attemptCount, TransactionStatus status, CancellationToken cancellationToken);
}
