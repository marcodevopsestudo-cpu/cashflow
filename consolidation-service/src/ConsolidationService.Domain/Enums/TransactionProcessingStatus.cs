namespace ConsolidationService.Domain.Enums;

/// <summary>
/// Represents the consolidation processing status of a transaction.
/// </summary>
public enum TransactionProcessingStatus
{
    Pending = 1,
    Consolidated = 2,
    Failed = 3,
    PendingManualReview = 4
}
