using System;

namespace MccSoft.LowLevelPrimitives.Serialization.DateOnlyConverters;

public static class DateOnlyHelpers
{
    public static DateOnly ToDateOnly(this DateTime date)
    {
        return new DateOnly(date.Year, date.Month, date.Day);
    }

    public static DateTime ToDateTime(this DateOnly date)
    {
        var result = date.ToDateTime(TimeOnly.MinValue);
        return DateTime.SpecifyKind(result, DateTimeKind.Utc);
    }
}
