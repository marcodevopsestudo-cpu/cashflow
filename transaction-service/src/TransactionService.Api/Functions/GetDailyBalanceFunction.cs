using System.Net;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using TransactionService.Api.Common.Extensions;
using TransactionService.Application.Transactions.Queries.GetDailyBalance;

namespace TransactionService.Api.Functions;

/// <summary>
/// Azure Function responsible for retrieving the daily balance for a given date.
/// </summary>
public sealed class GetDailyBalanceFunction(IMediator mediator)
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Executes the HTTP GET endpoint to retrieve the daily balance.
    /// </summary>
    /// <param name="request">The incoming HTTP request.</param>
    /// <param name="date">The date parameter from the route (expected format: yyyy-MM-dd).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An HTTP response containing the daily balance in JSON format.</returns>
    [Function(nameof(GetDailyBalanceFunction))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "daily-balance/{date}")] HttpRequestData request,
        string date,
        CancellationToken cancellationToken)
    {
        var parsedDate = DateOnly.Parse(date);

        var result = await _mediator.Send(
            new GetDailyBalanceQuery(parsedDate),
            cancellationToken);

        return await request.CreateJsonResponseAsync(
            HttpStatusCode.OK,
            result,
            cancellationToken);
    }
}
