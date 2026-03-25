namespace TransactionService.Application.Transactions.Common;

/// <summary>
/// Represents the daily balance response returned to the client,
/// including aggregated credits, debits, and the resulting balance.
/// </summary>
public sealed class DailyBalanceDto
{
    /// <summary>
    /// Gets the date associated with the balance calculation.
    /// </summary>
    public DateOnly BalanceDate { get; init; }

    /// <summary>
    /// Gets the total amount of credit transactions for the day.
    /// </summary>
    public decimal TotalCredits { get; init; }

    /// <summary>
    /// Gets the total amount of debit transactions for the day.
    /// </summary>
    public decimal TotalDebits { get; init; }

    /// <summary>
    /// Gets the resulting balance for the day,
    /// calculated as total credits minus total debits.
    /// </summary>
    public decimal Balance { get; init; }

    /// <summary>
    /// Gets the date and time (UTC) when the balance was last updated.
    /// </summary>
    public DateTime UpdatedAtUtc { get; init; }
}
