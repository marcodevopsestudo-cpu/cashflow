using ConsolidationService.Application.Abstractions;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Infrastructure.Data;
using Dapper;

namespace ConsolidationService.Infrastructure.Persistence;

/// <summary>
/// Implements batch lifecycle persistence operations.
/// </summary>
public sealed class DailyBatchRepository : IDailyBatchRepository
{
    private readonly NpgsqlConnectionFactory _connectionFactory;

    public DailyBatchRepository(NpgsqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<DailyBatch?> GetAsync(Guid batchId, CancellationToken cancellationToken)
    {
        const string sql = """
            select
                batch_id as BatchId,
                correlation_id as CorrelationId,
                status as Status,
                transaction_count as TransactionCount,
                retry_count as RetryCount,
                last_error as LastError,
                created_at_utc as CreatedAtUtc,
                started_at_utc as StartedAtUtc,
                completed_at_utc as CompletedAtUtc
            from daily_batch
            where batch_id = @BatchId;
            """;

        await using var connection = _connectionFactory.Create();
        return await connection.QuerySingleOrDefaultAsync<DailyBatch>(new CommandDefinition(sql, new { BatchId = batchId }, cancellationToken: cancellationToken));
    }

    public async Task<DailyBatch> UpsertPendingAsync(Guid batchId, string correlationId, int transactionCount, CancellationToken cancellationToken)
    {
        const string sql = """
            insert into daily_batch (batch_id, correlation_id, status, transaction_count, retry_count, created_at_utc)
            values (@BatchId, @CorrelationId, @Status, @TransactionCount, 0, @CreatedAtUtc)
            on conflict (batch_id)
            do update set correlation_id = excluded.correlation_id,
                          transaction_count = excluded.transaction_count
            returning
                batch_id as BatchId,
                correlation_id as CorrelationId,
                status as Status,
                transaction_count as TransactionCount,
                retry_count as RetryCount,
                last_error as LastError,
                created_at_utc as CreatedAtUtc,
                started_at_utc as StartedAtUtc,
                completed_at_utc as CompletedAtUtc;
            """;

        await using var connection = _connectionFactory.Create();
        return await connection.QuerySingleAsync<DailyBatch>(new CommandDefinition(
            sql,
            new { BatchId = batchId, CorrelationId = correlationId, TransactionCount = transactionCount, Status = 1, CreatedAtUtc = DateTime.UtcNow },
            cancellationToken: cancellationToken));
    }

    public async Task MarkAsProcessingAsync(Guid batchId, CancellationToken cancellationToken)
    {
        const string sql = """
            update daily_batch
               set status = 2,
                   started_at_utc = coalesce(started_at_utc, @StartedAtUtc)
             where batch_id = @BatchId;
            """;

        await using var connection = _connectionFactory.Create();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { BatchId = batchId, StartedAtUtc = DateTime.UtcNow }, cancellationToken: cancellationToken));
    }

    public async Task MarkAsSucceededAsync(Guid batchId, CancellationToken cancellationToken)
    {
        const string sql = """
            update daily_batch
               set status = 3,
                   completed_at_utc = @CompletedAtUtc,
                   last_error = null
             where batch_id = @BatchId;
            """;

        await using var connection = _connectionFactory.Create();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { BatchId = batchId, CompletedAtUtc = DateTime.UtcNow }, cancellationToken: cancellationToken));
    }

    public async Task MarkAsFailedAsync(Guid batchId, string errorMessage, int retryCount, CancellationToken cancellationToken)
    {
        const string sql = """
            update daily_batch
               set status = 4,
                   retry_count = @RetryCount,
                   completed_at_utc = @CompletedAtUtc,
                   last_error = @ErrorMessage
             where batch_id = @BatchId;
            """;

        await using var connection = _connectionFactory.Create();
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { BatchId = batchId, RetryCount = retryCount, CompletedAtUtc = DateTime.UtcNow, ErrorMessage = errorMessage },
            cancellationToken: cancellationToken));
    }
}
