using Microsoft.Extensions.Logging;
using TransactionService.Application.Abstractions.Messaging;
using TransactionService.Application.Abstractions.Persistence;
using TransactionService.Application.Common.Diagnostics;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Messaging;

/// <summary>
/// Processes pending outbox messages and publishes them to Azure Service Bus.
/// </summary>
public sealed class OutboxProcessor : IOutboxProcessor
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IIntegrationEventPublisher _publisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OutboxProcessor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxProcessor"/> class.
    /// </summary>
    /// <param name="outboxRepository">The outbox repository.</param>
    /// <param name="transactionRepository">The transaction repository.</param>
    /// <param name="publisher">The integration event publisher.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    public OutboxProcessor(
        IOutboxRepository outboxRepository,
        ITransactionRepository transactionRepository,
        IIntegrationEventPublisher publisher,
        IUnitOfWork unitOfWork,
        ILogger<OutboxProcessor> logger)
    {
        _outboxRepository = outboxRepository;
        _transactionRepository = transactionRepository;
        _publisher = publisher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Processes a batch of pending outbox messages.
    /// </summary>
    /// <param name="batchSize">The batch size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of processed messages.</returns>
    public async Task<int> ProcessPendingMessagesAsync(int batchSize, CancellationToken cancellationToken)
    {
        _logger.OutboxProcessingStarted(batchSize);

        var messages = await _outboxRepository.GetPendingAsync(batchSize, cancellationToken);

        var processedCount = 0;

        foreach (var message in messages)
        {
            if (await ProcessMessageAsync(message, cancellationToken))
            {
                processedCount++;
            }
        }

        _logger.OutboxProcessingFinished(processedCount);

        return processedCount;
    }
    private static StoredIntegrationEvent MapToIntegrationEvent(OutboxMessage message)
    {
        return new StoredIntegrationEvent(
            message.Id,
            message.EventName,
            message.EventVersion,
            message.AggregateId,
            message.CorrelationId,
            message.OccurredOnUtc,
            message.Payload);
    }

    private async Task UpdateTransactionStatusAsync(
    string aggregateId,
    bool markAsPublished,
    CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(aggregateId, out var transactionId))
        {
            return;
        }

        var transaction = await _transactionRepository.GetForUpdateAsync(transactionId, cancellationToken);

        if (transaction is null)
        {
            return;
        }

        if (markAsPublished)
        {
            transaction.MarkAsPublished();
        }
        else
        {
            transaction.MarkAsFailedToPublish();
        }

        await _transactionRepository.UpdateAsync(transaction, cancellationToken);
    }
    private async Task HandleFailureAsync(
    OutboxMessage message,
    Exception exception,
    CancellationToken cancellationToken)
    {
        message.MarkAsFailed(exception.Message);

        await _outboxRepository.UpdateAsync(message, cancellationToken);

        await UpdateTransactionStatusAsync(
            message.AggregateId,
            markAsPublished: false,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.OutboxMessagePublishFailed(exception, message.Id, message.CorrelationId);
    }

    private async Task HandleSuccessAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        message.MarkAsProcessed();

        await _outboxRepository.UpdateAsync(message, cancellationToken);

        await UpdateTransactionStatusAsync(
            message.AggregateId,
            markAsPublished: true,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
    private async Task<bool> ProcessMessageAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var integrationEvent = MapToIntegrationEvent(message);

        try
        {
            await _publisher.PublishAsync(integrationEvent, cancellationToken);

            await HandleSuccessAsync(message, cancellationToken);
            _logger.OutboxMessagePublished(message.Id, message.CorrelationId);
            return true;
        }
        catch (Exception ex)
        {
            await HandleFailureAsync(message, ex, cancellationToken);
            return false;
        }
    }
}
