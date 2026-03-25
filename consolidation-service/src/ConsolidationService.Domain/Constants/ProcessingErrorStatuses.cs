namespace ConsolidationService.Domain.Constants;

/// <summary>
/// Contains constant values representing processing error statuses.
/// </summary>
public static class ProcessingErrorStatuses
{
    /// <summary>
    /// Status indicating that the transaction requires manual review after a processing failure.
    /// </summary>
    public const string PendingManualReview = "PendingManualReview";
}
