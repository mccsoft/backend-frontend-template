#nullable enable
using System;

namespace MccSoft.LowLevelPrimitives.Extensions;

public static class DateTimeExtensions
{
    public static DateTime AsUtc(this DateTime dateTime) =>
        DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
}
