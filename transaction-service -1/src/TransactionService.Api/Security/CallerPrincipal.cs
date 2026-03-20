namespace TransactionService.Api.Security;

/// <summary>
/// Represents the authenticated caller information resolved from platform headers.
/// </summary>
public sealed record CallerPrincipal(
    string? AppId,
    string? TenantId,
    string? Audience,
    string? Issuer,
    IReadOnlyCollection<string> Roles,
    IReadOnlyDictionary<string, string[]> Claims);
