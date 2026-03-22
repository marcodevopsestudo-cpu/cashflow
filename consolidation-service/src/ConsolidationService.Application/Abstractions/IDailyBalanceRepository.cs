using ConsolidationService.Domain.ValueObjects;

namespace ConsolidationService.Application.Abstractions;

/// <summary>
/// Provides persistence operations for the daily balance read model.
/// </summary>
public interface IDailyBalanceRepository
{
    Task UpsertAsync(IReadOnlyCollection<DailyAggregate> aggregates, CancellationToken cancellationToken);
}
