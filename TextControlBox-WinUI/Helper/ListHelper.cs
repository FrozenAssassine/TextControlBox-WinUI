using Collections.Pooled;
using System;
using System.Linq;

namespace TextControlBox.Helper
{
    internal class ListHelper
    {
        public struct ValueResult
        {
            public ValueResult(int Index, int Count)
            {
                this.Index = Index;
                this.Count = Count;
            }
            public int Index;
            public int Count;
        }
        public static ValueResult CheckValues(PooledList<string> TotalLines, int Index, int Count)
        {
            if (Index >= TotalLines.Count)
            {
                Index = TotalLines.Count - 1 < 0 ? 0 : TotalLines.Count - 1;
                Count = 0;
            }
            if (Index + Count >= TotalLines.Count)
            {
                int difference = TotalLines.Count - Index;
                if (difference >= 0)
                    Count = difference;
            }

            if (Count < 0)
                Count = 0;
            if (Index < 0)
                Index = 0;

            return new ValueResult(Index, Count);
        }

        public static void GCList(PooledList<string> TotalLines)
        {
            int identificador = GC.GetGeneration(TotalLines);
            GC.Collect(identificador, GCCollectionMode.Forced);
        }

        public static void Clear(PooledList<string> TotalLines, bool AddNewLine = false)
        {
            TotalLines.Clear();
            GCList(TotalLines);
            if (AddNewLine)
            {
                TotalLines.Add("");
            }
        }

        public static PooledList<string> GetLines(PooledList<string> TotalLines, int Index, int Count)
        {
            var res = CheckValues(TotalLines, Index, Count);
            return TotalLines.Skip(res.Index).Take(res.Count).ToPooledList();
        }
        public static string GetLinesAsString(PooledList<string> Lines, string NewLineCharacter)
        {
            return string.Join(NewLineCharacter, Lines);
        }
        public static string[] GetLinesFromString(string content, string NewLineCharacter)
        {
            return content.Split(NewLineCharacter);
        }
        public static string[] CreateLines(string[] lines, int Start, string Beginning, string End)
        {
            if (Start > 0)
                lines = lines.Skip(Start).ToArray();

            lines[0] = Beginning + lines[0];
            if (lines.Length - 1 > 0)
                lines[lines.Length - 1] = lines[lines.Length - 1] + End;
            return lines;
        }
    }
}