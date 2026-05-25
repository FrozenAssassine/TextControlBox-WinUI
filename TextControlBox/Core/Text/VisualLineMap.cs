using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;

namespace TextControlBoxNS.Core.Text;

internal readonly struct VisualLineInfo
{
    public VisualLineInfo(int visualIndex, int logicalLineIndex, int startChar, int length, int layoutLength, int globalStartIndex, bool isContinuation)
    {
        VisualIndex = visualIndex;
        LogicalLineIndex = logicalLineIndex;
        StartChar = startChar;
        Length = length;
        LayoutLength = layoutLength;
        GlobalStartIndex = globalStartIndex;
        IsContinuation = isContinuation;
    }

    public int VisualIndex { get; }
    public int LogicalLineIndex { get; }
    public int StartChar { get; }
    public int Length { get; }
    public int LayoutLength { get; }
    public int GlobalStartIndex { get; }
    public bool IsContinuation { get; }
}

internal sealed class VisualLineMap
{
    private readonly List<VisualLineInfo> visualLines = new();
    private int[] logicalLineStartIndices = Array.Empty<int>();
    private int[] logicalLineVisualStart = Array.Empty<int>();
    private int[] logicalLineVisualCount = Array.Empty<int>();

    public IReadOnlyList<VisualLineInfo> VisualLines => visualLines;
    public int TotalVisualLines => visualLines.Count;

    public void Update(CanvasTextLayout layout, TextManager textManager)
    {
        visualLines.Clear();

        int lineCount = textManager.LinesCount;
        if (layout == null || lineCount == 0)
        {
            logicalLineStartIndices = Array.Empty<int>();
            logicalLineVisualStart = Array.Empty<int>();
            logicalLineVisualCount = Array.Empty<int>();
            return;
        }

        logicalLineStartIndices = new int[lineCount];
        logicalLineVisualStart = new int[lineCount];
        logicalLineVisualCount = new int[lineCount];

        int lineEndingLength = textManager.NewLineCharacter.Length;
        int currentIndex = 0;
        for (int i = 0; i < lineCount; i++)
        {
            logicalLineStartIndices[i] = currentIndex;
            logicalLineVisualStart[i] = -1;
            logicalLineVisualCount[i] = 0;
            currentIndex += textManager.GetLineLength(i);
            if (i < lineCount - 1)
                currentIndex += lineEndingLength;
        }

        var lineMetrics = layout.LineMetrics;
        int globalStart = 0;
        for (int i = 0; i < lineMetrics.Length; i++)
        {
            var metrics = lineMetrics[i];
            int lineGlobalStart = globalStart;
            int logicalLineIndex = GetLogicalLineIndexFromGlobal(globalStart);
            if (logicalLineIndex < 0)
                continue;

            int lineStartIndex = logicalLineStartIndices[logicalLineIndex];
            int lineLength = textManager.GetLineLength(logicalLineIndex);
            int startChar = Math.Clamp(lineGlobalStart - lineStartIndex, 0, lineLength);
            int remainingLength = Math.Max(0, lineLength - startChar);
            int layoutLength = Math.Max(0, metrics.CharacterCount);
            int length = Math.Clamp(layoutLength, 0, remainingLength);
            bool isContinuation = startChar > 0;

            visualLines.Add(new VisualLineInfo(i, logicalLineIndex, startChar, length, layoutLength, lineGlobalStart, isContinuation));

            if (logicalLineVisualStart[logicalLineIndex] == -1)
                logicalLineVisualStart[logicalLineIndex] = i;
            logicalLineVisualCount[logicalLineIndex]++;

            globalStart += layoutLength;
        }
    }

    public int GetLogicalLineIndex(int visualLineIndex)
    {
        if (visualLineIndex < 0 || visualLineIndex >= visualLines.Count)
            return -1;

        return visualLines[visualLineIndex].LogicalLineIndex;
    }

    public bool IsWrappedContinuation(int visualLineIndex)
    {
        if (visualLineIndex < 0 || visualLineIndex >= visualLines.Count)
            return false;

        return visualLines[visualLineIndex].IsContinuation;
    }

    public int GetVisualLineIndex(int logicalLineIndex)
    {
        if (logicalLineIndex < 0 || logicalLineIndex >= logicalLineVisualStart.Length)
            return -1;

        return logicalLineVisualStart[logicalLineIndex];
    }

    public int GetVisualLineCount(int logicalLineIndex)
    {
        if (logicalLineIndex < 0 || logicalLineIndex >= logicalLineVisualCount.Length)
            return 0;

        return logicalLineVisualCount[logicalLineIndex];
    }

    public bool TryGetVisualPosition(int globalIndex, out int visualLineIndex, out int column)
    {
        visualLineIndex = -1;
        column = 0;

        if (visualLines.Count == 0)
            return false;

        visualLineIndex = GetVisualLineIndexFromGlobal(globalIndex);
        if (visualLineIndex < 0 || visualLineIndex >= visualLines.Count)
            return false;

        var line = visualLines[visualLineIndex];
        int logicalLength = line.Length;
        int columnRaw = Math.Max(0, globalIndex - line.GlobalStartIndex);
        column = logicalLength == 0 ? 0 : Math.Clamp(columnRaw, 0, logicalLength);
        return true;
    }

    public bool TryGetLogicalPosition(int visualLineIndex, int column, out int logicalLine, out int character)
    {
        logicalLine = -1;
        character = 0;

        if (visualLineIndex < 0 || visualLineIndex >= visualLines.Count)
            return false;

        var line = visualLines[visualLineIndex];
        logicalLine = line.LogicalLineIndex;
        int logicalLength = line.Length;
        int localColumn = logicalLength == 0 ? 0 : Math.Clamp(column, 0, logicalLength);
        character = Math.Clamp(line.StartChar + localColumn, 0, line.StartChar + logicalLength);
        return true;
    }

    private int GetLogicalLineIndexFromGlobal(int globalIndex)
    {
        if (logicalLineStartIndices.Length == 0)
            return -1;

        int index = Array.BinarySearch(logicalLineStartIndices, globalIndex);
        if (index >= 0)
            return index;

        index = ~index - 1;
        return Math.Clamp(index, 0, logicalLineStartIndices.Length - 1);
    }

    private int GetVisualLineIndexFromGlobal(int globalIndex)
    {
        int low = 0;
        int high = visualLines.Count - 1;

        while (low <= high)
        {
            int mid = (low + high) / 2;
            var line = visualLines[mid];
            int start = line.GlobalStartIndex;
            int length = Math.Max(1, line.LayoutLength);
            int end = start + length;

            if (globalIndex < start)
                high = mid - 1;
            else if (globalIndex >= end)
                low = mid + 1;
            else
                return mid;
        }

        return Math.Clamp(low, 0, visualLines.Count - 1);
    }
}
