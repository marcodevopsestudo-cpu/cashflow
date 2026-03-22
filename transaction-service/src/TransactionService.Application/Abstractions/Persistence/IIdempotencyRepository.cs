using TransactionService.Domain.Entities;

namespace TransactionService.Application.Abstractions.Persistence;

/// <summary>
/// Defines persistence operations for managing idempotency entries.
/// </summary>
/// <remarks>
/// Implementations are responsible for ensuring consistency and atomicity
/// when storing and updating idempotent request state.
/// </remarks>
public interface IIdempotencyRepository
{
    /// <summary>
    /// Retrieves an idempotency entry by its key.
    /// </summary>
    /// <param name="idempotencyKey">The unique idempotency key associated with the request.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// The matching <see cref="IdempotencyEntry"/> if found; otherwise, <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="idempotencyKey"/> is null.
    /// </exception>
    Task<IdempotencyEntry?> GetByKeyAsync(string idempotencyKey, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new idempotency entry to the persistence store.
    /// </summary>
    /// <param name="entry">The idempotency entry to be persisted.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="entry"/> is null.
    /// </exception>
    Task AddAsync(IdempotencyEntry entry, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing idempotency entry in the persistence store.
    /// </summary>
    /// <param name="entry">The idempotency entry to be updated.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="entry"/> is null.
    /// </exception>
    Task UpdateAsync(IdempotencyEntry entry, CancellationToken cancellationToken);
}
