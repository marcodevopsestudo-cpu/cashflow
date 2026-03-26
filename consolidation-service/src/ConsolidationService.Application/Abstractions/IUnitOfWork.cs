namespace ConsolidationService.Application.Abstractions;

/// <summary>
/// Represents a transaction boundary for coordinating persistence operations
/// as a single atomic unit of work.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists pending changes.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of affected rows.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
