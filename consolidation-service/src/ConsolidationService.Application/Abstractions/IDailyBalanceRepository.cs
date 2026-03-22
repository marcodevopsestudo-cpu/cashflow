using ConsolidationService.Domain.ValueObjects;

namespace ConsolidationService.Application.Abstractions;

/// <summary>
/// Provides persistence operations for the daily balance read model.
/// </summary>
public interface IDailyBalanceRepository
{
    /// <summary>
    /// upsert a daily consolidation item
    /// </summary>
    /// <param name="aggregates"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpsertAsync(IReadOnlyCollection<DailyAggregate> aggregates, CancellationToken cancellationToken);
}
