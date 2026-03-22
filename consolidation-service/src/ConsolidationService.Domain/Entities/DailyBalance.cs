namespace ConsolidationService.Domain.Entities;

/// <summary>
/// Represents the read model that stores the consolidated financial balance for a specific day.
/// </summary>
/// <remarks>
/// This entity is typically used as a projection of processed transactions,
/// allowing efficient queries over daily financial summaries.
/// </remarks>
public sealed class DailyBalance
{
    /// <summary>
    /// Gets or sets the date associated with the balance.
    /// </summary>
    public DateOnly BalanceDate { get; set; }

    /// <summary>
    /// Gets or sets the total amount of credited transactions for the day.
    /// </summary>
    public decimal TotalCredits { get; set; }

    /// <summary>
    /// Gets or sets the total amount of debited transactions for the day.
    /// </summary>
    public decimal TotalDebits { get; set; }

    /// <summary>
    /// Gets or sets the resulting balance for the day.
    /// </summary>
    /// <remarks>
    /// Typically calculated as <c>TotalCredits - TotalDebits</c>.
    /// </remarks>
    public decimal Balance { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp indicating when the balance was last updated.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}
