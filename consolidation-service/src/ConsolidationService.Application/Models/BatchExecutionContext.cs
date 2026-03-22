using ConsolidationService.Application.Contracts;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.ValueObjects;

namespace ConsolidationService.Application.Models;

/// <summary>
/// Represents the mutable workflow context used across the manual consolidation pipeline.
/// </summary>
public sealed class BatchExecutionContext
{
    public required ConsolidationBatchMessage Message { get; init; }

    public DailyBatch? Batch { get; set; }

    public IReadOnlyCollection<Transaction> Transactions { get; set; } = Array.Empty<Transaction>();

    public IReadOnlyCollection<DailyAggregate> Aggregates { get; set; } = Array.Empty<DailyAggregate>();

    public DateTime StartedAtUtc { get; init; } = DateTime.UtcNow;

    public string MessageId { get; set; } = string.Empty;
}
