#nullable enable
using System;
using System.Linq.Expressions;
using NeinLinq;

namespace MccSoft.LowLevelPrimitives.Extensions;

public static class DateTimeExtensions
{
    [InjectLambda]
    public static DateTime AsUtc(this DateTime dateTime) =>
        DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

    // ReSharper disable once UnusedMember.Local
    private static Expression<Func<DateTime, DateTime>> AsUtcExpr =>
        static dateTime => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
}
