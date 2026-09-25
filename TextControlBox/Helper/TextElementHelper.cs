using System;
using System.Globalization;

namespace TextControlBoxNS.Helper;

internal static class TextElementHelper
{
    /// <summary>
    /// Returns the length (in UTF-16 code units) of the text element (grapheme cluster / surrogate pair)
    /// starting at <paramref name="index"/> in <paramref name="text"/>.
    /// </summary>
    public static int GetNextTextElementLength(string text, int index)
    {
        if (string.IsNullOrEmpty(text) || index < 0 || index >= text.Length)
            return 0;

        int len = StringInfo.GetNextTextElementLength(text, index);
        return len > 0 ? len : 1;
    }

    /// <summary>
    /// Returns the length (in UTF-16 code units) of the text element (grapheme cluster / surrogate pair)
    /// ending at <paramref name="index"/> in <paramref name="text"/>.
    /// </summary>
    public static int GetPreviousTextElementLength(string text, int index)
    {
        if (string.IsNullOrEmpty(text) || index <= 0)
            return 0;

        if (index > text.Length)
            index = text.Length;

        if (index == 1)
            return 1;

        // Search backward up to 64 code units to find the text element boundary
        int searchStart = Math.Max(0, index - 64);
        if (searchStart > 0 && char.IsLowSurrogate(text[searchStart]))
        {
            searchStart--;
        }

        int current = searchStart;
        int lastElementStart = current;
        while (current < index)
        {
            lastElementStart = current;
            int len = StringInfo.GetNextTextElementLength(text, current);
            if (len <= 0)
                len = 1;
            current += len;
        }

        int prevLen = index - lastElementStart;
        return prevLen > 0 ? prevLen : 1;
    }

    /// <summary>
    /// If <paramref name="index"/> falls inside a multi-code-unit text element (e.g. surrogate pair or grapheme cluster),
    /// returns the start index of that element. Otherwise returns <paramref name="index"/>.
    /// </summary>
    public static int SnapToTextElementStart(string text, int index)
    {
        if (string.IsNullOrEmpty(text) || index <= 0)
            return 0;

        if (index >= text.Length)
            return text.Length;

        int searchStart = Math.Max(0, index - 64);
        if (searchStart > 0 && char.IsLowSurrogate(text[searchStart]))
        {
            searchStart--;
        }

        int current = searchStart;
        while (current < index)
        {
            int len = StringInfo.GetNextTextElementLength(text, current);
            if (len <= 0)
                len = 1;

            if (current + len > index)
            {
                return current;
            }
            current += len;
        }

        return index;
    }

    /// <summary>
    /// If <paramref name="index"/> falls inside a multi-code-unit text element (e.g. surrogate pair or grapheme cluster),
    /// returns the end index of that element. Otherwise returns <paramref name="index"/>.
    /// </summary>
    public static int SnapToTextElementEnd(string text, int index)
    {
        if (string.IsNullOrEmpty(text) || index <= 0)
            return 0;

        if (index >= text.Length)
            return text.Length;

        int searchStart = Math.Max(0, index - 64);
        if (searchStart > 0 && char.IsLowSurrogate(text[searchStart]))
        {
            searchStart--;
        }

        int current = searchStart;
        while (current < index)
        {
            int len = StringInfo.GetNextTextElementLength(text, current);
            if (len <= 0)
                len = 1;

            if (current + len > index)
            {
                return current + len;
            }
            current += len;
        }

        return index;
    }
}
