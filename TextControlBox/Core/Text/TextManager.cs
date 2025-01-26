using Collections.Pooled;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TextControlBoxNS.Helper;

namespace TextControlBoxNS.Core.Text;

internal class TextManager
{
    public PooledList<string> totalLines = new PooledList<string>(0);

    public int _FontSize = 18;
    public LineEnding _LineEnding = LineEnding.CRLF;
    public FontFamily _FontFamily = new FontFamily("Consolas");
    public string NewLineCharacter = "\r\n";
    public SyntaxHighlightLanguage _SyntaxHighlighting = null;
    public int MaxFontsize = 125;
    public int MinFontSize = 3;
    public bool _IsReadonly = false;

    public int GetLineLength(int line)
    {
        return GetLineText(line).Length;
    }

    public int LinesCount => totalLines.Count;

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
        return string.Join(NewLineCharacter, totalLines);
    }
    public string GetLinesAsString(int start, int count)
    {
        if (start == 0 && count >= totalLines.Count)
            return GetLinesAsString();

        if (start + count >= totalLines.Count)
            return string.Join(NewLineCharacter, totalLines.Skip(start));

        return string.Join(NewLineCharacter, totalLines.Skip(start).Take(count));
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
    public void String_AddToStart(int line, string add)
    {
        totalLines[line] = add + totalLines[line];
    }

    public void DeleteAt(int index)
    {
        if (index >= totalLines.Count)
            index = totalLines.Count - 1 < 0 ? totalLines.Count - 1 : 0;

        totalLines.RemoveAt(index);
    }

    public void InsertOrAddRange(IEnumerable<string> lines, int index)
    {
        if (index >= totalLines.Count)
            totalLines.AddRange(lines);
        else
        {
            var lineList = lines as IList<string> ?? lines.ToList();
            totalLines.Capacity = Math.Max(totalLines.Count + lineList.Count, totalLines.Capacity);
            totalLines.InsertRange(index < 0 ? 0 : index, lineList);
        }
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
        foreach (var line in totalLines.Span)
        {
            count += line.Length + 1;
        }
        return count > 0 ? count - 1 : 0;
    }
}
