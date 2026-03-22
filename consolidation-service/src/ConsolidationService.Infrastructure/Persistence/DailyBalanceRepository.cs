using ConsolidationService.Application.Abstractions;
using ConsolidationService.Domain.ValueObjects;
using ConsolidationService.Infrastructure.Data;
using Dapper;

namespace ConsolidationService.Infrastructure.Persistence;

/// <summary>
/// Provides PostgreSQL persistence operations for daily balance aggregates using upsert semantics.
/// </summary>
public sealed class DailyBalanceRepository : IDailyBalanceRepository
{
    private readonly NpgsqlConnectionFactory _connectionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DailyBalanceRepository"/> class.
    /// </summary>
    /// <param name="connectionFactory">
    /// Factory used to create PostgreSQL connections for repository operations.
    /// </param>
    public DailyBalanceRepository(NpgsqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Inserts or updates daily balance rows based on the provided aggregates.
    /// </summary>
    /// <param name="aggregates">
    /// The collection of daily aggregates to be persisted.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous upsert operation.
    /// </returns>
    /// <remarks>
    /// When a balance date already exists, the stored totals are incremented using the incoming values,
    /// and the resulting balance is recalculated in the database.
    /// </remarks>
    public async Task UpsertAsync(IReadOnlyCollection<DailyAggregate> aggregates, CancellationToken cancellationToken)
    {
        const string sql = """
            insert into daily_balance (balance_date, total_credits, total_debits, balance, updated_at_utc)
            values (@BalanceDate, @TotalCredits, @TotalDebits, @Balance, @UpdatedAtUtc)
            on conflict (balance_date)
            do update set
                total_credits = daily_balance.total_credits + excluded.total_credits,
                total_debits = daily_balance.total_debits + excluded.total_debits,
                balance = (daily_balance.total_credits + excluded.total_credits) - (daily_balance.total_debits + excluded.total_debits),
                updated_at_utc = excluded.updated_at_utc;
            """;

        var updatedAtUtc = DateTime.UtcNow;

        var rows = aggregates.Select(aggregate => new
        {
            aggregate.BalanceDate,
            aggregate.TotalCredits,
            aggregate.TotalDebits,
            aggregate.Balance,
            UpdatedAtUtc = updatedAtUtc
        }).ToArray();

        await using var connection = _connectionFactory.Create();

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                rows,
                cancellationToken: cancellationToken));
    }
}
