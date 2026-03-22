using System.Threading;
using ConsolidationService.Application.Abstractions;

namespace ConsolidationService.Application.Telemetry;

/// <summary>
/// Provides access to the ambient telemetry context for the current asynchronous execution flow.
/// </summary>
/// <remarks>
/// This implementation uses <see cref="AsyncLocal{T}"/> to ensure that telemetry data is isolated
/// per logical execution context, even when running in parallel or across async/await boundaries.
/// </remarks>
public sealed class TelemetryContextAccessor : ITelemetryContextAccessor
{
    private static readonly AsyncLocal<TelemetryContext> _current = new();

    /// <summary>
    /// Gets or sets the correlation identifier used to trace the request across systems.
    /// </summary>
    public string CorrelationId
    {
        get => Current.CorrelationId;
        set => Current.CorrelationId = value;
    }

    /// <summary>
    /// Gets or sets the batch identifier associated with the current execution context.
    /// </summary>
    public Guid BatchId
    {
        get => Current.BatchId;
        set => Current.BatchId = value;
    }

    /// <summary>
    /// Gets or sets the message identifier associated with the current processing operation.
    /// </summary>
    public string MessageId
    {
        get => Current.MessageId;
        set => Current.MessageId = value;
    }

    private static TelemetryContext Current
    {
        get => _current.Value ??= new TelemetryContext();
        set => _current.Value = value;
    }

    private sealed class TelemetryContext
    {
        public string CorrelationId { get; set; } = string.Empty;
        public Guid BatchId { get; set; }
        public string MessageId { get; set; } = string.Empty;
    }
}
