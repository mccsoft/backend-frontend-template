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

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.OrderBy(_ => Random.Shared.Next());
    }
}
