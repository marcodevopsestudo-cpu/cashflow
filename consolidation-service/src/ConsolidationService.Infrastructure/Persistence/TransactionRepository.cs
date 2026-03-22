using ConsolidationService.Application.Abstractions;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.Enums;
using ConsolidationService.Infrastructure.Data;
using Dapper;

namespace ConsolidationService.Infrastructure.Persistence;

/// <summary>
/// Implements transaction persistence operations against PostgreSQL.
/// </summary>
public sealed class TransactionRepository : ITransactionRepository
{
    private readonly NpgsqlConnectionFactory _connectionFactory;

    public TransactionRepository(NpgsqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyCollection<Transaction>> GetPendingByIdsAsync(IReadOnlyCollection<long> transactionIds, CancellationToken cancellationToken)
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

    public async Task MarkAsConsolidatedAsync(IReadOnlyCollection<long> transactionIds, Guid batchId, DateTime consolidatedAtUtc, CancellationToken cancellationToken)
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
        await connection.ExecuteAsync(new CommandDefinition(
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

    public async Task MarkAsFailedAsync(IReadOnlyCollection<long> transactionIds, Guid batchId, int attemptCount, TransactionProcessingStatus status, CancellationToken cancellationToken)
    {
        const string sql = """
            update transactions
               set processing_status = @Status,
                   last_batch_id = @BatchId,
                   processing_attempt_count = @AttemptCount
             where id = any(@TransactionIds);
            """;

        await using var connection = _connectionFactory.Create();
        await connection.ExecuteAsync(new CommandDefinition(
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
