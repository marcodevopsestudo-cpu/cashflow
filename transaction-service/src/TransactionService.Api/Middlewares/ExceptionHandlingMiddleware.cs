using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using TransactionService.Api.Common.Extensions;
using TransactionService.Application.Common.Errors;
using TransactionService.Application.Common.Exceptions;
using TransactionService.Api.Resources;

namespace TransactionService.Api.Middlewares;

/// <summary>
/// Middleware responsible for handling unhandled exceptions during function execution
/// and converting them into standardized HTTP error responses.
/// </summary>
public sealed class ExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
    /// </summary>
    /// <param name="logger">The logger used to record exception details.</param>
    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to handle the function execution pipeline,
    /// catching and processing any unhandled exceptions.
    /// </summary>
    /// <param name="context">The current function execution context.</param>
    /// <param name="next">The next middleware or function delegate in the pipeline.</param>
    /// <returns>A task that represents the asynchronous execution.</returns>
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
                _logger.LogError(ex, ApiMessageCatalog.Logs.UnhandledNonHttpTriggerException);
                throw;
            }

            var correlationId = context.GetCorrelationId();

            _logger.LogError(
                ex,
                ApiMessageCatalog.Logs.UnhandledRequestException,
                correlationId);

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
