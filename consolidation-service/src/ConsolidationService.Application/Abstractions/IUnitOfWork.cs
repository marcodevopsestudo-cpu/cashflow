namespace ConsolidationService.Application.Abstractions;

/// <summary>
/// Represents a transaction boundary for coordinating persistence operations
/// as a single atomic unit of work.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Begins a new transactional scope for persistence operations.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// An <see cref="IAsyncDisposable"/> representing the transactional scope.
    /// Disposing the returned instance is expected to finalize the transaction
    /// (commit or rollback, depending on the implementation).
    /// </returns>
    /// <remarks>
    /// The implementation should ensure that all operations executed within the scope
    /// participate in the same transaction. Proper disposal of the returned scope is required
    /// to guarantee consistency and resource cleanup.
    /// </remarks>
    Task<IAsyncDisposable> BeginAsync(CancellationToken cancellationToken);
}
