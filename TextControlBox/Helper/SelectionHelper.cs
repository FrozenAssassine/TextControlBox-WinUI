using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Core.Selection;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Models;
using Windows.Foundation;

namespace TextControlBoxNS.Helper;

internal class SelectionHelper
{
    //returns whether the pointer is over a selection
    public static bool PointerIsOverSelection(TextRenderer textRenderer, SelectionManager selectionManager, Point pointerPosition)
    {
        if (textRenderer.DrawnTextLayout == null || !selectionManager.HasSelection)
            return false;

        CanvasTextLayoutRegion[] regions = textRenderer.DrawnTextLayout.GetCharacterRegions(selectionManager.currentTextSelection.renderedIndex, selectionManager.currentTextSelection.renderedLength);
        for (int i = 0; i < regions.Length; i++)
        {
            if (regions[i].LayoutBounds.Contains(pointerPosition))
                return true;
        }
        return false;
    }

    public static bool CursorIsInSelection(SelectionManager selectionManager, CursorPosition cursorPosition)
    {
        var textSel = selectionManager.OrderTextSelectionSeparated();
        if (textSel.startNull && textSel.endNull)
            return false;

        //Cursorposition is smaller than the start of selection
        if (textSel.startLine > cursorPosition.LineNumber)
            return false;

        //Selectionend is smaller than Cursorposition -> not in selection
        if (textSel.endLine < cursorPosition.LineNumber)
            return false;

        //Selection-start line equals Cursor line:
        if (cursorPosition.LineNumber == textSel.startLine)
            return cursorPosition.CharacterPosition > textSel.startChar;

        //Selection-end line equals Cursor line
        else if (cursorPosition.LineNumber == textSel.endLine)
            return cursorPosition.CharacterPosition < textSel.endChar;
        return true;
    }
    public static bool TextIsSelected(CursorPosition start, CursorPosition end)
    {
        if (start.IsNull || end.IsNull)
            return false;

        return start.LineNumber != end.LineNumber ||
            start.CharacterPosition != end.CharacterPosition;
    }

    public static (bool startNull, bool endNull, int startLine, int startChar, int endLine, int endChar) OrderTextSelectionSeparated(TextSelection selection, bool? hasSelection = null)
    {
        //allow for passing whether the selection is null or not. Required for undo/redo and selectionmanager checking. They are differentiated
        if (hasSelection.HasValue ? !hasSelection.Value : !selection.HasSelection)
            return (true, true, -1, -1, -1, -1);

        if (selection.EndPosition.IsNull && !selection.StartPosition.IsNull)
            return (false, true, selection.StartPosition.LineNumber, selection.StartPosition.CharacterPosition, -1, -1);

        if (!selection.EndPosition.IsNull && selection.StartPosition.IsNull)
            return (false, true, selection.EndPosition.LineNumber, selection.EndPosition.CharacterPosition, -1, -1);

        int startLine = selection.GetMinLine();
        int endLine = selection.GetMaxLine();
        int startPosition;
        int endPosition;

        if (startLine == endLine)
        {
            startPosition = selection.GetMinChar();
            endPosition = selection.GetMaxChar();
        }
        else
        {
            if (selection.StartPosition.LineNumber < selection.EndPosition.LineNumber)
            {
                endPosition = selection.EndPosition.CharacterPosition;
                startPosition = selection.StartPosition.CharacterPosition;
            }
            else
            {
                endPosition = selection.StartPosition.CharacterPosition;
                startPosition = selection.EndPosition.CharacterPosition;
            }
        }

        return (false, false, startLine, startPosition, endLine, endPosition);
    }

    //returns whether the selection starts at character zero and ends
    //needs to pass any selection object, since undo/redo uses different textselection
    public static bool WholeLinesAreSelected(TextSelection selection, TextManager textManager)
    {
        if (!selection.HasSelection)
            return false;

        var sel = OrderTextSelectionSeparated(selection);
        if (sel.startNull && sel.endNull)
            return false;

        return sel.startChar == 0 && sel.endChar == textManager.GetLineLength(sel.endLine);
    }

