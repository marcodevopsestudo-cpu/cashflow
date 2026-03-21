using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace TransactionService.Api.Functions;

public sealed class PingFunction
{
    [Function("PingFunction")]
    public HttpResponseData Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ping")] HttpRequestData request)
    {
        var response = request.CreateResponse(HttpStatusCode.OK);
        response.WriteString("pong");
        return response;
    }
}
