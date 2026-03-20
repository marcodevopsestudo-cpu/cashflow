using Microsoft.Azure.Functions.Worker;
using TransactionService.Api.Common.Constants;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

public sealed class CorrelationIdMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(ILogger<CorrelationIdMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var correlationId = await ResolveCorrelationIdAsync(context);

        context.Items[CorrelationConstants.CorrelationIdItemKey] = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            await next(context);
        }

        await AddCorrelationIdToResponseAsync(context, correlationId);
    }

    private static async Task<string> ResolveCorrelationIdAsync(FunctionContext context)
    {
        var request = await context.GetHttpRequestDataAsync();

        if (request is null)
        {
            return Guid.NewGuid().ToString("N");
        }

        if (request.Headers.TryGetValues(CorrelationConstants.CorrelationHeaderName, out var values))
        {
            var headerValue = values.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                return headerValue;
            }
        }

        return Guid.NewGuid().ToString("N");
    }

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
}