using ConsolidationService.Domain.Enums;

namespace ConsolidationService.Domain.Entities;

/// <summary>
/// Represents a transaction produced by Transaction Service and consumed by the consolidator.
/// </summary>
public sealed class Transaction
{
    public long Id { get; set; }

    public decimal Amount { get; set; }

    public TransactionType Type { get; set; }

    public DateTime OccurredAtUtc { get; set; }

    public TransactionProcessingStatus ProcessingStatus { get; set; }

    public Guid? LastBatchId { get; set; }

    public DateTime? ConsolidatedAtUtc { get; set; }

    public int ProcessingAttemptCount { get; set; }

    public DateOnly BalanceDate => DateOnly.FromDateTime(OccurredAtUtc);
}
