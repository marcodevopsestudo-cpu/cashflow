using TransactionService.Domain.Entities;

namespace TransactionService.Application.Abstractions.Persistence;

/// <summary>
/// Defines persistence operations for outbox messages.
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Adds a new outbox message.
    /// </summary>
    /// <param name="message">The outbox message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken);

    /// <summary>
    /// Gets pending outbox messages ordered by creation time.
    /// </summary>
    /// <param name="batchSize">The maximum number of messages to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The pending outbox messages.</returns>
    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an outbox message.
    /// </summary>
    /// <param name="message">The message to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken);
}
