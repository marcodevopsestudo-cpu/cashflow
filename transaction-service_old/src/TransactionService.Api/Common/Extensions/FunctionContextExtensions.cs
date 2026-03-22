using Microsoft.Azure.Functions.Worker;
using TransactionService.Api.Common.Constants;

namespace TransactionService.Api.Common.Extensions;

/// <summary>
/// Provides extension methods for <see cref="FunctionContext"/>.
/// </summary>
public static class FunctionContextExtensions
{
    /// <summary>
    /// Gets the correlation identifier stored in the current function context.
    /// </summary>
    /// <param name="context">The current function context.</param>
    /// <returns>The correlation identifier for the current invocation.</returns>
    public static string GetCorrelationId(this FunctionContext context)
    {
        if (context.Items.TryGetValue(CorrelationConstants.CorrelationIdItemKey, out var value) &&
            value is string correlationId &&
            !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId;
        }

        return Guid.NewGuid().ToString("N");
    }
}
