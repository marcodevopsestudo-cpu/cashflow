namespace TransactionService.Api.Security;

public sealed record CallerPrincipal(
    string? AppId,
    string? TenantId,
    string? Audience,
    string? Issuer,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Scopes);
