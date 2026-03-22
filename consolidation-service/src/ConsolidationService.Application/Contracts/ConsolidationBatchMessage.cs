namespace ConsolidationService.Application.Contracts;

/// <summary>
/// Represents the message published by Transaction Service to request a consolidation cycle.
/// </summary>
public sealed class ConsolidationBatchMessage
{
    public Guid BatchId { get; init; }

    public string CorrelationId { get; init; } = string.Empty;

    public DateTime PublishedAtUtc { get; init; }

    public IReadOnlyCollection<long> TransactionIds { get; init; } = Array.Empty<long>();
}
