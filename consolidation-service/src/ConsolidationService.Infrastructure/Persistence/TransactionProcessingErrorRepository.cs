using ConsolidationService.Application.Abstractions;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Infrastructure.Data;
using Dapper;

namespace ConsolidationService.Infrastructure.Persistence;

/// <summary>
/// Provides PostgreSQL persistence operations for transaction processing errors
/// that require manual review.
/// </summary>
public sealed class TransactionProcessingErrorRepository : ITransactionProcessingErrorRepository
{
    private readonly NpgsqlConnectionFactory _connectionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionProcessingErrorRepository"/> class.
    /// </summary>
    /// <param name="connectionFactory">
    /// Factory used to create PostgreSQL connections for repository operations.
    /// </param>
    public TransactionProcessingErrorRepository(NpgsqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Inserts transaction or batch processing errors into the persistence store.
    /// </summary>
    /// <param name="items">
    /// The collection of processing errors to be persisted.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous insert operation.
    /// </returns>
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

        if (items.Count == 0)
        {
            return;
        }

        await using var connection = _connectionFactory.Create();

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                items,
                cancellationToken: cancellationToken));
    }
}
