using TransactionService.Domain.Enums;

namespace TransactionService.Application.Transactions.Common;

/// <summary>
/// Represents a transaction DTO.
/// </summary>
public sealed record TransactionDto(
    Guid Id,
    string AccountId,
    TransactionKind Kind,
    decimal Amount,
    string Currency,
    DateTime DateUtc,
    string? Description,
    string Status,
    string CorrelationId,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
