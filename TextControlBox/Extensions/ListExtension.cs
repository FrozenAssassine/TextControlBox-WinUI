using Collections.Pooled;
using System;
using System.Collections.Generic;

namespace TextControlBoxNS.Extensions;

internal static class ListExtension
{
    private static int CurrentLineIndex = 0;

    public static string GetLineText(this PooledList<string> list, int index)
    {
        if (list.Count == 0)
            return string.Empty;

        if (index == -1 && list.Count > 0)
            return list[^1];

        index = Math.Clamp(index, 0, list.Count - 1);
        return list.Span[index];
    }

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
