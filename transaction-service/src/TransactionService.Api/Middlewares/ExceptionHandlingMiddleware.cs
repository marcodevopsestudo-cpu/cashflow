using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using TransactionService.Api.Common.Extensions;
using TransactionService.Application.Common.Errors;
using TransactionService.Application.Common.Exceptions;
using TransactionService.Api.Resources;

namespace TransactionService.Api.Middlewares;

/// <summary>
/// Middleware responsible for translating application exceptions
/// into standardized HTTP error responses.
/// </summary>
/// <remarks>
/// If the exception is a known <see cref="ApplicationExceptionBase"/>,
/// the middleware returns the mapped error details. Otherwise, it returns
/// a generic internal server error response.
/// </remarks>
public sealed class ExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
{
    /// <summary>
    /// Executes the middleware pipeline and converts thrown exceptions
    /// into HTTP responses when possible.
    /// </summary>
    /// <param name="context">The current function execution context.</param>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> or <paramref name="next"/> is null.
    /// </exception>
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

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
                ApiMessageCatalog.UnexpectedError,
                correlationId,
                CancellationToken.None);
        }
    }
}
