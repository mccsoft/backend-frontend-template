#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using NeinLinq;

namespace MccSoft.LowLevelPrimitives.Extensions;

public static class StringExtensions
{
    [InjectLambda]
    public static bool HasValue([NotNullWhen(true)] this string? value) =>
        !string.IsNullOrWhiteSpace(value);

    public static Expression<Func<string?, bool>> HasValueExpr { get; } =
        value => !string.IsNullOrWhiteSpace(value);

    /// <inheritdoc cref="string.IsNullOrWhiteSpace"/>
    [InjectLambda]
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value) =>
        string.IsNullOrWhiteSpace(value);

    public static Expression<Func<string?, bool>> IsNullOrWhiteSpaceExpr { get; } =
        value => string.IsNullOrWhiteSpace(value);

    /// <inheritdoc cref="string.IsNullOrEmpty"/>
    [InjectLambda]
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value) =>
        string.IsNullOrEmpty(value);

    public static Expression<Func<string?, bool>> IsNullOrEmptyExpr { get; } =
        value => string.IsNullOrEmpty(value);
}
