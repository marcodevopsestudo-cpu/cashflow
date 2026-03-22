using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using TransactionService.Api.Common.Constants;
using TransactionService.Api.Common.Extensions;
using TransactionService.Application.Common.Errors;

namespace TransactionService.Api.Middlewares;

/// <summary>
/// Middleware responsible for validating and storing the idempotency key
/// for HTTP POST requests.
/// </summary>
/// <remarks>
/// This middleware ensures that POST requests include a valid idempotency header.
/// When present and valid, the key is stored in the current <see cref="FunctionContext"/>
/// for later use during request processing.
/// </remarks>
public sealed class IdempotencyKeyMiddleware : IFunctionsWorkerMiddleware
{
    /// <summary>
    /// Executes the middleware logic for validating the idempotency key.
    /// </summary>
    /// <param name="context">The current function execution context.</param>
    /// <param name="next">The next middleware delegate in the execution pipeline.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> or <paramref name="next"/> is null.
    /// </exception>
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var request = await context.GetHttpRequestDataAsync();

        if (request is null)
        {
            await next(context);
            return;
        }

        if (string.Equals(request.Method, "POST", StringComparison.OrdinalIgnoreCase))
        {
            if (!request.Headers.TryGetValues(IdempotencyConstants.IdempotencyHeaderName, out var values))
            {
                context.GetInvocationResult().Value = await CreateMissingIdempotencyResponseAsync(request, context);
                return;
            }

            var idempotencyKey = values.SingleOrDefault();

            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                context.GetInvocationResult().Value = await CreateMissingIdempotencyResponseAsync(request, context);
                return;
            }

            context.SetIdempotencyKey(idempotencyKey.Trim());
        }

        await next(context);
    }

    /// <summary>
    /// Creates the standardized bad request response used when the idempotency header
    /// is missing or invalid.
    /// </summary>
    /// <param name="request">The current HTTP request.</param>
    /// <param name="context">The current function execution context.</param>
    /// <returns>A task containing the HTTP error response.</returns>
    private static Task<Microsoft.Azure.Functions.Worker.Http.HttpResponseData> CreateMissingIdempotencyResponseAsync(
        Microsoft.Azure.Functions.Worker.Http.HttpRequestData request,
        FunctionContext context)
    {
        return request.CreateErrorResponseAsync(
            HttpStatusCode.BadRequest,
            ErrorCodes.Validation,
            $"{IdempotencyConstants.IdempotencyHeaderName} header is required.",
            context.GetCorrelationId(),
            CancellationToken.None);
    }
}
