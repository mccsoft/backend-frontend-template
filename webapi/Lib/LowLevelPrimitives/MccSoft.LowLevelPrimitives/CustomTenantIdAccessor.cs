using System;
using System.Threading;

namespace MccSoft.LowLevelPrimitives;

public class CustomTenantIdAccessor
{
    public const string TenantIdClaim = "tenant";

    /// <summary>
    /// We store last created tenant ID to set it up for every new created entity in case if Http context for current operation does not exist. I.e. for data seeding
    /// </summary>
    private static AsyncLocal<bool?> _ignoreTenantIdQueryFilter = new();

    /// <summary>
    /// Disables TenantId filter for SELECT requests
    /// Enables it back when returned disposable is disposed.
    /// Useful e.g. during Login procedure, where you need to find the user across tenants.
    /// </summary>
    /// <returns>
    /// IDisposable, that enables filter back to the original value when disposed
    ///!!!DON'T FORGET TO DISPOSE THE RETURNED DISPOSABLE (preferably via wrapping it in `using` statement)!!!
    /// </returns>
    public static IDisposable IgnoreTenantIdQueryFilter()
    {
        bool? oldIgnoreTenantIdQueryFilterValue = _ignoreTenantIdQueryFilter.Value;
        _ignoreTenantIdQueryFilter.Value = true;

        return new Disposable(() =>
        {
            _ignoreTenantIdQueryFilter.Value = oldIgnoreTenantIdQueryFilterValue;
        });
    }

    /// <summary>
    /// Returns true if TenantId query filter should be disabled
    /// </summary>
    public static bool IsTenantIdQueryFilterDisabled => _ignoreTenantIdQueryFilter.Value == true;

    /// <summary>
    /// Sets the current tenant id to the <paramref name="tenantId"/>.
    /// Resets the value of TenantId back when returned disposable is disposed.
    /// Useful when http context with user's tenant is unavailable.
    /// </summary>
    /// <returns>
    /// IDisposable, that resets the tenant back to the original value when disposed
    ///!!!DON'T FORGET TO DISPOSE THE RETURNED DISPOSABLE!!!
    /// </returns>
    public static IDisposable SetCustomTenantId(int tenantId)
    {
        int? oldCustomTenantId = _customTenant.Value;
        _customTenant.Value = tenantId;

        return new Disposable(() =>
        {
            _customTenant.Value = oldCustomTenantId;
        });
    }

    /// <summary>
    /// We store last created tenant ID to set it up for every new created entity in case if Http context for current operation does not exist. I.e. for data seeding
    /// </summary>
    private static AsyncLocal<int?> _customTenant = new AsyncLocal<int?>();

    /// <summary>
    /// Get stored custom tenant id in case if http context with user's tenant is unavailable
    /// </summary>
    public static int? GetCustomTenantId()
    {
        return _customTenant.Value;
    }
}
