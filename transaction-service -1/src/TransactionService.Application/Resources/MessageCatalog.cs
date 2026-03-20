namespace TransactionService.Application.Resources;

/// <summary>
/// Centralizes reusable messages.
/// </summary>
public static class MessageCatalog
{
    public const string InvalidPayload = "The request payload is invalid.";
    public const string AccountIdRequired = "AccountId is required.";
    public const string CurrencyRequired = "Currency is required.";
    public const string KindRequired = "Kind is required.";
    public const string AmountMustBeGreaterThanZero = "Amount must be greater than zero.";
    public const string CorrelationIdRequired = "CorrelationId is required.";
    public const string IdempotencyKeyRequired = "Idempotency-Key is required.";
    public const string TransactionNotFound = "The transaction '{0}' was not found.";
    public const string PublishFailed = "The outbox message could not be published.";
    public const string UnsupportedKind = "The transaction kind '{0}' is not supported.";
    public const string OutboxProcessingStarted = "Starting outbox processing for up to {0} message(s).";
    public const string OutboxProcessingFinished = "Outbox processing finished. {0} message(s) processed.";
    public const string ServiceBusTopicRequired = "ServiceBus:TopicName is required.";
    public const string PageSizeMustBeGreaterThanZero = "Outbox page size must be greater than zero.";
}
