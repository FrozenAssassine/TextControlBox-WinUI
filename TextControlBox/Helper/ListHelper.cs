using Collections.Pooled;
using System;
using System.Linq;

namespace TextControlBoxNS.Helper
{
    internal class ListHelper
    {
        public struct ValueResult
        {
            public ValueResult(int index, int count)
            {
                this.Index = index;
                this.Count = count;
            }
            public int Index;
            public int Count;
        }
        public static ValueResult CheckValues(PooledList<string> totalLines, int index, int count)
        {
            if (index >= totalLines.Count)
            {
                index = totalLines.Count - 1 < 0 ? 0 : totalLines.Count - 1;
                count = 0;
            }
            if (index + count >= totalLines.Count)
            {
                int difference = totalLines.Count - index;
                if (difference >= 0)
                    count = difference;
            }

            if (count < 0)
                count = 0;
            if (index < 0)
                index = 0;

            return new ValueResult(index, count);
        }

        public static void GCList(PooledList<string> totalLines)
        {
            int id = GC.GetGeneration(totalLines);
            GC.Collect(id, GCCollectionMode.Forced);
        }

        public static void Clear(PooledList<string> totalLines, bool addNewLine = false)
        {
            totalLines.Clear();
            GCList(totalLines);

            if (addNewLine)
                totalLines.Add("");
        }

        public static string GetLinesAsString(PooledList<string> lines, string newLineCharacter)
        {
            return string.Join(newLineCharacter, lines);
        }
        public static string[] GetLinesFromString(string content, string newLineCharacter)
        {
            return content.Split(newLineCharacter);
        }
        public static string[] CreateLines(string[] lines, int start, string beginning, string end)
        {
            if (start > 0)
                lines = lines.Skip(start).ToArray();

            lines[0] = beginning + lines[0];
            if (lines.Length - 1 > 0)
                lines[lines.Length - 1] = lines[lines.Length - 1] + end;
            return lines;
        }
    }
}