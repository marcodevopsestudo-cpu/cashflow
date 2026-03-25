using Microsoft.Extensions.Logging;

namespace TransactionService.Application.Common.Diagnostics;

/// <summary>
/// Centralizes structured application logs.
/// </summary>
public static partial class TransactionLog
{
    /// <summary>
    /// Logs that the authorization was successfully accepted.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="correlationId">The correlation identifier for the request.</param>
    /// <param name="callerAppId">The caller application identifier.</param>
    /// <param name="callerTenantId">The caller tenant identifier.</param>
    [LoggerMessage(EventId = 900, Level = LogLevel.Information,
        Message = "Authorization accepted. CorrelationId={CorrelationId} CallerAppId={CallerAppId} CallerTenantId={CallerTenantId}")]
    public static partial void AuthorizationAccepted(this ILogger logger, string correlationId, string callerAppId, string callerTenantId);

    /// <summary>
    /// Logs that the authorization was rejected.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="correlationId">The correlation identifier for the request.</param>
    /// <param name="callerAppId">The caller application identifier, if available.</param>
    /// <param name="reason">The reason for rejection.</param>
    [LoggerMessage(EventId = 901, Level = LogLevel.Warning,
        Message = "Authorization rejected. CorrelationId={CorrelationId} CallerAppId={CallerAppId} Reason={Reason}")]
    public static partial void AuthorizationRejected(this ILogger logger, string correlationId, string? callerAppId, string reason);

    /// <summary>
    /// Logs the start of a transaction creation process.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="correlationId">The correlation identifier for the request.</param>
    /// <param name="idempotencyKey">The idempotency key associated with the request.</param>
    [LoggerMessage(EventId = 1000, Level = LogLevel.Information,
        Message = "CreateTransaction started. AccountId={AccountId} CorrelationId={CorrelationId} IdempotencyKey={IdempotencyKey}")]
    public static partial void CreateTransactionStarted(this ILogger logger, string accountId, string correlationId, string idempotencyKey);

    /// <summary>
    /// Logs that the transaction was successfully persisted along with its outbox message.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="outboxMessageId">The outbox message identifier.</param>
    /// <param name="correlationId">The correlation identifier for the request.</param>
    [LoggerMessage(EventId = 1001, Level = LogLevel.Information,
        Message = "CreateTransaction persisted. TransactionId={TransactionId} OutboxMessageId={OutboxMessageId} CorrelationId={CorrelationId}")]
    public static partial void CreateTransactionPersisted(this ILogger logger, Guid transactionId, Guid outboxMessageId, string correlationId);

    /// <summary>
    /// Logs that a transaction creation request was replayed due to idempotency.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="correlationId">The correlation identifier for the request.</param>
    /// <param name="idempotencyKey">The idempotency key associated with the request.</param>
    [LoggerMessage(EventId = 1002, Level = LogLevel.Information,
        Message = "CreateTransaction replayed. TransactionId={TransactionId} CorrelationId={CorrelationId} IdempotencyKey={IdempotencyKey}")]
    public static partial void CreateTransactionReplayed(this ILogger logger, Guid transactionId, string correlationId, string idempotencyKey);

    /// <summary>
    /// Logs an idempotency conflict during transaction creation.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="correlationId">The correlation identifier for the request.</param>
    /// <param name="idempotencyKey">The idempotency key associated with the request.</param>
    [LoggerMessage(EventId = 1003, Level = LogLevel.Warning,
        Message = "CreateTransaction idempotency conflict. CorrelationId={CorrelationId} IdempotencyKey={IdempotencyKey}")]
    public static partial void CreateTransactionIdempotencyConflict(this ILogger logger, string correlationId, string idempotencyKey);

    /// <summary>
    /// Logs that a transaction creation request is already in progress for a given idempotency key.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="correlationId">The correlation identifier for the request.</param>
    /// <param name="idempotencyKey">The idempotency key associated with the request.</param>
    [LoggerMessage(EventId = 1004, Level = LogLevel.Warning,
        Message = "CreateTransaction already in progress. CorrelationId={CorrelationId} IdempotencyKey={IdempotencyKey}")]
    public static partial void CreateTransactionAlreadyInProgress(this ILogger logger, string correlationId, string idempotencyKey);

    /// <summary>
    /// Logs that a transaction was not found when queried by its identifier.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="transactionId">The transaction identifier.</param>
    [LoggerMessage(EventId = 1100, Level = LogLevel.Warning,
        Message = "GetTransactionById not found. TransactionId={TransactionId}")]
    public static partial void TransactionNotFound(this ILogger logger, Guid transactionId);

    /// <summary>
    /// Logs the start of the outbox processing routine.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="batchSize">The number of messages to process in the batch.</param>
    [LoggerMessage(EventId = 1200, Level = LogLevel.Information,
        Message = "Outbox processing started. BatchSize={BatchSize}")]
    public static partial void OutboxProcessingStarted(this ILogger logger, int batchSize);

    /// <summary>
    /// Logs the completion of the outbox processing routine.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="processedCount">The number of processed messages.</param>
    [LoggerMessage(EventId = 1201, Level = LogLevel.Information,
        Message = "Outbox processing finished. ProcessedCount={ProcessedCount}")]
    public static partial void OutboxProcessingFinished(this ILogger logger, int processedCount);

    /// <summary>
    /// Logs that an outbox message was successfully published.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="outboxMessageId">The outbox message identifier.</param>
    /// <param name="correlationId">The correlation identifier for the request.</param>
    [LoggerMessage(EventId = 1202, Level = LogLevel.Information,
        Message = "Outbox message published. OutboxMessageId={OutboxMessageId} CorrelationId={CorrelationId}")]
    public static partial void OutboxMessagePublished(this ILogger logger, Guid outboxMessageId, string correlationId);

    /// <summary>
    /// Logs that publishing an outbox message has failed.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception thrown during publishing.</param>
    /// <param name="outboxMessageId">The outbox message identifier.</param>
    /// <param name="correlationId">The correlation identifier for the request.</param>
    [LoggerMessage(EventId = 1203, Level = LogLevel.Error,
        Message = "Outbox message publish failed. OutboxMessageId={OutboxMessageId} CorrelationId={CorrelationId}")]
    public static partial void OutboxMessagePublishFailed(this ILogger logger, Exception exception, Guid outboxMessageId, string correlationId);

    /// <summary>
    /// Logs the completion of the CreateTransaction function execution.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="correlationId">The correlation identifier for the request.</param>
    /// <param name="idempotencyReplayed">Indicates whether the response was replayed due to idempotency.</param>
    [LoggerMessage(EventId = 1300, Level = LogLevel.Information,
        Message = "CreateTransaction function completed. TransactionId={TransactionId} CorrelationId={CorrelationId} IdempotencyReplayed={IdempotencyReplayed}")]
    public static partial void CreateTransactionFunctionCompleted(this ILogger logger, Guid transactionId, string correlationId, bool idempotencyReplayed);
}
