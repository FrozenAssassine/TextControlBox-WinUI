using System;
using System.Text.RegularExpressions;
using TextControlBox.Helper;

namespace TextControlBox.Extensions
{
    internal static class StringExtension
    {
        public static int IndexOfWholeWord(string Text, string Word, int StartIndex)
        {
            for (int j = StartIndex; j < Text.Length &&
                (j = Text.IndexOf(Word, j, StringComparison.Ordinal)) >= 0; j++)
            {
                if ((j == 0 || !char.IsLetterOrDigit(Text, j - 1)) &&
                    (j + Word.Length == Text.Length || !char.IsLetterOrDigit(Text, j + Word.Length)))
                {
                    return j;
                }
            }

            return -1;
        }
        public static int LastIndexOfWholeWord(string Text, string Word)
        {
            int StartIndex = Text.Length - 1;
            while (StartIndex >= 0 && (StartIndex = Text.LastIndexOf(Word, StartIndex, StringComparison.Ordinal)) != -1)
            {
                if (StartIndex > 0)
                {
                    if (!char.IsLetterOrDigit(Text[StartIndex - 1]))
                    {
                        return StartIndex;
                    }
                }

                if (StartIndex + Text.Length < Text.Length)
                {
                    if (!char.IsLetterOrDigit(Text[StartIndex + Word.Length]))
                    {
                        return StartIndex;
                    }
                }

                StartIndex--;
            }
            return -1;
        }
        public static string RemoveFirstOccurence(this string value, string removeString)
        {
            int index = value.IndexOf(removeString, StringComparison.Ordinal);
            return index < 0 ? value : value.Remove(index, removeString.Length);
        }

        public static string AddToEnd(this string text, string add)
        {
            return text + add;
        }
        public static string AddToStart(this string text, string add)
        {
            return add + text;
        }
        public static string AddText(this string text, string add, int position)
        {
            if (position < 0)
                position = 0;

            if (position >= text.Length || text.Length <= 0)
                return text + add;
            else
                return text.Insert(position, add);
        }
        public static string SafeRemove(this string text, int start, int count = -1)
        {
            if (start >= text.Length || start < 0)
                return text;

            if (count <= -1)
                return text.Remove(start);
            else
                return text.Remove(start, count);
        }
        public static bool Contains(this string text, SearchParameter Parameter)
        {
            if (Parameter.WholeWord)
                return Regex.IsMatch(text, Parameter.SearchExpression, RegexOptions.Compiled);

            if (Parameter.MatchCase)
                return text.Contains(Parameter.Word, StringComparison.Ordinal);
            else
                return text.Contains(Parameter.Word, StringComparison.OrdinalIgnoreCase);
        }
        public static string Safe_Substring(this string text, int index, int count = -1)
        {
            if (index >= text.Length)
                return "";
            else if (count == -1)
                return text.Substring(index);
            else
                return text.Substring(index, count);
        }
    }
}
