using Collections.Pooled;
using System;
using System.Collections.Generic;

namespace TextControlBoxNS.Extensions;

internal static class ListExtension
{
    public static string GetString(this IEnumerable<string> lines, string NewLineCharacter)
    {
        return string.Join(NewLineCharacter, lines);
    }

    public static string[] GetLines(this string[] lines, int start, int count)
    {
        ReadOnlySpan<string> span = lines.AsSpan(start, count);
        return span.ToArray();
    }
}
