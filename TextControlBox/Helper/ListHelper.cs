using Collections.Pooled;
using System;

namespace TextControlBoxNS.Helper;

internal class ListHelper
{
    public static void GCList(PooledList<string> totalLines)
    {
        int id = GC.GetGeneration(totalLines);
        GC.Collect(id, GCCollectionMode.Forced);
    }

    public static string[] GetLinesFromString(string content, string newLineCharacter)
    {
        return content.Split(newLineCharacter);
    }
    public static string[] CreateLines(string[] lines, int start, string beginning, string end)
    {
        int length = lines.Length - start;
        if (length <= 0) return Array.Empty<string>();

        string[] result = new string[length];

        Array.Copy(lines, start, result, 0, length);

        result[0] = beginning + result[0];
        if (length > 1)
            result[length - 1] += end;

        return result;
    }
    public static string[] GetLines(string[] lines, int start, int count)
    {
        ReadOnlySpan<string> span = lines.AsSpan(start, count);
        return span.ToArray();
    }
}