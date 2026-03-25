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


    public const string PendingMessagesRetrieved =
        "Pending outbox messages retrieved. Count={Count}, BatchSize={BatchSize}, MessageIds={MessageIds}";

    public const string GroupedProcessingStarted =
        "Starting grouped outbox processing. PublishChunkSize={PublishChunkSize}, TotalMessages={TotalMessages}";

    public const string GroupedProcessingFinished =
        "Outbox grouped processing finished. TotalMessages={TotalMessages}, SuccessfullyProcessed={SuccessfullyProcessed}, Remaining={Remaining}";

    public const string BatchProcessingStarted =
        "Processing outbox batch. Count={Count}, MessageIds={MessageIds}, CorrelationIds={CorrelationIds}, AggregateIds={AggregateIds}";

    public const string BatchPublishReturnedSuccessfully =
        "Outbox batch publish returned successfully. Count={Count}, BatchEventId={BatchEventId}";

    public const string BatchPublishFailedDetailed =
        "Batch publish failed. Falling back to individual processing. Count={Count}, MessageIds={MessageIds}, CorrelationIds={CorrelationIds}, AggregateIds={AggregateIds}";

    public const string IndividualFallbackStarted =
        "Starting individual fallback processing for outbox messages. Count={Count}, MessageIds={MessageIds}";

    public const string IndividualFallbackFinished =
        "Individual fallback processing finished. Attempted={Attempted}, Succeeded={Succeeded}, Failed={Failed}";

    public const string SingleMessagePublishedSuccessfully =
        "Single outbox message published successfully. MessageId={MessageId}, CorrelationId={CorrelationId}";

    public const string SingleMessageFailedDetailed =
        "Failed to publish and process single outbox message. MessageId={MessageId}, CorrelationId={CorrelationId}, EventName={EventName}, AggregateId={AggregateId}";

    public const string ProcessedBatchPersistedSuccessfully =
        "Processed batch messages persisted successfully. Count={Count}, MessageIds={MessageIds}";

    public const string SingleProcessedPersistedSuccessfully =
        "Single processed outbox message persisted successfully. MessageId={MessageId}, CorrelationId={CorrelationId}";

    public const string MarkingMessageAsFailed =
        "Marking outbox message as failed. MessageId={MessageId}, CorrelationId={CorrelationId}, Error={Error}";

    public const string FailedMessagePersistedSuccessfully =
        "Failed outbox message persisted successfully. MessageId={MessageId}, CorrelationId={CorrelationId}";


    public const string PublishingBatchToBus =
        "Publishing outbox batch to integration bus. Count={Count}, BatchEventId={BatchEventId}";

    public const string MarkingBatchAsProcessed =
        "Marking outbox batch messages as processed. Count={Count}, BatchEventId={BatchEventId}";

    public const string MessageMarkedAsProcessedAfterBatch =
        "Outbox message marked as processed after batch publish. MessageId={MessageId}, CorrelationId={CorrelationId}, AggregateId={AggregateId}";

    public const string PublishingSingleMessage =
        "Publishing single outbox message. MessageId={MessageId}, CorrelationId={CorrelationId}, EventName={EventName}, AggregateId={AggregateId}";

    public const string MarkingSingleMessageAsProcessed =
        "Marking single outbox message as processed. MessageId={MessageId}, CorrelationId={CorrelationId}";

    public const string SettingProcessedOnUtcForBatchMessage =
        "Setting ProcessedOnUtc for outbox message in batch. MessageId={MessageId}, CorrelationId={CorrelationId}";

    public const string PersistingProcessedBatchMessages =
        "Persisting processed batch messages. Count={Count}, MessageIds={MessageIds}";

    public const string SettingProcessedOnUtcForSingleMessage =
        "Setting ProcessedOnUtc for single outbox message. MessageId={MessageId}, CorrelationId={CorrelationId}";

}
