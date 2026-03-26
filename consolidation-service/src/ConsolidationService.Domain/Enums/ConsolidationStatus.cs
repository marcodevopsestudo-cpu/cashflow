/// <summary>
/// Represents the lifecycle status of a consolidation process.
/// </summary>
public enum ConsolidationStatus
{
    /// <summary>
    /// The consolidation process has not started yet.
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// The consolidation process is currently in progress.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// The consolidation process completed successfully.
    /// </summary>
    Consolidated = 2,

    /// <summary>
    /// The consolidation process failed due to an error.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// The consolidation process requires manual review due to validation or processing issues.
    /// </summary>
    PendingManualReview = 4
}
