using Collections.Pooled;
using System;

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
    }
}