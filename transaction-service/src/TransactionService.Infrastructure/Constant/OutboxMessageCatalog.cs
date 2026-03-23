namespace TransactionService.Infrastructure.Constants;

/// <summary>
/// Defines log and processing messages used by the outbox infrastructure.
/// </summary>
public static class OutboxMessageCatalog
{
    /// <summary>
    /// Message logged when outbox processing starts.
    /// </summary>
    public const string ProcessingStarted =
        "Outbox processing started. BatchSize: {BatchSize}";

    /// <summary>
    /// Message logged when no pending outbox messages are found.
    /// </summary>
    public const string NoPendingMessagesFound =
        "No pending outbox messages found.";

    /// <summary>
    /// Message logged when outbox processing finishes.
    /// </summary>
    public const string ProcessingFinished =
        "Outbox processing finished. Processed: {Count}";

    /// <summary>
    /// Message logged when a batch publish succeeds for an item.
    /// </summary>
    public const string MessagePublishedInBatch =
        "Outbox message published in batch. Id: {MessageId}, CorrelationId: {CorrelationId}";

    /// <summary>
    /// Message logged when batch publish fails and fallback starts.
    /// </summary>
    public const string BatchPublishFailedFallback =
        "Batch publish failed. Falling back to individual processing.";

    /// <summary>
    /// Message logged when a single message is published successfully.
    /// </summary>
    public const string MessagePublished =
        "Outbox message published. Id: {MessageId}, CorrelationId: {CorrelationId}";

    /// <summary>
    /// Message logged when publishing a single message fails.
    /// </summary>
    public const string MessagePublishFailed =
        "Outbox message publish failed. Id: {MessageId}, CorrelationId: {CorrelationId}";
}
