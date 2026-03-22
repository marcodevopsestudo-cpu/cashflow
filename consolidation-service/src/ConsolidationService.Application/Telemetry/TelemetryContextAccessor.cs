using ConsolidationService.Application.Abstractions;

namespace ConsolidationService.Application.Telemetry;

/// <summary>
/// Stores the ambient telemetry identifiers for the current execution scope.
/// </summary>
public sealed class TelemetryContextAccessor : ITelemetryContextAccessor
{
    public string CorrelationId { get; set; } = string.Empty;

    public Guid BatchId { get; set; }

    public string MessageId { get; set; } = string.Empty;
}
