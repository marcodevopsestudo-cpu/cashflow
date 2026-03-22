using Microsoft.Extensions.Logging;

namespace TransactionService.Application.Common.Diagnostics;

/// <summary>
/// Centralizes structured application logs.
/// </summary>
public static partial class TransactionLog
{

    [LoggerMessage(EventId = 900, Level = LogLevel.Information,
        Message = "Authorization accepted. CorrelationId={CorrelationId} CallerAppId={CallerAppId} CallerTenantId={CallerTenantId}")]
    public static partial void AuthorizationAccepted(this ILogger logger, string correlationId, string callerAppId, string callerTenantId);

    [LoggerMessage(EventId = 901, Level = LogLevel.Warning,
        Message = "Authorization rejected. CorrelationId={CorrelationId} CallerAppId={CallerAppId} Reason={Reason}")]
    public static partial void AuthorizationRejected(this ILogger logger, string correlationId, string? callerAppId, string reason);
    [LoggerMessage(EventId = 1000, Level = LogLevel.Information,
        Message = "CreateTransaction started. AccountId={AccountId} CorrelationId={CorrelationId} IdempotencyKey={IdempotencyKey}")]
    public static partial void CreateTransactionStarted(this ILogger logger, string accountId, string correlationId, string idempotencyKey);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information,
        Message = "CreateTransaction persisted. TransactionId={TransactionId} OutboxMessageId={OutboxMessageId} CorrelationId={CorrelationId}")]
    public static partial void CreateTransactionPersisted(this ILogger logger, Guid transactionId, Guid outboxMessageId, string correlationId);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information,
        Message = "CreateTransaction replayed. TransactionId={TransactionId} CorrelationId={CorrelationId} IdempotencyKey={IdempotencyKey}")]
    public static partial void CreateTransactionReplayed(this ILogger logger, Guid transactionId, string correlationId, string idempotencyKey);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Warning,
        Message = "CreateTransaction idempotency conflict. CorrelationId={CorrelationId} IdempotencyKey={IdempotencyKey}")]
    public static partial void CreateTransactionIdempotencyConflict(this ILogger logger, string correlationId, string idempotencyKey);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Warning,
        Message = "CreateTransaction already in progress. CorrelationId={CorrelationId} IdempotencyKey={IdempotencyKey}")]
    public static partial void CreateTransactionAlreadyInProgress(this ILogger logger, string correlationId, string idempotencyKey);

    [LoggerMessage(EventId = 1100, Level = LogLevel.Warning,
        Message = "GetTransactionById not found. TransactionId={TransactionId}")]
    public static partial void TransactionNotFound(this ILogger logger, Guid transactionId);

    [LoggerMessage(EventId = 1200, Level = LogLevel.Information,
        Message = "Outbox processing started. BatchSize={BatchSize}")]
    public static partial void OutboxProcessingStarted(this ILogger logger, int batchSize);

    [LoggerMessage(EventId = 1201, Level = LogLevel.Information,
        Message = "Outbox processing finished. ProcessedCount={ProcessedCount}")]
    public static partial void OutboxProcessingFinished(this ILogger logger, int processedCount);

    [LoggerMessage(EventId = 1202, Level = LogLevel.Information,
        Message = "Outbox message published. OutboxMessageId={OutboxMessageId} CorrelationId={CorrelationId}")]
    public static partial void OutboxMessagePublished(this ILogger logger, Guid outboxMessageId, string correlationId);

    [LoggerMessage(EventId = 1203, Level = LogLevel.Error,
        Message = "Outbox message publish failed. OutboxMessageId={OutboxMessageId} CorrelationId={CorrelationId}")]
    public static partial void OutboxMessagePublishFailed(this ILogger logger, Exception exception, Guid outboxMessageId, string correlationId);

    [LoggerMessage(EventId = 1300, Level = LogLevel.Information,
        Message = "CreateTransaction function completed. TransactionId={TransactionId} CorrelationId={CorrelationId} IdempotencyReplayed={IdempotencyReplayed}")]
    public static partial void CreateTransactionFunctionCompleted(this ILogger logger, Guid transactionId, string correlationId, bool idempotencyReplayed);
}
