using Microsoft.Azure.Functions.Worker;

using TransactionService.Api.Common.Constants;

namespace TransactionService.Api.Common.Extensions;

public static class FunctionContextIdempotencyExtensions
{

    public static void SetIdempotencyKey(this FunctionContext context, string idempotencyKey)
    {
        context.Items[IdempotencyConstants.IdempotencyItemKey] = idempotencyKey;
    }

    public static string? GetIdempotencyKey(this FunctionContext context)
    {
        if (context.Items.TryGetValue(IdempotencyConstants.IdempotencyItemKey, out var value) &&
            value is string key &&
            !string.IsNullOrWhiteSpace(key))
        {
            return key;
        }

        return null;
    }
}