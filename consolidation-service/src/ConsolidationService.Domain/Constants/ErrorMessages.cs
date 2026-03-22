namespace ConsolidationService.Domain.Constants;

public static class ErrorMessages
{
    public const string AlreadyProcessed = "Batch '{0}' has already been processed.";
    public const string NoPendingTransactions = "Batch '{0}' does not contain pending transactions.";
    public const string ProcessingFailed = "Failed to process batch '{0}'.";
    public const string InvalidServiceBusMessage = "Service Bus message body could not be deserialized.";
}
