namespace TransactionService.Application.Abstractions.Messaging;

/// <summary>
/// Defines a service responsible for publishing pending outbox messages.
/// </summary>
public interface IOutboxProcessor
{
    /// <summary>
    /// Processes a batch of pending outbox messages.
    /// </summary>
    /// <param name="batchSize">The batch size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of processed messages.</returns>
    Task<int> ProcessPendingMessagesAsync(int batchSize, CancellationToken cancellationToken);
}