    public static (int start, int end) GetWordBoundaries(string line, int characterPosition)
    {
        if (string.IsNullOrEmpty(line))
            return (0, 0);

        characterPosition = Math.Clamp(characterPosition, 0, line.Length);

        int targetIndex;
        if (characterPosition < line.Length && !char.IsWhiteSpace(line[characterPosition]))
        {
            targetIndex = characterPosition;
        }
        else if (characterPosition > 0 && !char.IsWhiteSpace(line[characterPosition - 1]))
        {
            targetIndex = characterPosition - 1;
        }
        else if (characterPosition < line.Length)
        {
            targetIndex = characterPosition;
        }
        else
        {
            targetIndex = Math.Max(0, line.Length - 1);
        }

        CharClass targetClass = CharClassHelper.GetCharClass(line[targetIndex]);
        int start = targetIndex;
        while (start > 0 && CharClassHelper.GetCharClass(line[start - 1]) == targetClass)
        {
            start--;
        }
        int end = targetIndex + 1;
        while (end < line.Length && CharClassHelper.GetCharClass(line[end]) == targetClass)
        {
            end++;
        }

        start = TextElementHelper.SnapToTextElementStart(line, start);
        end = TextElementHelper.SnapToTextElementEnd(line, end);

        return (start, end);
    }

    /// <summary>
    /// Calculates the character index range [renderedSelectionStart, renderedSelectionLength] within the
    /// currently rendered text layout for a given document selection.
    /// Supports CRLF, LF, and CR line ending formats with exact boundary mapping.
    /// </summary>
    public static (int startIndex, int length) CalculateRenderedSelectionIndices(
        int unrenderedLinesToRenderStart,
        int numberOfRenderedLines,
        int lineEndingLength,
        IReadOnlyList<string> lines,
        int startLine,
        int characterPosStart,
        int endLine,
        int characterPosEnd,
        int textLength)
    {
        if (numberOfRenderedLines <= 0 || lines == null || lines.Count == 0 || textLength <= 0)
            return (0, 0);

        // Normalize endpoints so (startLine, characterPosStart) <= (endLine, characterPosEnd)
        if (startLine > endLine || (startLine == endLine && characterPosStart > characterPosEnd))
        {
            (startLine, endLine) = (endLine, startLine);
            (characterPosStart, characterPosEnd) = (characterPosEnd, characterPosStart);
        }

        int linesCount = lines.Count;
        startLine = Math.Clamp(startLine, 0, linesCount - 1);
        endLine = Math.Clamp(endLine, 0, linesCount - 1);
        characterPosStart = Math.Clamp(characterPosStart, 0, lines[startLine].Length);
        characterPosEnd = Math.Clamp(characterPosEnd, 0, lines[endLine].Length);

        int lastRenderedLine = unrenderedLinesToRenderStart + numberOfRenderedLines - 1;

        // Selection completely outside visible range
        if (endLine < unrenderedLinesToRenderStart || startLine > lastRenderedLine)
            return (0, 0);

        int clampedStartLine = startLine;
        int clampedStartChar = characterPosStart;
        if (clampedStartLine < unrenderedLinesToRenderStart)
        {
            clampedStartLine = unrenderedLinesToRenderStart;
            clampedStartChar = 0;
        }

        int clampedEndLine = endLine;
        int clampedEndChar = characterPosEnd;
        if (clampedEndLine > lastRenderedLine)
        {
            clampedEndLine = lastRenderedLine;
            clampedEndChar = lines[clampedEndLine].Length;
        }

        int selStartIndex = 0;
        int selEndIndex = 0;

        if (clampedStartLine == clampedEndLine)
        {
            int lengthToLine = 0;
            for (int i = 0; i < clampedStartLine - unrenderedLinesToRenderStart; i++)
            {
                if (i < numberOfRenderedLines)
                {
                    lengthToLine += lines[unrenderedLinesToRenderStart + i].Length + lineEndingLength;
                }
            }

            int currentLineLen = lines[clampedStartLine].Length;
            selStartIndex = Math.Min(clampedStartChar, currentLineLen) + lengthToLine;
            selEndIndex = Math.Min(clampedEndChar, currentLineLen) + lengthToLine;
        }
        else
        {
            for (int i = 0; i < clampedStartLine - unrenderedLinesToRenderStart; i++)
            {
                if (i >= numberOfRenderedLines)
                    break;
                selStartIndex += lines[unrenderedLinesToRenderStart + i].Length + lineEndingLength;
            }
            int startLineLen = lines[clampedStartLine].Length;
            selStartIndex += Math.Min(clampedStartChar, startLineLen);

            for (int i = 0; i < clampedEndLine - unrenderedLinesToRenderStart; i++)
            {
                if (i >= numberOfRenderedLines)
                    break;
                selEndIndex += lines[unrenderedLinesToRenderStart + i].Length + lineEndingLength;
            }
            int endLineLen = lines[clampedEndLine].Length;
            selEndIndex += Math.Min(clampedEndChar, endLineLen);
        }

        int renderedSelectionStart = Math.Clamp(Math.Min(selStartIndex, selEndIndex), 0, textLength);
        int maxEnd = Math.Clamp(Math.Max(selStartIndex, selEndIndex), 0, textLength);
        int renderedSelectionLength = maxEnd - renderedSelectionStart;

        return (renderedSelectionStart, renderedSelectionLength);
    }

