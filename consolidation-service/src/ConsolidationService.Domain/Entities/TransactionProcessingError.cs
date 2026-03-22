namespace ConsolidationService.Domain.Entities;

/// <summary>
/// Represents a transaction or batch item that failed processing and requires manual investigation.
/// </summary>
/// <remarks>
/// This entity is used to persist failure details for auditing, troubleshooting,
/// and manual review workflows.
/// </remarks>
public sealed class TransactionProcessingError
{
    /// <summary>
    /// Gets or sets the unique identifier of the error record.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the batch associated with the failure.
    /// </summary>
    public Guid BatchId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the transaction associated with the failure, if applicable.
    /// </summary>
    public long? TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the correlation identifier used to trace the request across systems.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error code representing the type of failure.
    /// </summary>
    /// <remarks>
    /// Typically derived from the exception type or mapped to a domain-specific error code.
    /// </remarks>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable error message describing the failure.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stack trace associated with the failure, if available.
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp indicating when the error was recorded.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the number of retry attempts performed before the failure was recorded.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the current status of the error processing workflow.
    /// </summary>
    /// <remarks>
    /// Indicates whether the error is pending review, resolved, or reprocessed.
    /// </remarks>
    public string Status { get; set; } = string.Empty;
}
