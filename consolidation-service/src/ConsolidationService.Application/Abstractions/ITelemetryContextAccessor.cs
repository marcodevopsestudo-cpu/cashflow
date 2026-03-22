namespace ConsolidationService.Application.Abstractions;

/// <summary>
/// Provides an ambient telemetry context for structured logging and correlation.
/// </summary>
public interface ITelemetryContextAccessor
{
    string CorrelationId { get; set; }

    Guid BatchId { get; set; }

    string MessageId { get; set; }
}
