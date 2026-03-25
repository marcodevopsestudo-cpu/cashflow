using ConsolidationService.Application.Abstractions;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.Enums;
using ConsolidationService.Infrastructure.Data;
using Dapper;

namespace ConsolidationService.Infrastructure.Persistence;

/// <summary>
/// Provides PostgreSQL persistence operations for transaction retrieval and state transitions.
/// </summary>
public sealed class TransactionRepository : ITransactionRepository
{
    private readonly NpgsqlConnectionFactory _connectionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionRepository"/> class.
    /// </summary>
    /// <param name="connectionFactory">
    /// Factory used to create PostgreSQL connections for repository operations.
    /// </param>
    public TransactionRepository(NpgsqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Retrieves pending transactions filtered by the specified identifiers.
    /// </summary>
    /// <param name="transactionIds">
    /// The collection of transaction identifiers to retrieve.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A read-only collection containing the pending transactions found for the specified identifiers.
    /// </returns>
    public async Task<IReadOnlyCollection<Transaction>> GetPendingByIdsAsync(
        IReadOnlyCollection<Guid> transactionIds,
        CancellationToken cancellationToken)
    {
        const string sql = """
            select
                id,
                amount,
                type as Type,
                occurred_at_utc as OccurredAtUtc,
                processing_status as ProcessingStatus,
                last_batch_id as LastBatchId,
                consolidated_at_utc as ConsolidatedAtUtc,
                processing_attempt_count as ProcessingAttemptCount
            from transactions
            where id = any(@TransactionIds)
              and processing_status = @PendingStatus;
            """;

        await using var connection = _connectionFactory.Create();

        var command = new CommandDefinition(
            sql,
            new
            {
                TransactionIds = transactionIds.ToArray(),
                PendingStatus = (int)TransactionProcessingStatus.Pending
            },
            cancellationToken: cancellationToken);

        var result = await connection.QueryAsync<Transaction>(command);

        return result.ToArray();
    }

    /// <summary>
    /// Marks the specified transactions as successfully consolidated.
    /// </summary>
    /// <param name="transactionIds">
    /// The collection of transaction identifiers to update.
    /// </param>
    /// <param name="batchId">
    /// The identifier of the batch responsible for the consolidation.
    /// </param>
    /// <param name="consolidatedAtUtc">
    /// The UTC timestamp indicating when the transactions were consolidated.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous update operation.
    /// </returns>
    public async Task MarkAsConsolidatedAsync(
        IReadOnlyCollection<Guid> transactionIds,
        Guid batchId,
        DateTime consolidatedAtUtc,
        CancellationToken cancellationToken)
    {
        const string sql = """
            update transactions
               set processing_status = @Status,
                   last_batch_id = @BatchId,
                   consolidated_at_utc = @ConsolidatedAtUtc,
                   processing_attempt_count = processing_attempt_count + 1
             where id = any(@TransactionIds);
            """;

        await using var connection = _connectionFactory.Create();

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    TransactionIds = transactionIds.ToArray(),
                    BatchId = batchId,
                    ConsolidatedAtUtc = consolidatedAtUtc,
                    Status = (int)TransactionProcessingStatus.Consolidated
                },
                cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Marks the specified transactions as failed during processing.
    /// </summary>
    /// <param name="transactionIds">
    /// The collection of transaction identifiers to update.
    /// </param>
    /// <param name="batchId">
    /// The identifier of the batch associated with the failed processing attempt.
    /// </param>
    /// <param name="attemptCount">
    /// The number of processing attempts recorded for the transactions.
    /// </param>
    /// <param name="status">
    /// The resulting processing status to assign to the transactions.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous update operation.
    /// </returns>
    public async Task MarkAsFailedAsync(
        IReadOnlyCollection<Guid> transactionIds,
        Guid batchId,
        int attemptCount,
        TransactionProcessingStatus status,
        CancellationToken cancellationToken)
    {
        const string sql = """
            update transactions
               set processing_status = @Status,
                   last_batch_id = @BatchId,
                   processing_attempt_count = @AttemptCount
             where id = any(@TransactionIds);
            """;

        await using var connection = _connectionFactory.Create();

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    TransactionIds = transactionIds.ToArray(),
                    Status = (int)status,
                    BatchId = batchId,
                    AttemptCount = attemptCount
                },
                cancellationToken: cancellationToken));
    }
}
