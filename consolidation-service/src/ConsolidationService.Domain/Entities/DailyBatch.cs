using ConsolidationService.Domain.Enums;

namespace ConsolidationService.Domain.Entities;

/// <summary>
/// Represents the processing control row for a consolidation batch.
/// </summary>
public sealed class DailyBatch
{
    public Guid BatchId { get; set; }

    public string CorrelationId { get; set; } = string.Empty;

    public BatchStatus Status { get; set; }

    public int TransactionCount { get; set; }

    public int RetryCount { get; set; }

    public string? LastError { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }
}
