using Microsoft.Azure.Functions.Worker;
using TransactionService.Api.Common.Constants;
using TransactionService.Api.Security;

namespace TransactionService.Api.Common.Extensions;

/// <summary>
/// Stores authorization context information for the current invocation.
/// </summary>
public static class FunctionContextAuthorizationExtensions
{
    public static void SetCaller(this FunctionContext context, CallerPrincipal principal)
    {
        if (!string.IsNullOrWhiteSpace(principal.AppId))
        {
            context.Items[AuthorizationConstants.CallerAppIdItemKey] = principal.AppId;
        }

        if (!string.IsNullOrWhiteSpace(principal.TenantId))
        {
            context.Items[AuthorizationConstants.CallerTenantIdItemKey] = principal.TenantId;
        }

        context.Items[AuthorizationConstants.CallerRolesItemKey] = principal.Roles.ToArray();
    }

    public static string? GetCallerAppId(this FunctionContext context)
        => context.Items.TryGetValue(AuthorizationConstants.CallerAppIdItemKey, out var value) ? value as string : null;

    public static string? GetCallerTenantId(this FunctionContext context)
        => context.Items.TryGetValue(AuthorizationConstants.CallerTenantIdItemKey, out var value) ? value as string : null;
}
