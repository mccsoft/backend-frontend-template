#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace MccSoft.LowLevelPrimitives.Extensions;

public static class LinqExtensions
{
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var item in enumerable)
            action(item);
    }

    public static bool ContainsIgnoreCase<T>(
        this IEnumerable<T> enumerable,
        Func<T, string> comparisonParam,
        string value
    )
    {
        return enumerable.Select(comparisonParam).Any(x => x.ToLower() == value.ToLower());
    }

    public static bool ContainsIgnoreCase(this IEnumerable<string> enumerable, string value)
    {
        return enumerable.Any(x => x.ToLower() == value.ToLower());
    }
}
