namespace ConsolidationService.Domain.Enums;

/// <summary>
/// Represents the type of a financial transaction.
/// </summary>
/// <remarks>
/// Determines how a transaction impacts the balance during aggregation.
/// </remarks>
public enum TransactionType
{
    /// <summary>
    /// Represents a credit transaction that increases the balance.
    /// </summary>
    Credit = 1,

    /// <summary>
    /// Represents a debit transaction that decreases the balance.
    /// </summary>
    Debit = 2
}
