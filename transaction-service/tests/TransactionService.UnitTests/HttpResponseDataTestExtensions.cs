using Microsoft.Azure.Functions.Worker.Http;

public static class HttpResponseDataTestExtensions
{
    public static async Task<string> ReadBodyAsStringAsync(this HttpResponseData response)
    {
        response.Body.Position = 0;
        using var reader = new StreamReader(response.Body);
        return await reader.ReadToEndAsync();
    }
}
