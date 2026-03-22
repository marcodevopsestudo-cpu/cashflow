namespace TransactionService.Api.Configuration;

/// <summary>
/// Configures app-to-app authorization for callers authenticated by Microsoft Entra ID.
/// </summary>
public sealed class EntraAuthorizationOptions
{
    public const string SectionName = "Authorization";

    public bool Enabled { get; set; }
    public string[] AllowedAppIds { get; set; } = [];
    public string[] AllowedAudiences { get; set; } = [];
    public string[] AllowedIssuers { get; set; } = [];
    public string[] RequiredRoles { get; set; } = [];
    public string[] RequiredScopes { get; set; } = [];
}
