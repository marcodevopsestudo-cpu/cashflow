using ConsolidationService.Domain.Enums;

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
    /// Gets or sets the unique identifier of the transaction.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the monetary amount of the transaction.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the type of the transaction, such as credit or debit.
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp indicating when the transaction occurred.
    /// </summary>
    public DateTime OccurredAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the current processing status of the transaction.
    /// </summary>
    public TransactionProcessingStatus ProcessingStatus { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the last batch that processed this transaction.
    /// </summary>
    public Guid? LastBatchId { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp indicating when the transaction was consolidated.
    /// </summary>
    public DateTime? ConsolidatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the number of processing attempts performed for this transaction.
    /// </summary>
    public int ProcessingAttemptCount { get; set; }

    /// <summary>
    /// Gets the balance date derived from <see cref="OccurredAtUtc"/>.
    /// </summary>
    /// <remarks>
    /// This value is used to group transactions during daily aggregation.
    /// </remarks>
    public DateOnly BalanceDate => DateOnly.FromDateTime(OccurredAtUtc);
}
