namespace ConsolidationService.Domain.Enums;

/// <summary>
/// Represents the lifecycle state of a consolidation batch.
/// </summary>
/// <remarks>
/// Defines the possible states a batch can transition through during processing.
/// </remarks>
public enum BatchStatus
{
    /// <summary>
    /// The batch has been created but has not yet started processing.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// The batch is currently being processed.
    /// </summary>
    Processing = 2,

    /// <summary>
    /// The batch has been successfully processed.
    /// </summary>
    Succeeded = 3,

    /// <summary>
    /// The batch processing has failed and may require retry or manual intervention.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// The batch has been intentionally ignored and will not be processed.
    /// </summary>
    Ignored = 5
}
