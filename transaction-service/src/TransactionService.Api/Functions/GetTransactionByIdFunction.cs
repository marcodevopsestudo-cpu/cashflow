using System.Net;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using TransactionService.Api.Common.Extensions;
using TransactionService.Application.Transactions.Queries.GetTransactionById;

namespace TransactionService.Api.Functions;

/// <summary>
/// Handles the get transaction by id endpoint.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetTransactionByIdFunction"/> class.
/// </remarks>
/// <param name="mediator">The mediator.</param>
public sealed class GetTransactionByIdFunction(IMediator mediator)
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Executes the endpoint.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The HTTP response.</returns>
    [Function(nameof(GetTransactionByIdFunction))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "transactions/{transactionId:guid}")] HttpRequestData request,
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTransactionByIdQuery(transactionId), cancellationToken);
        return await request.CreateJsonResponseAsync(HttpStatusCode.OK, result.ToResponse(), cancellationToken);
    }
}
