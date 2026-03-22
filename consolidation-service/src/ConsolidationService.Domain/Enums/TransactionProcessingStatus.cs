namespace ConsolidationService.Domain.Enums;

/// <summary>
/// Represents the processing status of a transaction during the consolidation lifecycle.
/// </summary>
/// <remarks>
/// Indicates the current state of a transaction as it progresses through
/// the consolidation pipeline, including failure and manual review scenarios.
/// </remarks>
public enum TransactionProcessingStatus
{
    /// <summary>
    /// The transaction is pending processing and has not yet been consolidated.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// The transaction has been successfully consolidated.
    /// </summary>
    Consolidated = 2,

    /// <summary>
    /// The transaction processing has failed.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// The transaction requires manual review due to processing issues.
    /// </summary>
    PendingManualReview = 4
}
