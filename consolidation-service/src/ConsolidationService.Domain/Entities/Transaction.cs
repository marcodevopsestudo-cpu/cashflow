using ConsolidationService.Domain.Enums;
using System.Transactions;

namespace ConsolidationService.Domain.Entities;

/// <summary>
/// Represents a transaction produced by the Transaction Service and processed by the consolidator.
/// </summary>
/// <remarks>
/// This entity contains the financial transaction data and processing metadata required
/// to support consolidation, retry tracking, and batch correlation.
/// </remarks>
public sealed class Transaction
{
    /// <summary>
    /// Gets the transaction identifier.
    /// </summary>
    public Guid TransactionId { get;  set; }

    /// <summary>
    /// Gets the account identifier.
    /// </summary>
    public string AccountId { get;  set; } = string.Empty;

    /// <summary>
    /// Gets the transaction kind.
    /// </summary>
    public TransactionKind Kind { get;  set; }

    /// <summary>
    /// Gets the transaction amount.
    /// </summary>
    public decimal Amount { get;  set; }

    /// <summary>
    /// Gets the currency.
    /// </summary>
    public string Currency { get;  set; } = string.Empty;

    /// <summary>
    /// Gets the transaction date in UTC.
    /// </summary>
    public DateTime TransactionDateUtc { get;  set; }

    /// <summary>
    /// Gets the optional description.
    /// </summary>
    public string? Description { get;  set; }

    /// <summary>
    /// Gets the correlation id.
    /// </summary>
    public string CorrelationId { get;  set; } = string.Empty;

    /// <summary>
    /// Gets the transaction status.
    /// </summary>
    public Enums.TransactionStatus Status { get;  set; }

    /// <summary>
    /// Gets the creation timestamp in UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get;  set; }

    /// <summary>
    /// Gets the last update timestamp in UTC.
    /// </summary>
    public DateTime? UpdatedAtUtc { get;  set; }

    public DateTime? ConsolidatedAtUtc { get;  set; }
     

}
