using System;
using System.Collections.Generic;

namespace TextControlBoxNS.Core;

/// <summary>
/// Pure (WinUI-free) visual-row bookkeeping for word wrap. In wrap mode a single document line can occupy
/// several visual rows, so the vertical scrollbar, caret and selection all work in "visual-row" space rather
/// than document-line space. This class holds the mapping — a per-line row count and the cumulative
/// start-row prefix sum — and supports cheap incremental updates when only a few lines change.
///
/// <para>The impure part (measuring how many rows a line wraps into at the current width, which needs a
/// <c>CanvasTextLayout</c>) is injected as a delegate, so all the arithmetic here is unit-testable without a
/// GPU canvas — which is exactly where an off-by-one silently corrupts scrolling or caret placement.</para>
/// </summary>
internal sealed class WrapRowMetrics
{
    // Row count per document line index.
    private readonly Dictionary<int, int> _rowCountByLine = new();
    // Cumulative start visual row of each document line: entry k is the first visual row of line k, and the
    // final entry is the total visual-row count. Length is lineCount + 1 when valid.
    private readonly List<int> _startRow = new();
    private int _totalVisualRows = 1;

    /// <summary>Total number of visual rows across the whole document (at least 1).</summary>
    public int TotalVisualRows => _totalVisualRows;

    /// <summary>True when the cached prefix sum matches <paramref name="lineCount"/> and can be used directly
    /// (otherwise callers should <see cref="Rebuild"/> first).</summary>
    public bool IsValidFor(int lineCount) => _startRow.Count == lineCount + 1;

    public void Clear()
    {
        _rowCountByLine.Clear();
        _startRow.Clear();
        _totalVisualRows = 1;
    }

    /// <summary>Rebuilds the whole mapping by measuring every line. <paramref name="measureRows"/> returns the
    /// number of visual rows a document line wraps into (>= 1).</summary>
    public void Rebuild(int lineCount, Func<int, int> measureRows)
    {
        _rowCountByLine.Clear();
        _startRow.Clear();

        int visualRow = 0;
        _startRow.Add(visualRow);
        for (int i = 0; i < lineCount; i++)
        {
            int rows = Math.Max(1, measureRows(i));
            _rowCountByLine[i] = rows;
            visualRow += rows;
            _startRow.Add(visualRow);
        }

        _totalVisualRows = Math.Max(1, visualRow);
    }

    /// <summary>Re-measures only <paramref name="dirtyLines"/> and shifts the cumulative start-row prefix by
    /// each line's row-count delta — cheap integer arithmetic plus a handful of measurements, versus a full
    /// <see cref="Rebuild"/>. Returns <c>false</c> (caller should Rebuild) when the cache is stale for
    /// <paramref name="lineCount"/> or more than <paramref name="remeasureLimit"/> lines are dirty.</summary>
    public bool ApplyIncremental(int lineCount, IEnumerable<int> dirtyLines, int dirtyCount, int remeasureLimit, Func<int, int> measureRows)
    {
        if (dirtyCount > remeasureLimit || !IsValidFor(lineCount))
            return false;

        bool changed = false;
        foreach (int line in dirtyLines)
        {
            if (line < 0 || line >= lineCount)
                continue;

            int oldRows = _rowCountByLine.TryGetValue(line, out int cached) ? cached : 1;
            int newRows = Math.Max(1, measureRows(line));
            if (newRows == oldRows)
                continue;

            _rowCountByLine[line] = newRows;
            int delta = newRows - oldRows;
            for (int i = line + 1; i < _startRow.Count; i++)
                _startRow[i] += delta;
            changed = true;
        }

        if (changed && _startRow.Count > 0)
            _totalVisualRows = Math.Max(1, _startRow[^1]);

        return true;
    }

    /// <summary>Visual rows document line <paramref name="lineIndex"/> occupies (1 when unknown/out of range).</summary>
    public int GetRowCount(int lineIndex)
        => _rowCountByLine.TryGetValue(lineIndex, out int rows) ? rows : 1;

    /// <summary>First visual row of document line <paramref name="lineIndex"/>. Falls back to summing row
    /// counts when the prefix cache is not valid for <paramref name="lineCount"/>.</summary>
    public int GetLineStartRow(int lineIndex, int lineCount)
    {
        int capped = Math.Clamp(lineIndex, 0, lineCount);
        if (IsValidFor(lineCount))
            return _startRow[capped];

        int visualRow = 0;
        for (int i = 0; i < capped; i++)
            visualRow += GetRowCount(i);
        return visualRow;
    }

    /// <summary>Document line that owns <paramref name="visualRow"/>. Binary-searches the prefix cache when
    /// valid; otherwise walks the row counts.</summary>
    public int GetDocumentLineFromVisualRow(int visualRow, int lineCount)
    {
        if (lineCount == 0)
            return 0;

        visualRow = Math.Clamp(visualRow, 0, Math.Max(0, _totalVisualRows - 1));

        if (IsValidFor(lineCount))
        {
            int low = 0;
            int high = lineCount - 1;
            while (low <= high)
            {
                int mid = low + ((high - low) / 2);
                int lineStart = _startRow[mid];
                int nextLineStart = _startRow[mid + 1];

                if (visualRow < lineStart)
                    high = mid - 1;
                else if (visualRow >= nextLineStart)
                    low = mid + 1;
                else
                    return mid;
            }
            return Math.Clamp(low, 0, lineCount - 1);
        }

        int rowCursor = 0;
        for (int i = 0; i < lineCount; i++)
        {
            int rows = GetRowCount(i);
            if (rowCursor + rows > visualRow)
                return i;
            rowCursor += rows;
        }
        return Math.Max(0, lineCount - 1);
    }

    /// <summary>Visual rows spanned by document lines [startLine, startLine + lineCount).</summary>
    public int GetRenderedVisualRowCount(int startLine, int lineCount, int totalLineCount)
    {
        int endLine = Math.Clamp(startLine + lineCount, 0, totalLineCount);
        return Math.Max(0, GetLineStartRow(endLine, totalLineCount) - GetLineStartRow(startLine, totalLineCount));
    }
}
