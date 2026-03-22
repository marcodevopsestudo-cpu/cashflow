using ConsolidationService.Application.Abstractions;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Infrastructure.Data;
using Dapper;

namespace ConsolidationService.Infrastructure.Persistence;

/// <summary>
/// Persists manual-review errors for batches or transactions.
/// </summary>
public sealed class TransactionProcessingErrorRepository : ITransactionProcessingErrorRepository
{
    private readonly NpgsqlConnectionFactory _connectionFactory;

    public TransactionProcessingErrorRepository(NpgsqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InsertAsync(IReadOnlyCollection<TransactionProcessingError> items, CancellationToken cancellationToken)
    {
        const string sql = """
            insert into transaction_processing_error
            (
                batch_id,
                transaction_id,
                correlation_id,
                error_code,
                error_message,
                stack_trace,
                created_at_utc,
                retry_count,
                status
            )
            values
            (
                @BatchId,
                @TransactionId,
                @CorrelationId,
                @ErrorCode,
                @ErrorMessage,
                @StackTrace,
                @CreatedAtUtc,
                @RetryCount,
                @Status
            );
            """;

        await using var connection = _connectionFactory.Create();
        await connection.ExecuteAsync(new CommandDefinition(sql, items.ToArray(), cancellationToken: cancellationToken));
    }
}
