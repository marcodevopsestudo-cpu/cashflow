using ConsolidationService.Domain.Entities;

namespace ConsolidationService.Application.Abstractions;

/// <summary>
/// Provides persistence operations for storing transaction processing errors that require manual review.
/// </summary>
public interface ITransactionProcessingErrorRepository
{
    /// <summary>
    /// Inserts a collection of transaction processing errors into the persistence store.
    /// </summary>
    /// <param name="items">
    /// The collection of <see cref="TransactionProcessingError"/> instances representing failed transactions.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous insert operation.
    /// </returns>
    /// <remarks>
    /// This method is typically used when a batch cannot be fully processed automatically,
    /// requiring manual intervention or further analysis.
    /// </remarks>
    Task InsertAsync(IReadOnlyCollection<TransactionProcessingError> items, CancellationToken cancellationToken);
}
