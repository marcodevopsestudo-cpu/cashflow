using Microsoft.Azure.Functions.Worker;
using TransactionService.Api.Common.Constants;
using TransactionService.Api.Security;

namespace TransactionService.Api.Common.Extensions;

/// <summary>
/// Provides extension methods to store and retrieve authorization context
/// information for the current function invocation.
/// </summary>
public static class FunctionContextAuthorizationExtensions
{
    /// <summary>
    /// Stores the authenticated caller information in the <see cref="FunctionContext"/>,
    /// including AppId, TenantId, and roles.
    /// </summary>
    /// <param name="context">The current function execution context.</param>
    /// <param name="principal">The caller principal containing authorization data.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> or <paramref name="principal"/> is null.
    /// </exception>
    public static void SetCaller(this FunctionContext context, CallerPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(principal);

        if (!string.IsNullOrWhiteSpace(principal.AppId))
        {
            context.Items[AuthorizationConstants.CallerAppIdItemKey] = principal.AppId;
        }

        if (!string.IsNullOrWhiteSpace(principal.TenantId))
        {
            context.Items[AuthorizationConstants.CallerTenantIdItemKey] = principal.TenantId;
        }

        // Avoid multiple enumerations and ensure materialization
        context.Items[AuthorizationConstants.CallerRolesItemKey] =
            principal.Roles?.ToArray() ?? Array.Empty<string>();
    }

    /// <summary>
    /// Retrieves the caller AppId from the <see cref="FunctionContext"/>.
    /// </summary>
    /// <param name="context">The current function execution context.</param>
    /// <returns>The caller AppId if available; otherwise, <c>null</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> is null.
    /// </exception>
    public static string? GetCallerAppId(this FunctionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Items.TryGetValue(AuthorizationConstants.CallerAppIdItemKey, out var value)
            ? value as string
            : null;
    }

    /// <summary>
    /// Retrieves the caller TenantId from the <see cref="FunctionContext"/>.
    /// </summary>
    /// <param name="context">The current function execution context.</param>
    /// <returns>The caller TenantId if available; otherwise, <c>null</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> is null.
    /// </exception>
    public static string? GetCallerTenantId(this FunctionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Items.TryGetValue(AuthorizationConstants.CallerTenantIdItemKey, out var value)
            ? value as string
            : null;
    }
}
