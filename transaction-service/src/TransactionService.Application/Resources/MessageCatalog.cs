namespace TransactionService.Application.Resources;

/// <summary>
/// Centralizes reusable messages.
/// </summary>
public static class MessageCatalog
{
    /// <summary>
    /// Message indicating that the request payload is invalid.
    /// </summary>
    public const string InvalidPayload = "The request payload is invalid.";

    /// <summary>
    /// Message indicating that the account identifier is required.
    /// </summary>
    public const string AccountIdRequired = "AccountId is required.";

    /// <summary>
    /// Message indicating that the currency is required.
    /// </summary>
    public const string CurrencyRequired = "Currency is required.";

    /// <summary>
    /// Message indicating that the transaction kind is required.
    /// </summary>
    public const string KindRequired = "Kind is required.";

    /// <summary>
    /// Message indicating that the amount must be greater than zero.
    /// </summary>
    public const string AmountMustBeGreaterThanZero = "Amount must be greater than zero.";

    /// <summary>
    /// Message indicating that the correlation identifier is required.
    /// </summary>
    public const string CorrelationIdRequired = "CorrelationId is required.";

    /// <summary>
    /// Message indicating that the idempotency key is required.
    /// </summary>
    public const string IdempotencyKeyRequired = "Idempotency-Key is required.";

    /// <summary>
    /// Message indicating that a transaction was not found.
    /// </summary>
    public const string TransactionNotFound = "The transaction '{0}' was not found.";

    /// <summary>
    /// Message indicating that publishing the outbox message failed.
    /// </summary>
    public const string PublishFailed = "The outbox message could not be published.";

    /// <summary>
    /// Message indicating that the transaction kind is not supported.
    /// </summary>
    public const string UnsupportedKind = "The transaction kind '{0}' is not supported.";

    /// <summary>
    /// Message indicating that outbox processing has started.
    /// </summary>
    public const string OutboxProcessingStarted = "Starting outbox processing for up to {0} message(s).";

    /// <summary>
    /// Message indicating that outbox processing has finished.
    /// </summary>
    public const string OutboxProcessingFinished = "Outbox processing finished. {0} message(s) processed.";

    /// <summary>
    /// Message indicating that the Service Bus topic name configuration is required.
    /// </summary>
    public const string ServiceBusTopicRequired = "ServiceBus:TopicName is required.";

    /// <summary>
    /// Message indicating that the outbox page size must be greater than zero.
    /// </summary>
    public const string PageSizeMustBeGreaterThanZero = "Outbox page size must be greater than zero.";

    /// <summary>
    /// Message indicating that a request with the same idempotency key is already in progress.
    /// </summary>
    public const string RequestWithSameIdempotencyKeyAlreadyInProgress = "A request with the same Idempotency-Key is already being processed.";

    /// <summary>
    /// Message indicating that the idempotency key was already used with a different payload.
    /// </summary>
    public const string IdempotencyKeyAlreadyUsedWithDifferentPayload = "The provided Idempotency-Key was already used with a different payload.";

    /// <summary>
    /// Contains log message templates used across the application layer.
    /// </summary>
    public static class Logs
    {
        /// <summary>
        /// Log message indicating a validation failure.
        /// </summary>
        public const string ValidationFailure = "Validation failure for {RequestName}: {Message}";

        /// <summary>
        /// Log message indicating the start of a transaction retrieval.
        /// </summary>
        public const string GetTransactionByIdStarted = "Getting transaction by id. TransactionId={TransactionId}";

        /// <summary>
        /// Log message indicating that a transaction was not found.
        /// </summary>
        public const string GetTransactionByIdNotFound = "Transaction not found. TransactionId={TransactionId}";

        /// <summary>
        /// Log message indicating that a transaction was successfully retrieved.
        /// </summary>
        public const string GetTransactionByIdSucceeded = "Transaction retrieved successfully. TransactionId={TransactionId}, AccountId={AccountId}, Amount={Amount}, Currency={Currency}, Kind={Kind}";

        /// <summary>
        /// Log message indicating that the operation was canceled.
        /// </summary>
        public const string GetTransactionByIdCanceled = "GetTransactionById was canceled. TransactionId={TransactionId}";

