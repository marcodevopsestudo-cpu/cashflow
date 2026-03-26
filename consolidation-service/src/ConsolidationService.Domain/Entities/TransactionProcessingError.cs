namespace ConsolidationService.Domain.Entities;

/// <summary>
/// Represents a transaction-level processing error that requires manual review or later inspection.
/// </summary>
public sealed class TransactionProcessingError
{
    /// <summary>
    /// Gets or sets the unique identifier of the error record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the batch associated with the error.
    /// </summary>
    public Guid BatchId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the transaction associated with the error.
    /// </summary>
    public Guid? TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the correlation identifier used to trace the failure.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the application-specific error code.
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message describing the reason for the failure.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stack trace associated with the failure.
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp indicating when the error record was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the retry count associated with the processing attempt.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the current error processing status.
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
