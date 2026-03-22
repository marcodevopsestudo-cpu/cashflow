using ConsolidationService.Application.Abstractions;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.Enums;
using ConsolidationService.Infrastructure.Data;
using Dapper;

namespace ConsolidationService.Infrastructure.Persistence;

/// <summary>
/// Provides PostgreSQL persistence operations for managing the lifecycle of consolidation batches.
/// </summary>
public sealed class DailyBatchRepository : IDailyBatchRepository
{
    private readonly NpgsqlConnectionFactory _connectionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DailyBatchRepository"/> class.
    /// </summary>
    /// <param name="connectionFactory">
    /// Factory used to create PostgreSQL connections for repository operations.
    /// </param>
    public DailyBatchRepository(NpgsqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Retrieves a batch by its identifier.
    /// </summary>
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

        return await connection.QuerySingleOrDefaultAsync<DailyBatch>(
            new CommandDefinition(sql, new { BatchId = batchId }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Inserts a new batch in pending state or updates an existing one.
    /// </summary>
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

        var createdAtUtc = DateTime.UtcNow;

        await using var connection = _connectionFactory.Create();

        return await connection.QuerySingleAsync<DailyBatch>(
            new CommandDefinition(
                sql,
                new
                {
                    BatchId = batchId,
                    CorrelationId = correlationId,
                    TransactionCount = transactionCount,
                    Status = (int)BatchStatus.Pending,
                    CreatedAtUtc = createdAtUtc
                },
                cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Marks the batch as processing.
    /// </summary>
    public async Task MarkAsProcessingAsync(Guid batchId, CancellationToken cancellationToken)
    {
        const string sql = """
            update daily_batch
               set status = @Status,
                   started_at_utc = coalesce(started_at_utc, @StartedAtUtc)
             where batch_id = @BatchId;
            """;

        var startedAtUtc = DateTime.UtcNow;

        await using var connection = _connectionFactory.Create();

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    BatchId = batchId,
                    Status = (int)BatchStatus.Processing,
                    StartedAtUtc = startedAtUtc
                },
                cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Marks the batch as successfully processed.
    /// </summary>
    public async Task MarkAsSucceededAsync(Guid batchId, CancellationToken cancellationToken)
    {
        const string sql = """
            update daily_batch
               set status = @Status,
                   completed_at_utc = @CompletedAtUtc,
                   last_error = null
             where batch_id = @BatchId;
            """;

        var completedAtUtc = DateTime.UtcNow;

        await using var connection = _connectionFactory.Create();

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    BatchId = batchId,
                    Status = (int)BatchStatus.Succeeded,
                    CompletedAtUtc = completedAtUtc
                },
                cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Marks the batch as failed and updates retry information.
    /// </summary>
    public async Task MarkAsFailedAsync(Guid batchId, string errorMessage, int retryCount, CancellationToken cancellationToken)
    {
        const string sql = """
            update daily_batch
               set status = @Status,
                   retry_count = @RetryCount,
                   completed_at_utc = @CompletedAtUtc,
                   last_error = @ErrorMessage
             where batch_id = @BatchId;
            """;

        var completedAtUtc = DateTime.UtcNow;

        await using var connection = _connectionFactory.Create();

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    BatchId = batchId,
                    Status = (int)BatchStatus.Failed,
                    RetryCount = retryCount,
                    CompletedAtUtc = completedAtUtc,
                    ErrorMessage = errorMessage
                },
                cancellationToken: cancellationToken));
    }
}
