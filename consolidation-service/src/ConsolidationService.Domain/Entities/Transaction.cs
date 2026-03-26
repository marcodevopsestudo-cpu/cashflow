using ConsolidationService.Domain.Enums;
using TransactionService.Domain.Enums;

namespace ConsolidationService.Domain.Entities;

/// <summary>
/// Represents a transaction produced by the Transaction Service and processed by the consolidator.
/// </summary>
public sealed class Transaction
{
    /// <summary>
    /// Gets or sets the unique identifier of the transaction.
    /// </summary>
    public Guid TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the account identifier associated with the transaction.
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transaction kind.
    /// </summary>
    public TransactionKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the transaction amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the transaction currency.
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC date and time when the transaction occurred.
    /// </summary>
    public DateTime TransactionDateUtc { get; set; }

    /// <summary>
    /// Gets or sets the transaction description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the correlation identifier used for distributed tracing.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the business status of the transaction.
    /// </summary>
    public TransactionStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp indicating when the transaction record was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp indicating when the transaction record was last updated.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the processing status used by the consolidation workflow.
    /// </summary>
    public ConsolidationStatus? ConsolidationStatus { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp indicating when the transaction was consolidated.
    /// </summary>
    public DateTime? ConsolidatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the batch identifier responsible for the latest processing attempt.
    /// </summary>
    public Guid? LastConsolidationBatchId { get; set; }

    /// <summary>
    /// Gets or sets the number of processing attempts made for this transaction.
    /// </summary>
    public int ConsolidationAttemptCount { get; set; }
}
