namespace TransactionService.Api.Common.Constants;

/// <summary>
/// Centralizes idempotency-related header names and context keys.
/// </summary>
public static class IdempotencyConstants
{
    /// <summary>
    /// Header name used to uniquely identify an idempotent request.
    /// </summary>
    public const string IdempotencyHeaderName = "Idempotency-Key";

    /// <summary>
    /// Key used to store or retrieve the idempotency key from the request context.
    /// </summary>
    public const string IdempotencyItemKey = "IdempotencyKey";

    /// <summary>
    /// Header name used to indicate that a response was replayed from an idempotent request.
    /// </summary>
    public const string IdempotencyReplayedHeaderName = "x-idempotency-replayed";
}
