namespace TransactionService.Domain.Enums;

/// <summary>
/// Represents the supported transaction kinds.
/// </summary>
/// <remarks>
/// Defines the direction of a transaction in relation to the account balance.
/// </remarks>
public enum TransactionKind
{
    /// <summary>
    /// Represents a credit transaction, which increases the account balance.
    /// </summary>
    Credit = 1,

    /// <summary>
    /// Represents a debit transaction, which decreases the account balance.
    /// </summary>
    Debit = 2
}