        /// <summary>
        /// Log message indicating an unexpected error during retrieval.
        /// </summary>
        public const string GetTransactionByIdUnexpectedError = "Unexpected error while retrieving transaction. TransactionId={TransactionId}";

        /// <summary>
        /// Log message indicating an Npgsql exception during retrieval.
        /// </summary>
        public const string GetTransactionByIdNpgsqlError = "Npgsql exception while retrieving transaction. Message={Message}";

        /// <summary>
        /// Log message indicating a PostgreSQL exception during retrieval.
        /// </summary>
        public const string GetTransactionByIdPostgresError = "Postgres exception while retrieving transaction. SqlState={SqlState}, Detail={Detail}, ConstraintName={ConstraintName}, TableName={TableName}, ColumnName={ColumnName}";

        /// <summary>
        /// Log message indicating the start of outbox processing.
        /// </summary>
        public const string ProcessOutboxStarted = "Starting outbox processing. CorrelationId={CorrelationId}, BatchSize={BatchSize}";

        /// <summary>
        /// Log message indicating that outbox processing was canceled.
        /// </summary>
        public const string ProcessOutboxCanceled = "Outbox processing was canceled. CorrelationId={CorrelationId}, BatchSize={BatchSize}";

        /// <summary>
        /// Log message indicating an unexpected error during outbox processing.
        /// </summary>
        public const string ProcessOutboxUnexpectedError = "Unexpected error while processing outbox messages. CorrelationId={CorrelationId}, BatchSize={BatchSize}";

        /// <summary>
        /// Log message indicating an Npgsql exception during outbox processing.
        /// </summary>
        public const string ProcessOutboxNpgsqlError = "Npgsql exception while processing outbox messages. CorrelationId={CorrelationId}, Message={Message}";

        /// <summary>
        /// Log message indicating an inner Npgsql exception during outbox processing.
        /// </summary>
        public const string ProcessOutboxInnerNpgsqlError = "Inner Npgsql exception while processing outbox messages. CorrelationId={CorrelationId}, Message={Message}";

        /// <summary>
        /// Log message indicating a PostgreSQL exception during outbox processing.
        /// </summary>
        public const string ProcessOutboxPostgresError = "Postgres exception while processing outbox messages. CorrelationId={CorrelationId}, SqlState={SqlState}, Detail={Detail}, ConstraintName={ConstraintName}, TableName={TableName}, ColumnName={ColumnName}";

        /// <summary>
        /// Log message indicating an inner PostgreSQL exception during outbox processing.
        /// </summary>
        public const string ProcessOutboxInnerPostgresError = "Inner Postgres exception while processing outbox messages. CorrelationId={CorrelationId}, SqlState={SqlState}, Detail={Detail}, ConstraintName={ConstraintName}, TableName={TableName}, ColumnName={ColumnName}";

        /// <summary>
        /// Log message indicating that a concurrency conflict was detected during transaction creation.
        /// </summary>
        public const string CreateTransactionConcurrencyDetected = "CreateTransaction concurrency detected. CorrelationId={CorrelationId}, IdempotencyKey={IdempotencyKey}";

        /// <summary>
        /// Log message indicating a concurrency anomaly during transaction creation.
        /// </summary>
        public const string CreateTransactionConcurrencyAnomaly = "CreateTransaction concurrency anomaly. Unique violation occurred but the idempotency entry was not found afterwards. CorrelationId={CorrelationId}, IdempotencyKey={IdempotencyKey}";

        /// <summary>
        /// Log message indicating a persistence failure during transaction creation.
        /// </summary>
        public const string CreateTransactionPersistenceFailure = "CreateTransaction persistence failure. CorrelationId={CorrelationId}, IdempotencyKey={IdempotencyKey}, TransactionId={TransactionId}, OutboxMessageId={OutboxMessageId}";

        /// <summary>
        /// Log message indicating a PostgreSQL failure during transaction creation.
        /// </summary>
        public const string CreateTransactionPostgresFailure = "CreateTransaction postgres failure. SqlState={SqlState}, ConstraintName={ConstraintName}, TableName={TableName}, ColumnName={ColumnName}";
    }
}
