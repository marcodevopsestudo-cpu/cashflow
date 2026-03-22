namespace TransactionService.Application.Abstractions.Messaging;

/// <summary>
/// Defines a publisher for integration events.
/// </summary>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Publishes the specified integration event.
    /// </summary>
    /// <param name="integrationEvent">The event to publish.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken);

    /// <summary>
    /// Publishes the specified stored integration event payload.
    /// </summary>
    /// <param name="integrationEvent">The stored event to publish.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task PublishAsync(StoredIntegrationEvent integrationEvent, CancellationToken cancellationToken);
}
