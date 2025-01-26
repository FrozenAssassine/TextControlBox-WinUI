using System;

namespace TextControlBoxNS.Extensions;

internal static class ListExtension
{
    public static string[] GetLines(this string[] lines, int start, int count)
    {
        ReadOnlySpan<string> span = lines.AsSpan(start, count);
        return span.ToArray();
    }
}
