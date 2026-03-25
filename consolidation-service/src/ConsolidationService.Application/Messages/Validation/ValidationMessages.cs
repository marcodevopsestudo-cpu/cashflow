namespace ConsolidationService.Application.Messages.Validation;

/// <summary>
/// Contains validation message templates used across the application layer.
/// </summary>
public static class ValidationMessages
{
    /// <summary>
    /// Contains validation messages related to command inputs.
    /// </summary>
    public static class Command
    {
        /// <summary>
        /// Validation message when BatchId is missing.
        /// </summary>
        public const string BatchIdRequired = "BatchId is required.";

        /// <summary>
        /// Validation message when CorrelationId is missing.
        /// </summary>
        public const string CorrelationIdRequired = "CorrelationId is required.";

        /// <summary>
        /// Validation message when CorrelationId exceeds the maximum allowed length.
        /// </summary>
        public const string CorrelationIdTooLong = "CorrelationId must not exceed 64 characters.";

        /// <summary>
        /// Validation message when no transaction identifiers are provided.
        /// </summary>
        public const string TransactionIdsRequired = "At least one transaction identifier is required.";

        /// <summary>
        /// Validation message when a transaction identifier is empty.
        /// </summary>
        public const string TransactionIdRequired = "TransactionId must not be empty.";

        /// <summary>
        /// Validation message when MessageId is missing.
        /// </summary>
        public const string MessageIdRequired = "MessageId is required.";
    }

    /// <summary>
    /// Contains validation messages related to transaction domain rules.
    /// </summary>
    public static class Transaction
    {
        /// <summary>
        /// Validation message when transaction amount is less than or equal to zero.
        /// </summary>
        public const string AmountMustBeGreaterThanZero = "Transaction amount must be greater than zero.";

        /// <summary>
        /// Validation message when the transaction balance date is invalid.
        /// </summary>
        public const string BalanceDateInvalid = "Transaction balance date is invalid.";
    }
}
