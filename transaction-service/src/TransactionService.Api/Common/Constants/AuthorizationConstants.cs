namespace TransactionService.Api.Common.Constants;

/// <summary>
/// Authorization-related constants.
/// </summary>
public static class AuthorizationConstants
{
    /// <summary>
    /// Header name used by Azure to pass the authenticated client principal information.
    /// </summary>
    public const string ClientPrincipalHeaderName = "x-ms-client-principal";

    /// <summary>
    /// Key used to store or retrieve the caller application ID from the request context.
    /// </summary>
    public const string CallerAppIdItemKey = "CallerAppId";

    /// <summary>
    /// Key used to store or retrieve the caller tenant ID from the request context.
    /// </summary>
    public const string CallerTenantIdItemKey = "CallerTenantId";

    /// <summary>
    /// Key used to store or retrieve the caller roles from the request context.
    /// </summary>
    public const string CallerRolesItemKey = "CallerRoles";
}
