using Microsoft.Azure.Functions.Worker.Http;

/// <summary>
/// Provides helper extension methods for <see cref="HttpResponseData"/> used in tests.
/// </summary>
public static class HttpResponseDataTestExtensions
{
    /// <summary>
    /// Reads the response body stream and returns its content as a string.
    /// </summary>
    /// <param name="response">The HTTP response data.</param>
    /// <returns>The response body as a string.</returns>
    public static async Task<string> ReadBodyAsStringAsync(this HttpResponseData response)
    {
        response.Body.Position = 0;
        using var reader = new StreamReader(response.Body);
        return await reader.ReadToEndAsync();
    }
}
