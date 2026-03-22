using TransactionService.Domain.Enums;

namespace TransactionService.Domain.Events;

/// <summary>
/// Represents the integration event emitted when a transaction is created.
/// </summary>
public sealed record TransactionCreatedEvent(
    Guid TransactionId,
    string AccountId,
    TransactionKind Kind,
    decimal Amount,
    string Currency,
    DateTime TransactionDateUtc,
    string? Description,
    string CorrelationId,
    DateTime CreatedAtUtc);
