using Collections.Pooled;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Schema;
using TextControlBoxNS.Extensions;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Models.Enums;

namespace TextControlBoxNS.Core.Text;

internal class TextManager
{
    public PooledList<string> totalLines = new PooledList<string>(0);

    public int _FontSize = 18;
    public LineEnding _LineEnding = LineEnding.CRLF;
    public FontFamily _FontFamily = new FontFamily("Consolas");
    public string NewLineCharacter = "\r\n";
    public SyntaxHighlightLanguage _CodeLanguage = null;
    public int MaxFontsize = 125;
    public int MinFontSize = 3;
    public bool _IsReadonly = false;

    public int GetLineLength(int line)
    {
        return GetLineText(line).Length;
    }

    public int LinesCount => totalLines.Count;

    public string LineAt(int line) => totalLines[line];

    public string GetLineText(int line)
    {
        if (totalLines.Count == 0)
            return "";

        if (line == -1 && totalLines.Count > 0)
            return totalLines[totalLines.Count - 1];
        line = line >= totalLines.Count ? totalLines.Count - 1 : line > 0 ? line : 0;
        return totalLines[line];
    }

    public string GetLinesAsString()
    {
        //not directly faster than string.join, but 3 times more memory efficient:
        if (totalLines.Count == 0)
            return string.Empty;

        var builder = new System.Text.StringBuilder();

        var span = totalLines.Span;

        for (int i = 0; i < span.Length; i++)
        {
            builder.Append(span[i].AsSpan());
            builder.Append(NewLineCharacter.AsSpan());
        }

        if (builder.Length > 0)
            builder.Length -= NewLineCharacter.Length;

        return builder.ToString();
    }
    public string GetLinesAsString(int start, int count)
    {
        if (start < 0 || count < 0 || start >= totalLines.Count)
            return string.Empty;

        int end = Math.Min(totalLines.Count, start + count);

        var builder = new System.Text.StringBuilder();

        var span = totalLines.Span.Slice(start, end - start);

        foreach (var line in span)
        {
            builder.Append(line.AsSpan());
            builder.Append(NewLineCharacter.AsSpan());
        }

        if (builder.Length > 0)
            builder.Length -= NewLineCharacter.Length;

        return builder.ToString();
    }

    public void SetLineText(int line, string text)
    {
        if (line == -1)
            line = totalLines.Count - 1;

        line = Math.Clamp(line, 0, totalLines.Count - 1);

        totalLines.Span[line] = text;
    }
    public void String_AddToEnd(int line, string add)
    {
        totalLines.Span[line] += add;
    }
    public void DeleteAt(int index)
    {
        if (index >= totalLines.Count)
            index = totalLines.Count - 1 < 0 ? totalLines.Count - 1 : 0;

        totalLines.RemoveAt(index);
        totalLines.TrimExcess();
    }

    public void InsertOrAddRange(IEnumerable<string> lines, int index)
    {
        if (index >= totalLines.Count)
            totalLines.AddRange(lines);
        else
            totalLines.InsertRange(index < 0 ? 0 : index, lines);
    }
    public void InsertOrAdd(int index, string lineText)
    {
        if (index >= totalLines.Count || index == -1)
            totalLines.Add(lineText);
        else
            totalLines.Insert(index, lineText);
    }

    public void ClearText(bool addNewLine = false)
    {
        totalLines.Clear();
        ListHelper.GCList(totalLines);

        if (addNewLine)
            totalLines.Add("");
    }
    public void CleanUp()
    {
        Debug.WriteLine("Collect GC");
        ListHelper.GCList(totalLines);
    }
    public void Safe_RemoveRange(int index, int count)
    {
        var res = ListHelper.CheckValues(totalLines, index, count);
        totalLines.RemoveRange(res.Index, res.Count);
        totalLines.TrimExcess();

        //clear up the memory of the list when more than 1mio items are removed
        if (res.Count > 1_000_000)
            ListHelper.GCList(totalLines);
    }

    public IEnumerable<string> GetLines(int start, int count)
    {
        if (start < 0 || count < 0 || start >= totalLines.Count)
            yield break;

        int end = Math.Min(totalLines.Count, start + count);
        for (int i = start; i < end; i++)
        {
            yield return totalLines[i];
        }
    }

    public void AddLine(string content = "")
    {
        totalLines.Add(content);
    }
    public void SwapLines(int originalIndex, int newIndex)
    {
        if (originalIndex < 0 || originalIndex >= totalLines.Count ||
            newIndex < 0 || newIndex >= totalLines.Count)
            return;

        (totalLines[originalIndex], totalLines[newIndex]) = (totalLines[newIndex], totalLines[originalIndex]);
    }

    public int CountCharacters()
    {
        int count = 0;
        foreach (var line in totalLines)
        {
            count += line.AsSpan().Length + 1;
        }
        return count > 0 ? count - 1 : 0;
    }
}
