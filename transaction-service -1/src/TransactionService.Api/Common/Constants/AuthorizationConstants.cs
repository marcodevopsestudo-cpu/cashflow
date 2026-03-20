namespace TransactionService.Api.Common.Constants;

/// <summary>
/// Authorization-related constants.
/// </summary>
public static class AuthorizationConstants
{
    public const string ClientPrincipalHeaderName = "x-ms-client-principal";
    public const string CallerAppIdItemKey = "CallerAppId";
    public const string CallerTenantIdItemKey = "CallerTenantId";
    public const string CallerRolesItemKey = "CallerRoles";
}
