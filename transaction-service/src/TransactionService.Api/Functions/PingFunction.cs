using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace TransactionService.Api.Functions;

/// <summary>
/// Provides a simple health check endpoint to verify that the service is running.
/// </summary>
public sealed class PingFunction
{
    private const string ResponseMessage = "pong";

    /// <summary>
    /// Handles HTTP GET requests to the "ping" endpoint and returns a simple response.
    /// </summary>
    /// <param name="request">The incoming HTTP request.</param>
    /// <returns>
    /// An <see cref="HttpResponseData"/> with status code 200 (OK) and a "pong" message.
    /// </returns>
    [Function("PingFunction")]
    public HttpResponseData Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ping")] HttpRequestData request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = request.CreateResponse(HttpStatusCode.OK);
        response.WriteStringAsync(ResponseMessage);

        return response;
    }
}
