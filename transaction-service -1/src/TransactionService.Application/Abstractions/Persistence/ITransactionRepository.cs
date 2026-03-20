using TransactionService.Domain.Entities;

namespace TransactionService.Application.Abstractions.Persistence;

/// <summary>
/// Defines persistence operations for transactions.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Adds a transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken);

    /// <summary>
    /// Updates a transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a transaction by identifier.
    /// </summary>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The transaction or null.</returns>
    Task<Transaction?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a transaction for update tracking.
    /// </summary>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tracked transaction or null.</returns>
    Task<Transaction?> GetForUpdateAsync(Guid transactionId, CancellationToken cancellationToken);
}
