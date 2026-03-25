using TransactionService.Application.Abstractions.Messaging;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(StoredIntegrationEvent integrationEvent, CancellationToken cancellationToken);

    Task PublishBatchAsync(TransactionBatchMessage message, CancellationToken cancellationToken);
}
