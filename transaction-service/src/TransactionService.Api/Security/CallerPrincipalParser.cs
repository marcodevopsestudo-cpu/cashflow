using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;

namespace TransactionService.Api.Security;

/// <summary>
/// Provides helper methods to parse the authenticated caller principal
/// from headers propagated by Azure App Service Easy Auth.
/// </summary>
public static class CallerPrincipalParser
{
    private const string ClientPrincipalHeaderName = "X-MS-CLIENT-PRINCIPAL";

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Parses the caller principal from the current HTTP request.
    /// </summary>
    /// <param name="request">The incoming HTTP request.</param>
    /// <returns>
    /// A <see cref="CallerPrincipal"/> instance when the header is present and valid;
    /// otherwise, <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is null.
    /// </exception>
    public static CallerPrincipal? Parse(HttpRequestData request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!request.Headers.TryGetValues(ClientPrincipalHeaderName, out var values))
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

            payload = JsonSerializer.Deserialize<ClientPrincipalPayload>(json, JsonSerializerOptions);
        }
        catch
        {
            return null;
        }

        if (payload?.Claims is null || payload.Claims.Count == 0)
        {
            return null;
        }

        var normalizedClaims = payload.Claims
            .Select(c => new ParsedClaim(c.GetClaimType(), c.GetClaimValue()))
            .ToArray();

        string? GetClaim(params string[] names) =>
            normalizedClaims
                .FirstOrDefault(c => names.Contains(c.Type, StringComparer.OrdinalIgnoreCase))
                ?.Value;

        var appId = GetClaim("azp", "appid");
        var tenantId = GetClaim("tid");
        var audience = GetClaim("aud");
        var issuer = GetClaim("iss");

        var roles = normalizedClaims
            .Where(c => string.Equals(c.Type, "roles", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToArray();

        var scopeClaim = GetClaim("scp");

        var scopes = string.IsNullOrWhiteSpace(scopeClaim)
            ? Array.Empty<string>()
            : scopeClaim.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new CallerPrincipal(
            appId,
            tenantId,
            audience,
            issuer,
            roles,
            scopes);
    }

    /// <summary>
    /// Represents the deserialized Easy Auth client principal payload.
    /// </summary>
    private sealed class ClientPrincipalPayload
    {
        /// <summary>
        /// Gets or sets the identity provider associated with the authenticated principal.
        /// </summary>
        public string? IdentityProvider { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets additional user details.
        /// </summary>
        public string? UserDetails { get; set; }

        /// <summary>
        /// Gets or sets the collection of claims associated with the principal.
        /// </summary>
        public List<ClientPrincipalClaim> Claims { get; set; } = [];
    }

    /// <summary>
    /// Represents a raw claim entry from the Easy Auth client principal payload.
    /// </summary>
    private sealed class ClientPrincipalClaim
    {
        /// <summary>
        /// Gets or sets the claim type.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the claim value.
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Gets or sets the alternate claim type property used by some payload formats.
        /// </summary>
        public string? Typ { get; set; }

        /// <summary>
        /// Gets or sets the alternate claim value property used by some payload formats.
        /// </summary>
        public string? Val { get; set; }

        /// <summary>
        /// Returns the effective claim type, considering alternate payload formats.
        /// </summary>
        /// <returns>The resolved claim type, or an empty string when unavailable.</returns>
        public string GetClaimType() => Type ?? Typ ?? string.Empty;

        /// <summary>
        /// Returns the effective claim value, considering alternate payload formats.
        /// </summary>
        /// <returns>The resolved claim value, or an empty string when unavailable.</returns>
        public string GetClaimValue() => Value ?? Val ?? string.Empty;
    }

    /// <summary>
    /// Represents a normalized claim used internally during parsing.
    /// </summary>
    /// <param name="Type">The normalized claim type.</param>
    /// <param name="Value">The normalized claim value.</param>
    private sealed record ParsedClaim(string Type, string Value);
}
