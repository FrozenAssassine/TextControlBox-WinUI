using Collections.Pooled;
using System;
using System.Collections.Generic;
using System.Linq;
using TextControlBoxNS.Helper;

namespace TextControlBoxNS.Extensions
{
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

        public static int GetLineLength(this PooledList<string> list, int index)
        {
            return GetLineText(list, index).Length;
        }

        public static void AddLine(this PooledList<string> list, string content = "")
        {
            list.Add(content);
        }
        public static void SetLineText(this PooledList<string> list, int line, string text)
        {
            if (line == -1)
                line = list.Count - 1;

            line = Math.Clamp(line, 0, list.Count - 1);

            list[line] = text;
        }
        public static string GetCurrentLineText(this PooledList<string> list)
        {
            return list[CurrentLineIndex < list.Count ? CurrentLineIndex : list.Count - 1 < 0 ? 0 : list.Count - 1];
        }
        public static void SetCurrentLineText(this PooledList<string> list, string text)
        {
            list[CurrentLineIndex < list.Count ? CurrentLineIndex : list.Count - 1] = text;
        }
        public static int CurrentLineLength(this PooledList<string> list)
        {
            return GetCurrentLineText(list).Length;
        }
        public static void UpdateCurrentLine(this PooledList<string> _, int currentLine)
        {
            CurrentLineIndex = currentLine;
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
        public static void InsertOrAddRange(this PooledList<string> list, IEnumerable<string> lines, int index)
        {
            if (index >= list.Count)
                list.AddRange(lines);
            else
                list.InsertRange(index < 0 ? 0 : index, lines);
        }
        public static void InsertNewLine(this PooledList<string> list, int index, string content = "")
        {
            list.InsertOrAdd(index, content);
        }
        public static void DeleteAt(this PooledList<string> list, int index)
        {
            if (index >= list.Count)
                index = list.Count - 1 < 0 ? list.Count - 1 : 0;

            list.RemoveAt(index);
            list.TrimExcess();
        }
        private static IEnumerable<string> GetLines_Small(this PooledList<string> totalLines, int start, int count)
        {
            var res = ListHelper.CheckValues(totalLines, start, count);

            for (int i = 0; i < res.Count; i++)
            {
                yield return totalLines[i + res.Index];
            }
        }

        public static IEnumerable<string> GetLines(this PooledList<string> totalLines, int start, int count)
        {
            //use different algorithm for small amount of lines:
            if (start + count < 400)
            {
                return GetLines_Small(totalLines, start, count);
            }

            return totalLines.Skip(start).Take(count);
        }

        public static string GetString(this IEnumerable<string> lines, string NewLineCharacter)
        {
            return string.Join(NewLineCharacter, lines);
        }

        public static void Safe_RemoveRange(this PooledList<string> totalLines, int index, int count)
        {
            var res = ListHelper.CheckValues(totalLines, index, count);
            totalLines.RemoveRange(res.Index, res.Count);
            totalLines.TrimExcess();

            //clear up the memory of the list when more than 1mio items are removed
            if (res.Count > 1_000_000)
                ListHelper.GCList(totalLines);
        }
        public static string[] GetLines(this string[] lines, int start, int count)
        {
            return lines.Skip(start).Take(count).ToArray();
        }

        public static void SwapLines(this PooledList<string> lines, int originalindex, int newindex)
        {
            string oldLine = lines.GetLineText(originalindex);
            lines.SetLineText(originalindex, lines.GetLineText(newindex));
            lines.SetLineText(newindex, oldLine);
        }
    }
}
