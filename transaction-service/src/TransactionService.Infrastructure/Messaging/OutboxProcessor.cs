using Microsoft.Extensions.Logging;
using TransactionService.Application.Abstractions.Messaging;
using TransactionService.Application.Abstractions.Persistence;
using TransactionService.Domain.Entities;
using TransactionService.Infrastructure.Constants;

namespace TransactionService.Infrastructure.Outbox;

/// <summary>
/// Processes pending outbox messages and publishes them to the integration bus.
/// Supports grouped publishing with individual fallback on failure.
/// </summary>
public sealed class OutboxProcessor : IOutboxProcessor
{
    private const int DefaultPublishChunkSize = 20;

    private readonly IOutboxRepository _outboxRepository;
    private readonly IIntegrationEventPublisher _publisher;
    private readonly ILogger<OutboxProcessor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxProcessor"/> class.
    /// </summary>
    /// <param name="outboxRepository">The outbox repository.</param>
    /// <param name="publisher">The integration event publisher.</param>
    /// <param name="logger">The logger.</param>
    public OutboxProcessor(
        IOutboxRepository outboxRepository,
        IIntegrationEventPublisher publisher,
        ILogger<OutboxProcessor> logger)
    {
        _outboxRepository = outboxRepository;
        _publisher = publisher;
        _logger = logger;
    }

    /// <summary>
    /// Processes pending outbox messages using the provided batch size.
    /// </summary>
    /// <param name="batchSize">The maximum number of pending messages to read.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of successfully processed messages.</returns>
    public async Task<int> ProcessPendingMessagesAsync(
        int batchSize,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(OutboxMessageCatalog.ProcessingStarted, batchSize);

        var messages = await _outboxRepository.GetPendingAsync(batchSize, cancellationToken);

        if (messages.Count == 0)
        {
            _logger.LogInformation(OutboxMessageCatalog.NoPendingMessagesFound);
            return 0;
        }

        var publishChunkSize = Math.Min(batchSize, DefaultPublishChunkSize);
        var processedCount = 0;

        foreach (var group in messages.Chunk(publishChunkSize))
        {
            processedCount += await ProcessMessageGroupAsync(group, cancellationToken);
        }

        _logger.LogInformation(OutboxMessageCatalog.ProcessingFinished, processedCount);

        return processedCount;
    }

    /// <summary>
    /// Processes a group of messages as a single batch publication.
    /// Falls back to individual processing if the batch publish fails.
    /// </summary>
    /// <param name="messages">The messages to process.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of successfully processed messages.</returns>
    private async Task<int> ProcessMessageGroupAsync(
        IReadOnlyCollection<OutboxMessage> messages,
        CancellationToken cancellationToken)
    {
        var batchEvent = MapToBatchIntegrationEvent(messages);

        try
        {
            await _publisher.PublishBatchAsync(batchEvent, cancellationToken);

            await HandleBatchSuccessAsync(messages, cancellationToken);

            foreach (var message in messages)
            {
                _logger.LogInformation(
                    OutboxMessageCatalog.MessagePublishedInBatch,
                    message.Id,
                    message.CorrelationId);
            }

            return messages.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, OutboxMessageCatalog.BatchPublishFailedFallback);

            return await ProcessIndividuallyOnFailureAsync(messages, cancellationToken);
        }
    }

    /// <summary>
    /// Processes each message individually after a batch publish failure.
    /// </summary>
    /// <param name="messages">The messages to process individually.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of successfully processed messages.</returns>
    private async Task<int> ProcessIndividuallyOnFailureAsync(
        IReadOnlyCollection<OutboxMessage> messages,
        CancellationToken cancellationToken)
    {
        var processedCount = 0;

        foreach (var message in messages)
        {
            if (await ProcessMessageAsync(message, cancellationToken))
            {
                processedCount++;
            }
        }

        return processedCount;
    }

    /// <summary>
    /// Processes a single outbox message.
    /// </summary>
    /// <param name="message">The message to process.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the message was successfully processed; otherwise, <c>false</c>.</returns>
    private async Task<bool> ProcessMessageAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            var integrationEvent = MapToIntegrationEvent(message);

            await _publisher.PublishAsync(integrationEvent, cancellationToken);

            await HandleSingleSuccessAsync(message, cancellationToken);

            _logger.LogInformation(
                OutboxMessageCatalog.MessagePublished,
                message.Id,
                message.CorrelationId);

            return true;
        }
        catch (Exception ex)
        {
            await HandleSingleFailureAsync(message, ex, cancellationToken);

            _logger.LogError(
                ex,
                OutboxMessageCatalog.MessagePublishFailed,
                message.Id,
                message.CorrelationId);

            return false;
        }
    }

    /// <summary>
    /// Marks a collection of messages as successfully processed and persists the changes.
    /// </summary>
    /// <param name="messages">The successfully processed messages.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task HandleBatchSuccessAsync(
        IReadOnlyCollection<OutboxMessage> messages,
        CancellationToken cancellationToken)
    {
        foreach (var message in messages)
        {
            message.MarkAsProcessed();
        }

        await _outboxRepository.UpdateRangeAsync(messages, cancellationToken);
        await _outboxRepository.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Marks a single message as successfully processed and persists the changes.
    /// </summary>
    /// <param name="message">The processed message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task HandleSingleSuccessAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        message.MarkAsProcessed();

        await _outboxRepository.UpdateAsync(message, cancellationToken);
        await _outboxRepository.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Marks a single message as failed and persists the changes.
    /// </summary>
    /// <param name="message">The failed message.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task HandleSingleFailureAsync(
        OutboxMessage message,
        Exception exception,
        CancellationToken cancellationToken)
    {
        message.MarkAsFailed(exception.Message);

        await _outboxRepository.UpdateAsync(message, cancellationToken);
        await _outboxRepository.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Maps a collection of outbox messages to a batch integration event.
    /// </summary>
    /// <param name="messages">The messages to map.</param>
    /// <returns>The batch integration event.</returns>
    private static StoredIntegrationEventBatch MapToBatchIntegrationEvent(
        IReadOnlyCollection<OutboxMessage> messages)
    {
        return new StoredIntegrationEventBatch(
            Guid.NewGuid(),
            DateTime.UtcNow,
            messages.Select(m => new StoredIntegrationEventBatchItem(
                m.Id,
                m.AggregateId,
                m.CorrelationId,
                m.OccurredOnUtc,
                m.Payload))
            .ToArray());
    }

    /// <summary>
    /// Maps an outbox message to a single integration event.
    /// </summary>
    /// <param name="message">The message to map.</param>
    /// <returns>The integration event.</returns>
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
}
