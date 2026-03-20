using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using System.Net;
using TransactionService.Api.Common.Extensions;
using TransactionService.Application.Common.Errors;
using TransactionService.Application.Common.Exceptions;

namespace TransactionService.Api.Middlewares;

/// <summary>
/// Translates application exceptions to HTTP responses.
/// </summary>
public sealed class ExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
{
    /// <summary>
    /// Executes the middleware.
    /// </summary>
    /// <param name="context">The function context.</param>
    /// <param name="next">The next delegate.</param>
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var request = await context.GetHttpRequestDataAsync();

            if (request is null)
            {
                throw;
            }

            var correlationId = context.GetCorrelationId();

            if (ex is ApplicationExceptionBase appEx)
            {
                context.GetInvocationResult().Value = await request.CreateErrorResponseAsync(
                    (HttpStatusCode)appEx.Error.HttpStatusCode,
                    appEx.Error.Code,
                    appEx.Error.Message,
                    correlationId,
                    CancellationToken.None);

                return;
            }

            context.GetInvocationResult().Value = await request.CreateErrorResponseAsync(
                HttpStatusCode.InternalServerError,
                ErrorCodes.Unexpected,
                "An unexpected error occurred.",
                correlationId,
                CancellationToken.None);
        }
    }
}
