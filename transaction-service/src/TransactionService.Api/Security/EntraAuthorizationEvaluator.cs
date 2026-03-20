using TransactionService.Api.Configuration;

namespace TransactionService.Api.Security;

/// <summary>
/// Evaluates whether an authenticated Entra caller is authorized to invoke the API.
/// </summary>
public sealed class EntraAuthorizationEvaluator
{
    public AuthorizationDecision Evaluate(CallerPrincipal? principal, EntraAuthorizationOptions options)
    {
        if (!options.Enabled)
        {
            return AuthorizationDecision.Allow();
        }

        if (principal is null)
        {
            return AuthorizationDecision.Deny(AuthorizationFailureReason.MissingPrincipal);
        }

        if (string.IsNullOrWhiteSpace(principal.AppId))
        {
            return AuthorizationDecision.Deny(AuthorizationFailureReason.MissingAppId);
        }

        if (options.AllowedAppIds.Length > 0 && !Contains(options.AllowedAppIds, principal.AppId))
        {
            return AuthorizationDecision.Deny(AuthorizationFailureReason.AppIdNotAllowed);
        }

        if (options.AllowedAudiences.Length > 0 && !Contains(options.AllowedAudiences, principal.Audience))
        {
            return AuthorizationDecision.Deny(AuthorizationFailureReason.InvalidAudience);
        }

        if (options.AllowedIssuers.Length > 0 && !Contains(options.AllowedIssuers, principal.Issuer))
        {
            return AuthorizationDecision.Deny(AuthorizationFailureReason.InvalidIssuer);
        }

        if (options.RequiredRoles.Length > 0 && !principal.Roles.Any(role => Contains(options.RequiredRoles, role)))
        {
            return AuthorizationDecision.Deny(AuthorizationFailureReason.MissingRequiredRole);
        }

        return AuthorizationDecision.Allow(principal);
    }

    private static bool Contains(IEnumerable<string> source, string? value)
        => !string.IsNullOrWhiteSpace(value) && source.Contains(value, StringComparer.OrdinalIgnoreCase);
}

public sealed record AuthorizationDecision(bool IsAllowed, AuthorizationFailureReason? FailureReason, CallerPrincipal? Principal)
{
    public static AuthorizationDecision Allow(CallerPrincipal? principal = null) => new(true, null, principal);
    public static AuthorizationDecision Deny(AuthorizationFailureReason reason) => new(false, reason, null);
}

public enum AuthorizationFailureReason
{
    MissingPrincipal,
    MissingAppId,
    AppIdNotAllowed,
    InvalidAudience,
    InvalidIssuer,
    MissingRequiredRole
}
