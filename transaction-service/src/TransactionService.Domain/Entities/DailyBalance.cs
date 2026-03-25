namespace TransactionService.Domain.Entities;

/// <summary>
/// Represents the daily balance aggregation for a specific date,
/// including total credits, total debits, and the resulting balance.
/// </summary>
public sealed class DailyBalance
{
    /// <summary>
    /// Gets or sets the date associated with the balance calculation.
    /// This represents the day for which the aggregation was performed.
    /// </summary>
    public DateOnly BalanceDate { get; set; }

    /// <summary>
    /// Gets or sets the total amount of credit transactions for the day.
    /// </summary>
    public decimal TotalCredits { get; set; }

    /// <summary>
    /// Gets or sets the total amount of debit transactions for the day.
    /// </summary>
    public decimal TotalDebits { get; set; }

    /// <summary>
    /// Gets or sets the resulting balance for the day,
    /// calculated as total credits minus total debits.
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Gets or sets the date and time (UTC) when the balance was last updated.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}
