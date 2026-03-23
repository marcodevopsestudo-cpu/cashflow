namespace TransactionService.Application.Abstractions.Messaging;

/// <summary>
/// Defines a publisher for integration events.
/// Supports publishing both domain integration events and stored outbox payloads.
/// </summary>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Publishes the specified integration event.
    /// </summary>
    /// <param name="integrationEvent">The integration event to publish.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken);

    /// <summary>
    /// Publishes the specified stored integration event payload.
    /// </summary>
    /// <param name="integrationEvent">The stored integration event to publish.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task PublishAsync(StoredIntegrationEvent integrationEvent, CancellationToken cancellationToken);

    /// <summary>
    /// Publishes the specified batch of stored integration events as a single message payload.
    /// </summary>
    /// <param name="message">The batch message containing the grouped transactions to publish.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task PublishBatchAsync(StoredIntegrationEventBatch message, CancellationToken cancellationToken);
}
