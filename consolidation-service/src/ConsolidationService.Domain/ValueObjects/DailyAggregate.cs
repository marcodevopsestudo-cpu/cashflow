namespace ConsolidationService.Domain.ValueObjects;

/// <summary>
/// Represents an aggregated credit/debit total for a specific balance date.
/// </summary>
public sealed record DailyAggregate(DateOnly BalanceDate, decimal TotalCredits, decimal TotalDebits)
{
    /// <summary>
    /// Gets the net balance for the day.
    /// </summary>
    public decimal Balance => TotalCredits - TotalDebits;
}
