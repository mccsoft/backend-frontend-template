using System;

namespace MccSoft.LowLevelPrimitives;

/// <summary>
/// interface for getting identification information about current user
/// </summary>
public partial interface IUserAccessor
{
    /// <summary>
    /// returns Id of current user; throws if user is not authenticated or we are out of http request context
    /// </summary>
    string GetUserId();

    /// <summary>
    /// returns Id of a tenant of current user; throws if user is not authenticated or we are out of http request context
    /// </summary>
    int GetTenantId();

    /// <summary>
    /// returns true if we are in scope of http request; false otherwise
    /// </summary>
    bool IsHttpContextAvailable { get; }

    /// <summary>
    /// returns true if we are in scope of http request and user is authenticated; false otherwise
    /// </summary>
    bool IsUserAuthenticated { get; }

    /// <summary>
    /// Sets the current tenant id to the <paramref name="tenantId"/>.
    /// Resets the value of TenantId back when returned disposable is disposed.
    /// Useful when http context with user's tenant is unavailable.
    /// </summary>
    /// <returns>
    /// IDisposable, that resets the tenant back to the original value when disposed
    ///!!!DON'T FORGET TO DISPOSE THE RETURNED DISPOSABLE!!!
    /// </returns>
    IDisposable SetCustomTenantId(int tenantId) =>
        CustomTenantIdAccessor.SetCustomTenantId(tenantId);

    int? GetCustomTenantId() => CustomTenantIdAccessor.GetCustomTenantId();
}
