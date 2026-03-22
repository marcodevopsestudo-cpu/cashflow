namespace ConsolidationService.Domain.Entities;

/// <summary>
/// Represents the read model that stores the consolidated balance for a single day.
/// </summary>
public sealed class DailyBalance
{
    public DateOnly BalanceDate { get; set; }

    public decimal TotalCredits { get; set; }

    public decimal TotalDebits { get; set; }

    public decimal Balance { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
