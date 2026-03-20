using TransactionService.Domain.Enums;

namespace TransactionService.Api.Contracts.Responses;

/// <summary>
/// Represents the transaction API response.
/// </summary>
public sealed record TransactionResponse(
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
