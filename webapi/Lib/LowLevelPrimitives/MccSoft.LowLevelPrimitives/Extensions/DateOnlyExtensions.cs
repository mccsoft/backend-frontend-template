#nullable enable
using System;
using System.Linq.Expressions;
using NeinLinq;

namespace MccSoft.LowLevelPrimitives.Extensions;

public static class DateOnlyExtensions
{
    public static DateOnly Max(DateOnly first, DateOnly second) =>
        DateOnly.FromDayNumber(Math.Max(first.DayNumber, second.DayNumber));

    [InjectLambda]
    public static DateOnly ToDateOnly(this DateTime dateTime) => DateOnly.FromDateTime(dateTime);

    // ReSharper disable once UnusedMember.Local
    private static Expression<Func<DateTime, DateOnly>> ToDateOnlyExpr =>
        static dateTime => DateOnly.FromDateTime(dateTime);

    [InjectLambda]
    public static TimeSpan Sub(this DateOnly first, DateOnly second) =>
        TimeSpan.FromDays(first.DayNumber - second.DayNumber);

    // ReSharper disable once UnusedMember.Local
    private static Expression<Func<DateOnly, DateOnly, TimeSpan>> SubExpr =>
        static (first, second) =>
            first.ToDateTime(TimeOnly.MinValue) - second.ToDateTime(TimeOnly.MinValue);

    [InjectLambda]
    public static DateTime ToUtcDateTime(this DateOnly date) =>
        date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

    // ReSharper disable once UnusedMember.Local
    private static Expression<Func<DateOnly, DateTime>> ToUtcDateTimeExpr =>
        static date => date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
}
