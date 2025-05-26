using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MccSoft.DomainHelpers;

public static class EfCoreExtensions
{
    // https://learn.microsoft.com/en-us/ef/core/miscellaneous/nullable-reference-types#required-navigation-properties
    [StackTraceHidden]
    public static T ThrowIfNotIncluded<T>(
        [NotNull] this T? backingField,
        [CallerMemberName] string navigationPropName = ""
    )
        where T : class
    {
        return backingField
            ?? throw new NullReferenceException($"{navigationPropName} was not included");
    }
}
