using Microsoft.Extensions.Logging;
using TransactionService.Api.Configuration;

namespace TransactionService.Api.Security;

/// <summary>
/// Evaluates whether an authenticated caller is authorized to access the API
/// based on the configured Microsoft Entra ID authorization options.
/// </summary>
public sealed class EntraAuthorizationEvaluator
{
  
    /// <summary>
    /// Evaluates the provided caller principal against the configured authorization rules.
    /// </summary>
    /// <param name="principal">The caller principal extracted from the incoming request.</param>
    /// <param name="options">The authorization options used to validate the caller.</param>
    /// <returns>
    /// An <see cref="AuthorizationDecision"/> indicating whether access is allowed
    /// and, if denied, the reason for the failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/> is null.
    /// </exception>
    public AuthorizationDecision Evaluate(CallerPrincipal? principal, EntraAuthorizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

      

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

        var hasRequiredRole =
            options.RequiredRoles.Length == 0 ||
            principal.Roles.Any(role => Contains(options.RequiredRoles, role));

        var hasRequiredScope =
            options.RequiredScopes.Length == 0 ||
            principal.Scopes.Any(scope => Contains(options.RequiredScopes, scope));

        if (!hasRequiredRole && !hasRequiredScope)
        {
            return AuthorizationDecision.Deny(AuthorizationFailureReason.MissingRequiredPermission);
        }

        return AuthorizationDecision.Allow(principal);
    }

    /// <summary>
    /// Determines whether the specified value exists in the source collection,
    /// using a case-insensitive comparison.
    /// </summary>
    /// <param name="source">The source collection to search.</param>
    /// <param name="value">The value to locate.</param>
    /// <returns>
    /// <c>true</c> when the value is not null or whitespace and exists in the collection;
    /// otherwise, <c>false</c>.
    /// </returns>
    private static bool Contains(IEnumerable<string> source, string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           source.Contains(value, StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Represents the result of an authorization evaluation.
/// </summary>
/// <param name="IsAllowed">Indicates whether the caller is authorized.</param>
/// <param name="FailureReason">The reason for denial when authorization fails.</param>
/// <param name="Principal">The authorized caller principal when authorization succeeds.</param>
public sealed record AuthorizationDecision(
    bool IsAllowed,
    AuthorizationFailureReason? FailureReason,
    CallerPrincipal? Principal)
{
    /// <summary>
    /// Creates an authorization decision representing a successful evaluation.
    /// </summary>
    /// <param name="principal">The authorized caller principal.</param>
    /// <returns>An allowed <see cref="AuthorizationDecision"/>.</returns>
    public static AuthorizationDecision Allow(CallerPrincipal? principal = null) => new(true, null, principal);

    /// <summary>
    /// Creates an authorization decision representing a failed evaluation.
    /// </summary>
    /// <param name="reason">The reason for the authorization failure.</param>
    /// <returns>A denied <see cref="AuthorizationDecision"/>.</returns>
    public static AuthorizationDecision Deny(AuthorizationFailureReason reason) => new(false, reason, null);
}

/// <summary>
/// Defines the possible reasons why authorization can fail.
/// </summary>
public enum AuthorizationFailureReason
{
    /// <summary>
    /// The request did not contain an authenticated caller principal.
    /// </summary>
    MissingPrincipal,

    /// <summary>
    /// The caller application identifier was missing from the claims.
    /// </summary>
    MissingAppId,

    /// <summary>
    /// The caller application identifier is not in the allowed list.
    /// </summary>
    AppIdNotAllowed,

    /// <summary>
    /// The token audience is not valid for this API.
    /// </summary>
    InvalidAudience,

    /// <summary>
    /// The token issuer is not valid for this API.
    /// </summary>
    InvalidIssuer,

    /// <summary>
    /// The caller does not have any of the required roles or scopes.
    /// </summary>
    MissingRequiredPermission
}