    /// <summary>
    /// Merges horizontal selection intervals on the same visual row.
    /// Prevents overlapping/double-blended semi-transparent selection boxes,
    /// eliminates duplicate zero-width Unicode mark artifacts, and ensures empty lines
    /// are drawn with a single clean indicator.
    /// </summary>
    public static List<(float startX, float endX)> MergeSelectionIntervals(
        IEnumerable<(float startX, float endX)> spans,
        float defaultEmptyLineWidth)
    {
        var spanList = spans?.ToList() ?? new List<(float, float)>();
        if (spanList.Count == 0)
            return new List<(float, float)>();

        bool allZeroWidth = true;
        for (int i = 0; i < spanList.Count; i++)
        {
            if (spanList[i].endX > spanList[i].startX + 0.001f)
            {
                allZeroWidth = false;
                break;
            }
        }

        if (allZeroWidth)
        {
            float minX = float.MaxValue;
            for (int i = 0; i < spanList.Count; i++)
            {
                if (spanList[i].startX < minX)
                    minX = spanList[i].startX;
            }
            if (minX == float.MaxValue)
                minX = 0;

            return new List<(float, float)> { (minX, minX + defaultEmptyLineWidth) };
        }

        var validSpans = new List<(float startX, float endX)>();
        for (int i = 0; i < spanList.Count; i++)
        {
            if (spanList[i].endX > spanList[i].startX + 0.001f)
            {
                validSpans.Add(spanList[i]);
            }
        }

        if (validSpans.Count == 0)
            return new List<(float, float)>();

        validSpans.Sort((a, b) => a.startX.CompareTo(b.startX));

        var result = new List<(float startX, float endX)>();
        float curStart = validSpans[0].startX;
        float curEnd = validSpans[0].endX;

        for (int i = 1; i < validSpans.Count; i++)
        {
            if (validSpans[i].startX <= curEnd + 0.5f)
            {
                if (validSpans[i].endX > curEnd)
                    curEnd = validSpans[i].endX;
            }
            else
            {
                result.Add((curStart, curEnd));
                curStart = validSpans[i].startX;
                curEnd = validSpans[i].endX;
            }
        }

        result.Add((curStart, curEnd));
        return result;
    }
}

