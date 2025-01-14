using Collections.Pooled;
using System;
using System.Collections.Generic;
using System.Linq;
using TextControlBoxNS.Helper;

namespace TextControlBoxNS.Extensions;

internal static class ListExtension
{
    private static int CurrentLineIndex = 0;

    public static string GetLineText(this PooledList<string> list, int index)
    {
        if (list.Count == 0)
            return "";

        if (index == -1 && list.Count > 0)
            return list[list.Count - 1];

        index = index >= list.Count ? list.Count - 1 : index > 0 ? index : 0;
        return list[index];
    }

    public static void SetLineText(this PooledList<string> list, int line, string text)
    {
        if (line == -1)
            line = list.Count - 1;

        line = Math.Clamp(line, 0, list.Count - 1);

        list[line] = text;
    }
    public static void String_AddToEnd(this PooledList<string> list, int line, string add)
    {
        list[line] = list[line] + add;
    }
    public static void InsertOrAdd(this PooledList<string> list, int index, string lineText)
    {
        if (index >= list.Count || index == -1)
            list.Add(lineText);
        else
            list.Insert(index, lineText);
    }
    public static void InsertNewLine(this PooledList<string> list, int index, string content = "")
    {
        list.InsertOrAdd(index, content);
    }
    public static string GetString(this IEnumerable<string> lines, string NewLineCharacter)
    {
        return string.Join(NewLineCharacter, lines);
    }

    public static string[] GetLines(this string[] lines, int start, int count)
    {
        return lines.Skip(start).Take(count).ToArray();
    }
}
