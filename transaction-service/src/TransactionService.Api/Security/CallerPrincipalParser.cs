using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;
using TransactionService.Api.Common.Constants;

namespace TransactionService.Api.Security;

/// <summary>
/// Parses caller identity data propagated by App Service Authentication / Easy Auth.
/// </summary>
public static class CallerPrincipalParser
{
    public static CallerPrincipal? Parse(HttpRequestData request)
    {
        if (!request.Headers.TryGetValues(AuthorizationConstants.ClientPrincipalHeaderName, out var headerValues))
        {
            return null;
        }

        var encodedValue = headerValues.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(encodedValue))
        {
            return null;
        }

        var json = DecodeBase64(encodedValue);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("claims", out var claimsElement) || claimsElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var claims = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var claimElement in claimsElement.EnumerateArray())
        {
            if (!claimElement.TryGetProperty("typ", out var typeElement) || !claimElement.TryGetProperty("val", out var valueElement))
            {
                continue;
            }

            var type = typeElement.GetString();
            var value = valueElement.GetString();

            if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (!claims.TryGetValue(type, out var values))
            {
                values = [];
                claims[type] = values;
            }

            values.Add(value);
        }

        string? FindFirst(params string[] claimTypes)
        {
            foreach (var claimType in claimTypes)
            {
                if (claims.TryGetValue(claimType, out var values) && values.Count > 0)
                {
                    return values[0];
                }
            }

            return null;
        }

        var roles = claims.TryGetValue("roles", out var roleValues)
            ? roleValues.ToArray()
            : Array.Empty<string>();

        return new CallerPrincipal(
            FindFirst("appid", "azp"),
            FindFirst("tid"),
            FindFirst("aud"),
            FindFirst("iss"),
            roles,
            claims.ToDictionary(static x => x.Key, static x => x.Value.ToArray(), StringComparer.OrdinalIgnoreCase));
    }

    private static string? DecodeBase64(string value)
    {
        try
        {
            var bytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(bytes);
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
