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

        var correlationId = context.GetCorrelationId();

        var idempotencyKey = context.GetIdempotencyKey();

        var command = new CreateTransactionCommand(
            payload!.AccountId,
            payload.Kind,
            payload.Amount,
            payload.Currency,
            payload.TransactionDateUtc,
            payload.Description,
            correlationId!,
            idempotencyKey!);

        var result = await _mediator.Send(command, cancellationToken);

        var statusCode = result.IsIdempotentReplay
            ? HttpStatusCode.OK
            : HttpStatusCode.Created;



        var response = request.CreateResponse(statusCode);

        // Idempotency headers
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            response.Headers.Add(IdempotencyConstants.IdempotencyHeaderName, idempotencyKey);
        }

        if (result.IsIdempotentReplay)
        {
            response.Headers.Add(IdempotencyConstants.IdempotencyReplayedHeaderName, "true");
        }

        await response.WriteAsJsonAsync(result, statusCode, cancellationToken);

        _logger.CreateTransactionFunctionCompleted(result.Transaction.Id, correlationId, result.IsIdempotentReplay);
        _logger.LogInformation(
    "CreateTransaction response generated. TransactionId={TransactionId}, Replay={Replay}",
    result.Transaction.Id,
    result.IsIdempotentReplay);
        return response;
    }
}
