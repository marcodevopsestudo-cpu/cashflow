namespace ConsolidationService.Application.Messages.Logs;

public static class BatchLogMessages
{
    public static class Workflow
    {
        public const string ProcessingStarted = "Starting consolidation batch processing. BatchId: {BatchId}, MessageId: {MessageId}, TransactionCount: {TransactionCount}";
        public const string ProcessingFinished = "Finished consolidation batch processing. BatchId: {BatchId}, MessageId: {MessageId}";
        public const string AlreadyProcessed = "Batch {BatchId} has already been processed or ignored.";
        public const string RegisteredForProcessing = "Batch registered for processing. BatchId: {BatchId}, TransactionCount: {TransactionCount}";
        public const string LoadedPendingTransactions = "Loaded pending transactions for batch. BatchId: {BatchId}, LoadedTransactions: {LoadedTransactions}";
        public const string AggregatedTransactions = "Aggregated batch transactions by date. BatchId: {BatchId}, AggregateCount: {AggregateCount}";
        public const string DailyBalanceUpdated = "Daily balance updated. BatchId: {BatchId}, AggregateCount: {AggregateCount}";
        public const string FinalizedSuccessfully = "Batch finalized successfully. BatchId: {BatchId}, ConsolidatedTransactions: {ConsolidatedTransactions}";
        public const string MovedToManualReview = "Batch processing moved to manual review. BatchId: {BatchId}, RetryCount: {RetryCount}, AffectedTransactions: {TransactionCount}";
    }

    public static class Worker
    {
        public const string MessageReceived = "Service Bus message received. BatchId: {BatchId}, MessageId: {MessageId}, TransactionCount: {TransactionCount}";
    }

    public static class Resilience
    {
        public const string RetryingOperation = "Retrying consolidation operation. Attempt: {Attempt}, DelaySeconds: {DelaySeconds}";
    }
}
