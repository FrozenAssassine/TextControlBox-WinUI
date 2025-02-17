using Collections.Pooled;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
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
    private LineEnding _LineEnding = LineEnding.CRLF;
    public LineEnding LineEnding 
    { 
        get => _LineEnding;
        set 
        {
            _LineEnding = value; 
            NewLineCharacter = LineEndings.LineEndingToString(value);
        }
    }

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
        if (line == -1)
            return totalLines[^1];

        if (line >= totalLines.Count || line < 0)
            throw new ArgumentOutOfRangeException("GetLineText provided line index out of range of valid values.");

        return totalLines[line];
    }

    public string GetLinesAsString()
    {
        return string.Join(NewLineCharacter, totalLines);
    }
    public string GetLinesAsString(int start, int count)
    {
        if (start + count == 0)
            return "";

        if (start == 0 && count >= totalLines.Count)
            return GetLinesAsString();

        if (start + count > totalLines.Count)
            throw new ArgumentOutOfRangeException("GetLinesAsString start + count is out of range of the size of the collection");

        return string.Join(NewLineCharacter, totalLines.Skip(start).Take(count));
    }

    public void SetLineText(int line, string text)
    {
        //-1 is the last line:
        if (line == -1)
        {
            totalLines[^1] = text;
            return;
        }

        if (line >= totalLines.Count || line < 0)
            throw new ArgumentOutOfRangeException("SetLineText provided line index out of range of valid values.");

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
        if (index >= totalLines.Count || index < 0)
            throw new IndexOutOfRangeException("DeleteAt: provided index is out of range");
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
    public void RemoveRange(int index, int count)
    {
        if (index + count > totalLines.Count)
            throw new IndexOutOfRangeException("RemoveRange index + count out of range");

        totalLines.RemoveRange(index, count);
        totalLines.TrimExcess();

        //clear up the memory of the list if more than 1_000_000 items are removed
        if (count > 1_000_000)
            ListHelper.GCList(totalLines);
    }

    public void AddLine(string content = "")
    {
        totalLines.Add(content);
    }
    public bool SwapLines(int originalIndex, int newIndex)
    {
        if (originalIndex < 0 || originalIndex >= totalLines.Count ||
            newIndex < 0 || newIndex >= totalLines.Count)
            return false;

        (totalLines[originalIndex], totalLines[newIndex]) = (totalLines[newIndex], totalLines[originalIndex]);
        return true;
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

    public int CountWords()
    {
        int wordCount = 0;

        foreach (var line in totalLines)
        {
            var span = line.AsSpan();
            int index = 0;

            while (index < span.Length)
            {
                while (index < span.Length && char.IsWhiteSpace(span[index]))
                {
                    index++;
                }

                if (index < span.Length)
                {
                    wordCount++;
                }

                while (index < span.Length && !char.IsWhiteSpace(span[index]))
                {
                    index++;
                }
            }
        }

        return wordCount;
    }
}
