namespace ConsolidationService.Application.Abstractions;

/// <summary>
/// Provides access to the ambient telemetry context used for correlation and structured logging
/// across the current execution flow.
/// </summary>
public interface ITelemetryContextAccessor
{
    /// <summary>
    /// Gets or sets the correlation identifier used to trace a request across distributed systems.
    /// </summary>
    /// <remarks>
    /// This value should remain consistent throughout the entire request lifecycle.
    /// </remarks>
    string CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the batch associated with the current execution context.
    /// </summary>
    /// <remarks>
    /// Used to correlate logs and operations related to a specific batch.
    /// </remarks>
    Guid BatchId { get; set; }

    /// <summary>
    /// Gets or sets the message identifier associated with the current processing operation.
    /// </summary>
    /// <remarks>
    /// Typically represents the unique identifier of a message received from a queue or event stream.
    /// </remarks>
    string MessageId { get; set; }
}
