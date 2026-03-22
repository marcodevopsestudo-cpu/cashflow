using Microsoft.Azure.Functions.Worker;
using TransactionService.Api.Common.Constants;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace TransactionService.Api.Common.Middleware;

/// <summary>
/// Middleware responsible for resolving, propagating, and logging
/// the correlation ID for each function invocation.
/// </summary>
/// <remarks>
/// The correlation ID is retrieved from the incoming request header if available;
/// otherwise, a new one is generated. It is then stored in the <see cref="FunctionContext"/>,
/// added to the logging scope, and included in the response headers.
/// </remarks>
public sealed class CorrelationIdMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationIdMiddleware"/> class.
    /// </summary>
    /// <param name="logger">The logger instance used for structured logging.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> is null.
    /// </exception>
    public CorrelationIdMiddleware(ILogger<CorrelationIdMiddleware> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the middleware logic to resolve and propagate the correlation ID.
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

        var correlationId = await ResolveCorrelationIdAsync(context);

        context.Items[CorrelationConstants.CorrelationIdItemKey] = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object>(1)
        {
            ["CorrelationId"] = correlationId
        }))
        {
            await next(context);
        }

        await AddCorrelationIdToResponseAsync(context, correlationId);
    }

    /// <summary>
    /// Resolves the correlation ID from the incoming HTTP request or generates a new one.
    /// </summary>
    /// <param name="context">The current function execution context.</param>
    /// <returns>
    /// The correlation ID extracted from the request header or a newly generated one.
    /// </returns>
    private static async Task<string> ResolveCorrelationIdAsync(FunctionContext context)
    {
        var request = await context.GetHttpRequestDataAsync();

        if (request is null)
        {
            return GenerateCorrelationId();
        }

        if (request.Headers.TryGetValues(CorrelationConstants.CorrelationHeaderName, out var values))
        {
            var headerValue = values.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                return headerValue;
            }
        }

        return GenerateCorrelationId();
    }

    /// <summary>
    /// Adds the correlation ID to the HTTP response headers if not already present.
    /// </summary>
    /// <param name="context">The current function execution context.</param>
    /// <param name="correlationId">The correlation ID to include in the response.</param>
    /// <returns>A completed task.</returns>
    private static Task AddCorrelationIdToResponseAsync(FunctionContext context, string correlationId)
    {
        var response = context.GetHttpResponseData();

        if (response is not null &&
            !response.Headers.TryGetValues(CorrelationConstants.CorrelationHeaderName, out _))
        {
            response.Headers.Add(CorrelationConstants.CorrelationHeaderName, correlationId);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Generates a new correlation ID.
    /// </summary>
    /// <returns>A new correlation ID string in "N" format.</returns>
    private static string GenerateCorrelationId()
        => Guid.NewGuid().ToString("N");
}
