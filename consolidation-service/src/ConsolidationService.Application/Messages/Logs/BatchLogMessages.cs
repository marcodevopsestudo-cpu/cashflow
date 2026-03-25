namespace ConsolidationService.Application.Messages.Logs;

/// <summary>
/// Contains structured log message templates related to batch processing within the consolidation workflow.
/// </summary>
public static class BatchLogMessages
{
    /// <summary>
    /// Contains log messages related to the main batch processing workflow.
    /// </summary>
    public static class Workflow
    {
        /// <summary>
        /// Log emitted when batch processing starts.
        /// </summary>
        public const string ProcessingStarted = "Starting consolidation batch processing. BatchId: {BatchId}, MessageId: {MessageId}, TransactionCount: {TransactionCount}.";

        /// <summary>
        /// Log emitted when batch processing finishes.
        /// </summary>
        public const string ProcessingFinished = "Finished consolidation batch processing. BatchId: {BatchId}, MessageId: {MessageId}.";

        /// <summary>
        /// Log emitted when the batch has already been processed or ignored.
        /// </summary>
        public const string AlreadyProcessed = "Batch {BatchId} has already been processed or ignored.";

        /// <summary>
        /// Log emitted when the batch is successfully registered for processing.
        /// </summary>
        public const string RegisteredForProcessing = "Batch registered for processing. BatchId: {BatchId}, TransactionCount: {TransactionCount}.";

        /// <summary>
        /// Log emitted after loading pending transactions for the batch.
        /// </summary>
        public const string LoadedPendingTransactions = "Loaded pending transactions for batch. BatchId: {BatchId}, LoadedTransactions: {LoadedTransactions}.";

        /// <summary>
        /// Log emitted after transactions are aggregated by date.
        /// </summary>
        public const string AggregatedTransactions = "Aggregated batch transactions by date. BatchId: {BatchId}, AggregateCount: {AggregateCount}.";

        /// <summary>
        /// Log emitted when the daily balance is updated.
        /// </summary>
        public const string DailyBalanceUpdated = "Daily balance updated. BatchId: {BatchId}, AggregateCount: {AggregateCount}.";

        /// <summary>
        /// Log emitted when the batch is finalized successfully.
        /// </summary>
        public const string FinalizedSuccessfully = "Batch finalized successfully. BatchId: {BatchId}, ConsolidatedTransactions: {ConsolidatedTransactions}.";

        /// <summary>
        /// Log emitted when batch processing is redirected to manual review after retries or failures.
        /// </summary>
        public const string MovedToManualReview = "Batch processing moved to manual review. BatchId: {BatchId}, RetryCount: {RetryCount}, AffectedTransactions: {TransactionCount}.";

        /// <summary>
        /// Log emitted when a specific transaction is moved to manual review.
        /// </summary>
        public const string TransactionMovedToManualReview = "Transaction {TransactionId} from batch {BatchId} was moved to manual review. Reason: {Reason}.";

        /// <summary>
        /// Log emitted when no valid transactions remain after validation.
        /// </summary>
        public const string NoValidTransactionsAfterValidation = "Batch {BatchId} has no valid transactions after validation.";
    }

    /// <summary>
    /// Contains log messages related to the background worker and message consumption.
    /// </summary>
    public static class Worker
    {
        /// <summary>
        /// Log emitted when a Service Bus message is received.
        /// </summary>
        public const string MessageReceived = "Service Bus message received. BatchId: {BatchId}, MessageId: {MessageId}, TransactionCount: {TransactionCount}.";
    }

    /// <summary>
    /// Contains log messages related to resilience mechanisms such as retries.
    /// </summary>
    public static class Resilience
    {
        /// <summary>
        /// Log emitted when a retry attempt is executed.
        /// </summary>
        public const string RetryingOperation = "Retrying consolidation operation. Attempt: {Attempt}, DelaySeconds: {DelaySeconds}.";
    }
}
