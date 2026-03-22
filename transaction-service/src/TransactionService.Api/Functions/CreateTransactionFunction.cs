using System.Net;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using TransactionService.Api.Common.Constants;
using TransactionService.Api.Common.Extensions;
using TransactionService.Api.Contracts.Requests;
using TransactionService.Application.Common.Diagnostics;
using TransactionService.Api.Resources;
using TransactionService.Application.Transactions.Commands.CreateTransaction;

namespace TransactionService.Api.Functions;

/// <summary>
/// Handles the create transaction endpoint.
/// </summary>
public sealed class CreateTransactionFunction
{
    private readonly IMediator _mediator;
    private readonly ILogger<CreateTransactionFunction> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTransactionFunction"/> class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    /// <param name="logger">The logger.</param>
    public CreateTransactionFunction(IMediator mediator, ILogger<CreateTransactionFunction> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Executes the endpoint.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="context">The HTTP FunctionContext.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The HTTP response.</returns>
    [Function(nameof(CreateTransactionFunction))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "transactions")] HttpRequestData request,
        FunctionContext context,
        CancellationToken cancellationToken)
    {
        var payload = await request.ReadFromJsonAsync<CreateTransactionRequest>(cancellationToken: cancellationToken);

        if (payload is null)
        {
            return await request.CreateErrorResponseAsync(HttpStatusCode.BadRequest, ApiMessageCatalog.ValidationErrorCode, ApiMessageCatalog.RequestBodyRequired, null, cancellationToken);
        }

        var correlationId = context.GetCorrelationId();

        var idempotencyKey = context.GetIdempotencyKey()
            ?? throw new InvalidOperationException(ApiMessageCatalog.IdempotencyKeyNotResolved);

        var result = await _mediator.Send(new CreateTransactionCommand(
            payload.AccountId,
            payload.Kind,
            payload.Amount,
            payload.Currency,
            payload.TransactionDateUtc,
            payload.Description,
            correlationId,
            idempotencyKey), cancellationToken);

        var response = await request.CreateJsonResponseAsync(HttpStatusCode.Created, result.Transaction.ToResponse(), cancellationToken);
        response.Headers.Add(IdempotencyConstants.IdempotencyReplayedHeaderName, result.IsIdempotentReplay.ToString().ToLowerInvariant());
        response.Headers.Add(IdempotencyConstants.IdempotencyHeaderName, idempotencyKey);

        _logger.CreateTransactionFunctionCompleted(result.Transaction.TransactionId, correlationId, result.IsIdempotentReplay);

        return response;
    }
}
