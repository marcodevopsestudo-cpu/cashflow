using TransactionService.Domain.Enums;

namespace TransactionService.Application.Transactions.Common;

/// <summary>
/// Represents a transaction DTO.
/// </summary>
public sealed record TransactionDto(
    Guid TransactionId,
    string AccountId,
    TransactionKind Kind,
    decimal Amount,
    string Currency,
    DateTime TransactionDateUtc,
    string? Description,
    string Status,
    string CorrelationId,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
