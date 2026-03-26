using ConsolidationService.Application.Abstractions;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using TransactionService.Infrastructure.Persistence;

namespace ConsolidationService.Infrastructure.Persistence;

/// <summary>
/// Provides PostgreSQL persistence operations for daily balance aggregates using upsert semantics.
/// </summary>
public sealed class DailyBalanceRepository : IDailyBalanceRepository
{
    private readonly TransactionDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="DailyBalanceRepository"/> class.
    /// </summary>
    /// <param name="dbContext">
    /// DbContext used for repository operations.
    /// </param>
    public DailyBalanceRepository(TransactionDbContext dbContext)
    {
        _dbContext = dbContext;
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
    /// and the resulting balance is recalculated in memory before persisting the changes.
    /// </remarks>
    public async Task UpsertAsync(IReadOnlyCollection<DailyAggregate> aggregates, CancellationToken cancellationToken)
    {
        if (aggregates.Count == 0)
        {
            return;
        }

        var updatedAtUtc = DateTime.UtcNow;
        var balanceDates = aggregates.Select(x => x.BalanceDate).ToArray();

        var existingBalances = await _dbContext.Set<DailyBalance>()
            .Where(x => balanceDates.Contains(x.BalanceDate))
            .ToListAsync(cancellationToken);

        foreach (var aggregate in aggregates)
        {
            var existingBalance = existingBalances.FirstOrDefault(x => x.BalanceDate == aggregate.BalanceDate);

            if (existingBalance is null)
            {
                var newBalance = new DailyBalance
                {
                    BalanceDate = aggregate.BalanceDate,
                    TotalCredits = aggregate.TotalCredits,
                    TotalDebits = aggregate.TotalDebits,
                    Balance = aggregate.Balance,
                    UpdatedAtUtc = updatedAtUtc
                };

                await _dbContext.Set<DailyBalance>().AddAsync(newBalance, cancellationToken);
                continue;
            }

            existingBalance.TotalCredits += aggregate.TotalCredits;
            existingBalance.TotalDebits += aggregate.TotalDebits;
            existingBalance.Balance = existingBalance.TotalCredits - existingBalance.TotalDebits;
            existingBalance.UpdatedAtUtc = updatedAtUtc;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
