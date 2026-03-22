namespace TransactionService.Domain.Resources;

/// <summary>
/// Centralizes domain-layer messages.
/// </summary>
public static class DomainMessageCatalog
{
    public const string IdempotencyKeyCannotBeNullOrEmpty = "IdempotencyKey cannot be null or empty.";
    public const string RequestHashCannotBeNullOrEmpty = "RequestHash cannot be null or empty.";
    public const string TransactionIdCannotBeEmpty = "TransactionId cannot be empty.";
    public const string IdempotencyEntryAlreadyCompleted = "The idempotency entry has already been completed.";
}
