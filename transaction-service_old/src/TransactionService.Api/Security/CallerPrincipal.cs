namespace TransactionService.Api.Security;

/// <summary>
/// Represents the authenticated caller extracted from the incoming request,
/// typically based on Microsoft Entra ID claims.
/// </summary>
/// <remarks>
/// This model encapsulates identity and authorization-related claims used
/// during authorization evaluation.
/// </remarks>
/// <param name="AppId">The application (client) identifier (appid/azp claim).</param>
/// <param name="TenantId">The tenant identifier (tid claim).</param>
/// <param name="Audience">The token audience (aud claim).</param>
/// <param name="Issuer">The token issuer (iss claim).</param>
/// <param name="Roles">The roles assigned to the caller (roles claim).</param>
/// <param name="Scopes">The scopes granted to the caller (scp claim).</param>
public sealed record CallerPrincipal(
    string? AppId,
    string? TenantId,
    string? Audience,
    string? Issuer,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Scopes)
{
    /// <summary>
    /// Gets an empty <see cref="CallerPrincipal"/> instance.
    /// </summary>
    public static CallerPrincipal Empty { get; } = new(
        null,
        null,
        null,
        null,
        [],
        []);
}
