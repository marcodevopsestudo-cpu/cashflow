using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using TransactionService.Api.Common.Extensions;
using TransactionService.Application.Common.Errors;
using TransactionService.Application.Common.Exceptions;
using TransactionService.Api.Resources;

namespace TransactionService.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

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
                _logger.LogError(ex, "Unhandled exception for non-HTTP trigger.");
                throw;
            }

            var correlationId = context.GetCorrelationId();

            _logger.LogError(
                ex,
                "Unhandled exception while processing request. CorrelationId={CorrelationId}",
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
