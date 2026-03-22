using ConsolidationService.Domain.Enums;

namespace ConsolidationService.Domain.Entities;

/// <summary>
/// Represents the processing control entity for a consolidation batch.
/// </summary>
/// <remarks>
/// This entity tracks the lifecycle of a batch, including its processing status,
/// retry attempts, correlation metadata, and execution timestamps.
/// </remarks>
public sealed class DailyBatch
{
    /// <summary>
    /// Gets or sets the unique identifier of the batch.
    /// </summary>
    public Guid BatchId { get; set; }

    /// <summary>
    /// Gets or sets the correlation identifier used to trace the batch across distributed systems.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current processing status of the batch.
    /// </summary>
    public BatchStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the number of transactions associated with the batch.
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// Gets or sets the number of retry attempts performed for the batch.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the last error message recorded during batch processing.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp indicating when the batch record was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp indicating when batch processing started.
    /// </summary>
    public DateTime? StartedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp indicating when batch processing completed.
    /// </summary>
    public DateTime? CompletedAtUtc { get; set; }
}
