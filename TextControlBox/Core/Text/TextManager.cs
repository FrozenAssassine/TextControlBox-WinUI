using Collections.Pooled;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Models;

namespace TextControlBoxNS.Core.Text;

internal class TextManager
{
    private EventsManager eventsManager;

    public PooledList<string> totalLines = new PooledList<string>(0);

    public int _FontSize = 18;
    private LineEnding _LineEnding = LineEnding.CRLF;
    public LineEnding LineEnding 
    { 
        get => _LineEnding;
        set 
        {
            _LineEnding = value;
            eventsManager.CallLineEndingChanged(value);
            NewLineCharacter = LineEndings.LineEndingToString(value);
        }
    }

    public void Init(EventsManager eventsManager)
    {
        this.eventsManager = eventsManager;
    }

    public FontFamily _FontFamily = new FontFamily("Consolas");
    public string NewLineCharacter = "\r\n";
    public SyntaxHighlightLanguage _SyntaxHighlighting = null;
    public int MaxFontsize = 125;
    public int MinFontSize = 3;
    public bool _IsReadOnly = false;

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
            throw new IndexOutOfRangeException("GetLineText provided line index out of range of valid values.");

        return totalLines[line];
    }

    public string GetLinesAsString()
    {
        if (totalLines.Count == 1 && totalLines[0].Length == 0)
            return "";
        
        return string.Join(NewLineCharacter, totalLines);
    }
    public string GetLinesAsString(int start, int count)
    {
        if (start < 0 || count < 0)
            throw new IndexOutOfRangeException("GetLinesAsString start or count less then zero");

        if (start + count == 0)
            return "";

        if (start == 0 && count >= totalLines.Count)
            return GetLinesAsString();

        if (start + count > totalLines.Count)
            throw new IndexOutOfRangeException("GetLinesAsString start + count is out of range of the size of the collection");

        return string.Join(NewLineCharacter, totalLines.Span.Slice(start, count).ToArray());
    }

    public LineSliceResult GetLinesForRendering(int start, int count)
    {
        if (start < 0 || count < 0 || start + count > totalLines.Count)
            return new LineSliceResult(string.Empty, ReadOnlySpan<string>.Empty);

        ReadOnlySpan<string> linesSlice = totalLines.Span.Slice(start, count);
        string joinedText = string.Join(NewLineCharacter, linesSlice);

        return new LineSliceResult(joinedText, linesSlice);
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
            throw new IndexOutOfRangeException("SetLineText provided line index out of range of valid values.");

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
        int lineEndingLength = LineEndings.LineEndingToString(this.LineEnding).Length;

        for (int i = 0; i < totalLines.Count; i++)
        {
            count += totalLines.Span[i].Length;

            //add line ending for all lines except the last
            if (i < totalLines.Count - 1)
                count += lineEndingLength;
        }

        return count;
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

    public int GetGlobalIndex(int line, int characterPosition, bool includeLineBreak = false)
    {
        if (LinesCount == 0)
            return 0;

        line = Math.Clamp(line, 0, LinesCount - 1);
        int lineLength = GetLineLength(line);
        characterPosition = Math.Clamp(characterPosition, 0, lineLength);

        int index = 0;
        int lineEndingLength = NewLineCharacter.Length;
        for (int i = 0; i < line; i++)
        {
            index += totalLines[i].Length + lineEndingLength;
        }

        index += characterPosition;

        if (includeLineBreak && characterPosition >= lineLength && line < LinesCount - 1)
            index += lineEndingLength;

        return index;
    }

    public bool TryGetVisualPosition(int line, int characterPosition, VisualLineMap visualLineMap, out int visualLine, out int column)
    {
        visualLine = -1;
        column = 0;

        if (visualLineMap == null || visualLineMap.TotalVisualLines == 0)
            return false;

        int globalIndex = GetGlobalIndex(line, characterPosition);
        return visualLineMap.TryGetVisualPosition(globalIndex, out visualLine, out column);
    }

    public bool TryGetGlobalIndexFromVisualPosition(int visualLineIndex, int column, VisualLineMap visualLineMap, out int globalIndex)
    {
        globalIndex = 0;

        if (visualLineMap == null || visualLineMap.TotalVisualLines == 0)
            return false;

        if (!visualLineMap.TryGetLogicalPosition(visualLineIndex, column, out int logicalLine, out int character))
            return false;

        globalIndex = GetGlobalIndex(logicalLine, character);
        return true;
    }

    public (int line, int character) GetLinePositionFromGlobalIndex(int index)
    {
        if (LinesCount == 0)
            return (0, 0);

        if (index <= 0)
            return (0, 0);

        int lineEndingLength = NewLineCharacter.Length;
        int currentIndex = 0;

        for (int i = 0; i < LinesCount; i++)
        {
            int lineLength = totalLines[i].Length;
            int lineTotal = lineLength + (i < LinesCount - 1 ? lineEndingLength : 0);

            if (index <= currentIndex + lineLength)
                return (i, Math.Clamp(index - currentIndex, 0, lineLength));

            if (index < currentIndex + lineTotal)
                return (i, lineLength);

            currentIndex += lineTotal;
        }

        int lastLine = LinesCount - 1;
        return (lastLine, totalLines[lastLine].Length);
    }
}
