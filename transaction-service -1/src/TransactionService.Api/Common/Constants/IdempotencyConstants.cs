namespace TransactionService.Api.Common.Constants;

/// <summary>
/// Centralizes idempotency-related header names and context keys.
/// </summary>
public static class IdempotencyConstants
{
    public const string IdempotencyHeaderName = "Idempotency-Key";
    public const string IdempotencyItemKey = "IdempotencyKey";
    public const string IdempotencyReplayedHeaderName = "x-idempotency-replayed";
}
