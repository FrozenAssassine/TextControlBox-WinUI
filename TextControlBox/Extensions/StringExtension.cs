using System;
using System.Text.RegularExpressions;
using TextControlBoxNS.Models;

namespace TextControlBoxNS.Extensions
{
    internal static class StringExtension
    {
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
            {
                if (count == text.Length)
                    return text.Remove(start);
                return text.Remove(start, count);
            }
        }
        public static bool Contains(this string text, SearchParameter parameter)
        {
            if (parameter.WholeWord)
                return Regex.IsMatch(text, parameter.SearchExpression, RegexOptions.Compiled);

            if (parameter.MatchCase)
                return text.Contains(parameter.Word, StringComparison.Ordinal);
            else
                return text.Contains(parameter.Word, StringComparison.OrdinalIgnoreCase);
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

        public static string[] SplitAt(this string text, int index)
        {
            string first = index < text.Length ? text.SafeRemove(index) : text;
            string second = index < text.Length ? text.Safe_Substring(index) : "";
            return [first, second];
        }

        public static int CountLines(this string text, string newLineCharacter)
        {
            //is slower than normal string operation but consumes soo much less memory
            var span = text.AsSpan();
            int lineCount = 1;

            int newLineLength = newLineCharacter.Length;

            for (int i = 0; i <= span.Length - newLineLength; i++)
            {
                if (span.Slice(i, newLineLength).SequenceEqual(newLineCharacter.AsSpan()))
                {
                    lineCount++;
                    i += newLineLength - 1;
                }
            }

            return lineCount;
        }
    }
}
