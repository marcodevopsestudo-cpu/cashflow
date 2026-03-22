using ConsolidationService.Application.Abstractions;
using ConsolidationService.Domain.ValueObjects;
using ConsolidationService.Infrastructure.Data;
using Dapper;

namespace ConsolidationService.Infrastructure.Persistence;

/// <summary>
/// Implements daily balance persistence using PostgreSQL upsert operations.
/// </summary>
public sealed class DailyBalanceRepository : IDailyBalanceRepository
{
    private readonly NpgsqlConnectionFactory _connectionFactory;

    public DailyBalanceRepository(NpgsqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

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

        var rows = aggregates.Select(x => new
        {
            x.BalanceDate,
            x.TotalCredits,
            x.TotalDebits,
            x.Balance,
            UpdatedAtUtc = DateTime.UtcNow
        }).ToArray();

        await using var connection = _connectionFactory.Create();
        await connection.ExecuteAsync(new CommandDefinition(sql, rows, cancellationToken: cancellationToken));
    }
}
