using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using System.Net;
using TransactionService.Api.Common.Constants;
using TransactionService.Api.Common.Extensions;
using TransactionService.Application.Common.Errors;

namespace TransactionService.Api.Middlewares;

public sealed class IdempotencyKeyMiddleware : IFunctionsWorkerMiddleware
{

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
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
                context.GetInvocationResult().Value = await request.CreateErrorResponseAsync(
                    HttpStatusCode.BadRequest,
                    ErrorCodes.Validation,
                    $"{IdempotencyConstants.IdempotencyHeaderName} header is required.",
                    context.GetCorrelationId(),
                    CancellationToken.None);

                return;
            }

            var idempotencyKey = values.SingleOrDefault();

            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                context.GetInvocationResult().Value = await request.CreateErrorResponseAsync(
                    HttpStatusCode.BadRequest,
                    ErrorCodes.Validation,
                    $"{IdempotencyConstants.IdempotencyHeaderName} header is required.",
                    context.GetCorrelationId(),
                    CancellationToken.None);

                return;
            }

            context.SetIdempotencyKey(idempotencyKey.Trim());
        }

        await next(context);
    }
}