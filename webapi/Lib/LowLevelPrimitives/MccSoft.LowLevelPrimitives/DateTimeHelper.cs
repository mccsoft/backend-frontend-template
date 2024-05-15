using System;
using System.Threading;

namespace MccSoft.LowLevelPrimitives;

/// <summary>
/// Helper to be able to override DateTime.UtcNow in tests, but avoid using/injecting IDateTimeProvider everywhere
/// (especially in Domain entities)
/// </summary>
public static class DateTimeHelper
{
    private static readonly AsyncLocal<DateTime?> _customDateTime = new();
    public static DateTime UtcNow => _customDateTime.Value ?? DateTime.UtcNow;

    public static Disposable SetCustomDateTime(DateTime dateTime)
    {
        var oldValue = _customDateTime.Value;
        _customDateTime.Value = dateTime;
        return new Disposable(() =>
        {
            _customDateTime.Value = oldValue;
        });
    }
}
