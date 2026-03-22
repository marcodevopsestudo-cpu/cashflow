using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TransactionService.Api.Common.Extensions;
using TransactionService.Api.Configuration;
using TransactionService.Api.Security;
using TransactionService.Application.Common.Diagnostics;
using TransactionService.Application.Common.Errors;

namespace TransactionService.Api.Middlewares;

/// <summary>
/// Authorizes authenticated Entra callers based on Easy Auth propagated headers.
/// </summary>
public sealed class EntraAuthorizationMiddleware : IFunctionsWorkerMiddleware
{
    private readonly IOptions<EntraAuthorizationOptions> _options;
    private readonly EntraAuthorizationEvaluator _evaluator;
    private readonly ILogger<EntraAuthorizationMiddleware> _logger;

    public EntraAuthorizationMiddleware(
        IOptions<EntraAuthorizationOptions> options,
        EntraAuthorizationEvaluator evaluator,
        ILogger<EntraAuthorizationMiddleware> logger)
    {
        _options = options;
        _evaluator = evaluator;
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var request = await context.GetHttpRequestDataAsync();
        if (request is null)
        {
            await next(context);
            return;
        }

        var principal = CallerPrincipalParser.Parse(request);
        var decision = _evaluator.Evaluate(principal, _options.Value);
        var correlationId = context.GetCorrelationId();

        if (!decision.IsAllowed)
        {
            var statusCode = decision.FailureReason is AuthorizationFailureReason.MissingPrincipal or AuthorizationFailureReason.MissingAppId
                ? HttpStatusCode.Unauthorized
                : HttpStatusCode.Forbidden;

            _logger.AuthorizationRejected(correlationId, principal?.AppId, decision.FailureReason?.ToString() ?? "Unknown");

            context.GetInvocationResult().Value = await request.CreateErrorResponseAsync(
                statusCode,
                statusCode == HttpStatusCode.Unauthorized ? ErrorCodes.Unauthorized : ErrorCodes.Forbidden,
                GetMessage(decision.FailureReason),
                correlationId,
                CancellationToken.None);

            return;
        }

        if (decision.Principal is not null)
        {
            context.SetCaller(decision.Principal);
            _logger.AuthorizationAccepted(correlationId, decision.Principal.AppId ?? "unknown", decision.Principal.TenantId ?? "unknown");
        }

        await next(context);
    }

    private static string GetMessage(AuthorizationFailureReason? reason) => reason switch
    {
        AuthorizationFailureReason.MissingPrincipal => "The request is not authenticated by Microsoft Entra ID.",
        AuthorizationFailureReason.MissingAppId => "The caller application identifier was not found in the token claims.",
        AuthorizationFailureReason.AppIdNotAllowed => "The caller application is not authorized to access this API.",
        AuthorizationFailureReason.InvalidAudience => "The token audience is not allowed for this API.",
        AuthorizationFailureReason.InvalidIssuer => "The token issuer is not allowed for this API.",
        
        _ => "The caller is not authorized to access this API."
    };
}
