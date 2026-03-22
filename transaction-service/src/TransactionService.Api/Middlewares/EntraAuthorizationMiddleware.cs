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
using TransactionService.Api.Resources;

namespace TransactionService.Api.Middlewares;

/// <summary>
/// Authorizes authenticated Microsoft Entra ID callers based on headers
/// propagated by Easy Auth.
/// </summary>
/// <remarks>
/// This middleware evaluates the caller identity extracted from the incoming request
/// and determines whether access should be granted according to the configured
/// <see cref="EntraAuthorizationOptions"/>.
/// </remarks>
public sealed class EntraAuthorizationMiddleware : IFunctionsWorkerMiddleware
{
    private readonly IOptions<EntraAuthorizationOptions> _options;
    private readonly EntraAuthorizationEvaluator _evaluator;
    private readonly ILogger<EntraAuthorizationMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntraAuthorizationMiddleware"/> class.
    /// </summary>
    /// <param name="options">The authorization options used to validate incoming callers.</param>
    /// <param name="evaluator">The evaluator responsible for authorization decision logic.</param>
    /// <param name="logger">The logger used for authorization diagnostics.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any dependency is null.
    /// </exception>
    public EntraAuthorizationMiddleware(
        IOptions<EntraAuthorizationOptions> options,
        EntraAuthorizationEvaluator evaluator,
        ILogger<EntraAuthorizationMiddleware> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the authorization middleware for the current function invocation.
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

        var principal = CallerPrincipalParser.Parse(request);
        var decision = _evaluator.Evaluate(principal, _options.Value);
        var correlationId = context.GetCorrelationId();

        if (!decision.IsAllowed)
        {
            var statusCode = decision.FailureReason is AuthorizationFailureReason.MissingPrincipal or AuthorizationFailureReason.MissingAppId
                ? HttpStatusCode.Unauthorized
                : HttpStatusCode.Forbidden;

            _logger.AuthorizationRejected(
                correlationId,
                principal?.AppId,
                decision.FailureReason?.ToString() ?? ApiMessageCatalog.AuthorizationReasonUnknown);

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

            _logger.AuthorizationAccepted(
                correlationId,
                decision.Principal.AppId ?? ApiMessageCatalog.AuthorizationValueUnknown,
                decision.Principal.TenantId ?? ApiMessageCatalog.AuthorizationValueUnknown);
        }

        await next(context);
    }

    /// <summary>
    /// Returns the error message associated with a specific authorization failure reason.
    /// </summary>
    /// <param name="reason">The authorization failure reason.</param>
    /// <returns>A user-friendly authorization failure message.</returns>
    private static string GetMessage(AuthorizationFailureReason? reason) => reason switch
    {
        AuthorizationFailureReason.MissingPrincipal => ApiMessageCatalog.MissingPrincipal,
        AuthorizationFailureReason.MissingAppId => ApiMessageCatalog.MissingAppId,
        AuthorizationFailureReason.AppIdNotAllowed => ApiMessageCatalog.AppIdNotAllowed,
        AuthorizationFailureReason.InvalidAudience => ApiMessageCatalog.InvalidAudience,
        AuthorizationFailureReason.InvalidIssuer => ApiMessageCatalog.InvalidIssuer,
        _ => ApiMessageCatalog.UnauthorizedCaller
    };
}
