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

        _logger.LogInformation(
            OutboxMessageCatalog.PendingMessagesRetrieved,
            messages.Count,
            batchSize,
            string.Join(",", messages.Select(message => message.Id)));

        var publishChunkSize = Math.Min(batchSize, DefaultPublishChunkSize);
        var processedCount = 0;

        _logger.LogInformation(
            OutboxMessageCatalog.GroupedProcessingStarted,
            publishChunkSize,
            messages.Count);

        foreach (var group in messages.Chunk(publishChunkSize))
        {
            processedCount += await ProcessMessageGroupAsync(group, cancellationToken);
        }

        _logger.LogInformation(OutboxMessageCatalog.ProcessingFinished, processedCount);

        _logger.LogInformation(
            OutboxMessageCatalog.GroupedProcessingFinished,
            messages.Count,
            processedCount,
            messages.Count - processedCount);

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

        _logger.LogInformation(
            OutboxMessageCatalog.BatchProcessingStarted,
            messages.Count,
            string.Join(",", messages.Select(message => message.Id)),
            string.Join(",", messages.Select(message => message.CorrelationId)),
            string.Join(",", messages.Select(message => message.AggregateId)));

        try
        {
            _logger.LogDebug(
                OutboxMessageCatalog.PublishingBatchToBus,
                messages.Count,
                batchEvent.BatchId);

            await _publisher.PublishBatchAsync(batchEvent, cancellationToken);

            _logger.LogInformation(
                OutboxMessageCatalog.BatchPublishReturnedSuccessfully,
                messages.Count,
                batchEvent.BatchId);

            _logger.LogDebug(
                OutboxMessageCatalog.MarkingBatchAsProcessed,
                messages.Count,
                batchEvent.BatchId);

            await HandleBatchSuccessAsync(messages, cancellationToken);

            foreach (var message in messages)
            {
                _logger.LogInformation(
                    OutboxMessageCatalog.MessagePublishedInBatch,
                    message.Id,
                    message.CorrelationId);

                _logger.LogDebug(
                    OutboxMessageCatalog.MessageMarkedAsProcessedAfterBatch,
                    message.Id,
                    message.CorrelationId,
                    message.AggregateId);
            }

            return messages.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                OutboxMessageCatalog.BatchPublishFailedDetailed,
                messages.Count,
                string.Join(",", messages.Select(message => message.Id)),
                string.Join(",", messages.Select(message => message.CorrelationId)),
                string.Join(",", messages.Select(message => message.AggregateId)));

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

        _logger.LogInformation(
            OutboxMessageCatalog.IndividualFallbackStarted,
            messages.Count,
            string.Join(",", messages.Select(message => message.Id)));

        foreach (var message in messages)
        {
            if (await ProcessMessageAsync(message, cancellationToken))
            {
                processedCount++;
            }
        }

        _logger.LogInformation(
            OutboxMessageCatalog.IndividualFallbackFinished,
            messages.Count,
            processedCount,
            messages.Count - processedCount);

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
            _logger.LogDebug(
                OutboxMessageCatalog.PublishingSingleMessage,
                message.Id,
                message.CorrelationId,
                message.EventName,
                message.AggregateId);

            var integrationEvent = MapToIntegrationEvent(message);

            await _publisher.PublishAsync(integrationEvent, cancellationToken);

            _logger.LogInformation(
                OutboxMessageCatalog.SingleMessagePublishedSuccessfully,
                message.Id,
                message.CorrelationId);

            _logger.LogDebug(
                OutboxMessageCatalog.MarkingSingleMessageAsProcessed,
                message.Id,
                message.CorrelationId);

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

            _logger.LogError(
                ex,
                OutboxMessageCatalog.SingleMessageFailedDetailed,
                message.Id,
                message.CorrelationId,
                message.EventName,
                message.AggregateId);

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
            _logger.LogDebug(
                OutboxMessageCatalog.SettingProcessedOnUtcForBatchMessage,
                message.Id,
                message.CorrelationId);

            message.MarkAsProcessed();
        }

        _logger.LogDebug(
            OutboxMessageCatalog.PersistingProcessedBatchMessages,
            messages.Count,
            string.Join(",", messages.Select(message => message.Id)));

        await _outboxRepository.UpdateRangeAsync(messages, cancellationToken);
        await _outboxRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            OutboxMessageCatalog.ProcessedBatchPersistedSuccessfully,
            messages.Count,
            string.Join(",", messages.Select(message => message.Id)));
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
        _logger.LogDebug(
            OutboxMessageCatalog.SettingProcessedOnUtcForSingleMessage,
            message.Id,
            message.CorrelationId);

        message.MarkAsProcessed();

        await _outboxRepository.UpdateAsync(message, cancellationToken);
        await _outboxRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            OutboxMessageCatalog.SingleProcessedPersistedSuccessfully,
            message.Id,
            message.CorrelationId);
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
        _logger.LogWarning(
            OutboxMessageCatalog.MarkingMessageAsFailed,
            message.Id,
            message.CorrelationId,
            exception.Message);

        message.MarkAsFailed(exception.Message);

        await _outboxRepository.UpdateAsync(message, cancellationToken);
        await _outboxRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            OutboxMessageCatalog.FailedMessagePersistedSuccessfully,
            message.Id,
            message.CorrelationId);
    }

    /// <summary>
    /// Maps a collection of outbox messages to a lightweight batch integration event.
    /// Publishes only the batch identifier and the transaction identifiers.
    /// </summary>
    /// <param name="messages">The messages to map.</param>
    /// <returns>The lightweight batch integration event.</returns>
    private static TransactionBatchMessage MapToBatchIntegrationEvent(
        IReadOnlyCollection<OutboxMessage> messages) => new(
                 Guid.NewGuid(),
                 [.. messages.Select(m => Guid.Parse(m.AggregateId))]);

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
