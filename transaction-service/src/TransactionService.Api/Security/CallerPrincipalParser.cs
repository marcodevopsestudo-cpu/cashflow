using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;

namespace TransactionService.Api.Security;

public static class CallerPrincipalParser
{
    public static CallerPrincipal? Parse(HttpRequestData request)
    {
        if (!request.Headers.TryGetValues("X-MS-CLIENT-PRINCIPAL", out var values))
        {
            return null;
        }

        var encoded = values.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(encoded))
        {
            return null;
        }

        ClientPrincipalPayload? payload;

        try
        {
            var decodedBytes = Convert.FromBase64String(encoded);
            var json = Encoding.UTF8.GetString(decodedBytes);

            payload = JsonSerializer.Deserialize<ClientPrincipalPayload>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch
        {
            return null;
        }

        if (payload?.Claims is null || payload.Claims.Count == 0)
        {
            return null;
        }

        string? GetClaim(params string[] names) =>
            payload.Claims
                .FirstOrDefault(c => names.Contains(c.Type, StringComparer.OrdinalIgnoreCase))
                ?.Value;

        var appId = GetClaim("azp", "appid");
        var tenantId = GetClaim("tid");
        var audience = GetClaim("aud");
        var issuer = GetClaim("iss");

        var roles = payload.Claims
            .Where(c => string.Equals(c.Type, "roles", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToArray();

        var scp = GetClaim("scp");

        var scopes = string.IsNullOrWhiteSpace(scp)
            ? Array.Empty<string>()
            : scp.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new CallerPrincipal(
            appId,
            tenantId,
            audience,
            issuer,
            roles,
            scopes);
    }

    private sealed class ClientPrincipalPayload
    {
        public string? IdentityProvider { get; set; }
        public string? UserId { get; set; }
        public string? UserDetails { get; set; }
        public List<ClientPrincipalClaim> Claims { get; set; } = [];
    }

    private sealed class ClientPrincipalClaim
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
