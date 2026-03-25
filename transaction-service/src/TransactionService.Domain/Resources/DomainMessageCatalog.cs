namespace TransactionService.Domain.Resources;

/// <summary>
/// Centralizes domain-layer messages.
/// </summary>
public static class DomainMessageCatalog
{
    /// <summary>
    /// Message indicating that the idempotency key cannot be null or empty.
    /// </summary>
    public const string IdempotencyKeyCannotBeNullOrEmpty = "IdempotencyKey cannot be null or empty.";

    /// <summary>
    /// Message indicating that the request hash cannot be null or empty.
    /// </summary>
    public const string RequestHashCannotBeNullOrEmpty = "RequestHash cannot be null or empty.";

    /// <summary>
    /// Message indicating that the transaction identifier cannot be empty.
    /// </summary>
    public const string TransactionIdCannotBeEmpty = "TransactionId cannot be empty.";

    /// <summary>
    /// Message indicating that the idempotency entry has already been completed.
    /// </summary>
    public const string IdempotencyEntryAlreadyCompleted = "The idempotency entry has already been completed.";
}
