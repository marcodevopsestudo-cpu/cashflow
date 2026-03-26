namespace ConsolidationService.Application.Contracts;

/// <summary>
/// Represents a message published by the Transaction Service to trigger a consolidation cycle.
/// </summary>
public sealed class ConsolidationBatchMessage
{
    /// <summary>
    /// Gets the unique identifier of the batch to be processed.
    /// </summary>
    public Guid BatchId { get; init; }

    /// <summary>
    /// Gets the correlation identifier used to trace the request across distributed systems.
    /// </summary>
    /// <remarks>
    /// This value should remain consistent throughout the entire processing lifecycle.
    /// </remarks>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets the UTC timestamp indicating when the message was published.
    /// </summary>
    public DateTime PublishedAtUtc { get; init; }

    /// <summary>
    /// Gets the collection of transaction identifiers to be included in the consolidation process.
    /// </summary>
    /// <remarks>
    /// Each identifier represents a transaction that must be processed as part of the batch.
    /// </remarks>
    public IReadOnlyCollection<Guid> TransactionIds { get; init; } = [];
}
