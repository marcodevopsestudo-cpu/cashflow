using Microsoft.Azure.Functions.Worker;
using TransactionService.Api.Common.Constants;

namespace TransactionService.Api.Common.Extensions;

/// <summary>
/// Provides extension methods to store and retrieve idempotency-related
/// information within the <see cref="FunctionContext"/>.
/// </summary>
public static class FunctionContextIdempotencyExtensions
{
    /// <summary>
    /// Stores the idempotency key in the current <see cref="FunctionContext"/>.
    /// </summary>
    /// <param name="context">The current function execution context.</param>
    /// <param name="idempotencyKey">The idempotency key to be stored.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> or <paramref name="idempotencyKey"/> is null.
    /// </exception>
    public static void SetIdempotencyKey(this FunctionContext context, string idempotencyKey)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(idempotencyKey);

        context.Items[IdempotencyConstants.IdempotencyItemKey] = idempotencyKey;
    }

    /// <summary>
    /// Retrieves the idempotency key from the current <see cref="FunctionContext"/>.
    /// </summary>
    /// <param name="context">The current function execution context.</param>
    /// <returns>
    /// The idempotency key if present and valid; otherwise, <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> is null.
    /// </exception>
    public static string? GetIdempotencyKey(this FunctionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Items.TryGetValue(IdempotencyConstants.IdempotencyItemKey, out var value) &&
            value is string key &&
            !string.IsNullOrWhiteSpace(key))
        {
            return key;
        }

        return null;
    }
}
