namespace ConsolidationService.Domain.Constants;

/// <summary>
/// Contains error message templates used across the domain layer.
/// </summary>
public static class ErrorMessages
{
    /// <summary>
    /// Contains error messages related to batch processing.
    /// </summary>
    public static class Batch
    {
        /// <summary>
        /// Error message when a batch has already been processed.
        /// </summary>
        public const string AlreadyProcessed = "Batch '{0}' has already been processed.";

        /// <summary>
        /// Error message when a batch does not contain pending transactions.
        /// </summary>
        public const string NoPendingTransactions = "Batch '{0}' does not contain pending transactions.";

        /// <summary>
        /// Error message when batch processing fails unexpectedly.
        /// </summary>
        public const string ProcessingFailed = "Failed to process batch '{0}'.";
    }

    /// <summary>
    /// Contains error messages related to background worker operations.
    /// </summary>
    public static class Worker
    {
        /// <summary>
        /// Error message when a Service Bus message cannot be deserialized.
        /// </summary>
        public const string InvalidServiceBusMessage = "Service Bus message body could not be deserialized.";
    }
}
