using TransactionService.Domain.Entities;

namespace TransactionService.Application.Abstractions.Persistence;

/// <summary>
/// Defines persistence operations for managing outbox messages.
/// Provides methods for adding, retrieving, and updating messages
/// as part of the Outbox pattern.
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Adds a new outbox message to the persistence store.
    /// </summary>
    /// <param name="message">The outbox message to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a batch of pending outbox messages ordered by creation time.
    /// Only messages that have not yet been processed are returned.
    /// </summary>
    /// <param name="batchSize">The maximum number of messages to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only list of pending outbox messages.</returns>
    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
        int batchSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Marks a single outbox message as modified in the persistence context.
    /// This does not persist changes until <see cref="SaveChangesAsync"/> is called.
    /// </summary>
    /// <param name="message">The message to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken);

    /// <summary>
    /// Marks a collection of outbox messages as modified in the persistence context.
    /// This enables batch updates for improved performance.
    /// Changes are not persisted until <see cref="SaveChangesAsync"/> is called.
    /// </summary>
    /// <param name="messages">The collection of messages to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateRangeAsync(
        IEnumerable<OutboxMessage> messages,
        CancellationToken cancellationToken);

    /// <summary>
    /// Persists all pending changes to the underlying data store.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
