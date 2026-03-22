namespace TransactionService.Api.Configuration;

/// <summary>
/// Represents configuration options for app-to-app authorization
/// using Microsoft Entra ID.
/// </summary>
/// <remarks>
/// This configuration defines which callers (applications) are allowed
/// to access the API based on their identity and claims.
/// </remarks>
public sealed class EntraAuthorizationOptions
{
    /// <summary>
    /// The configuration section name used to bind these options.
    /// </summary>
    public const string SectionName = "Authorization";

    /// <summary>
    /// Gets or sets a value indicating whether authorization is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the list of allowed application (client) IDs.
    /// </summary>
    /// <remarks>
    /// Only callers with a matching AppId will be authorized.
    /// </remarks>
    public string[] AllowedAppIds { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the list of valid audiences (aud claim).
    /// </summary>
    /// <remarks>
    /// The token audience must match one of these values.
    /// </remarks>
    public string[] AllowedAudiences { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the list of valid token issuers (iss claim).
    /// </summary>
    /// <remarks>
    /// The token issuer must match one of these values.
    /// </remarks>
    public string[] AllowedIssuers { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the required roles for authorization.
    /// </summary>
    /// <remarks>
    /// The caller must have at least one of these roles.
    /// </remarks>
    public string[] RequiredRoles { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the required scopes for authorization.
    /// </summary>
    /// <remarks>
    /// The caller must have at least one of these scopes.
    /// </remarks>
    public string[] RequiredScopes { get; set; } = Array.Empty<string>();
}
