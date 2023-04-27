#nullable enable
using System;

namespace MccSoft.LowLevelPrimitives.Extensions;

public static class RandomExtensions
{
    private const string _alphabet = "0123456789ABCDEF";

    public static string GetString(this Random random, int length = 10, string? alphabet = null)
    {
        alphabet ??= _alphabet;
        return string.Create(
            length,
            null as object,
            (span, _) =>
            {
                for (var i = 0; i < span.Length; i++)
                    span[i] = alphabet[random.Next(alphabet.Length)];
            }
        );
    }
}
