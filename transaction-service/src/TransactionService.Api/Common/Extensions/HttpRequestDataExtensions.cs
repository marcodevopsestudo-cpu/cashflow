using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using TransactionService.Api.Contracts.Responses;

namespace TransactionService.Api.Common.Extensions;

/// <summary>
/// Provides helper methods for HTTP requests.
/// </summary>
public static class HttpRequestDataExtensions
{
    /// <summary>
    /// Creates a JSON response.
    /// </summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="request">The request.</param>
    /// <param name="statusCode">The status code.</param>
    /// <param name="payload">The payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The HTTP response.</returns>
    public static async Task<HttpResponseData> CreateJsonResponseAsync<T>(this HttpRequestData request, HttpStatusCode statusCode, T payload, CancellationToken cancellationToken)
    {
        var response = request.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(payload,statusCode, cancellationToken);
        return response;
    }

    /// <summary>
    /// Creates an error response.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="statusCode">The status code.</param>
    /// <param name="code">The error code.</param>
    /// <param name="message">The message.</param>
    /// <param name="correlationId">The correlation id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The HTTP response.</returns>
    public static Task<HttpResponseData> CreateErrorResponseAsync(this HttpRequestData request, HttpStatusCode statusCode, string code, string message, string? correlationId, CancellationToken cancellationToken)
        => request.CreateJsonResponseAsync(statusCode, new ErrorResponse(code, message, correlationId), cancellationToken);
}
