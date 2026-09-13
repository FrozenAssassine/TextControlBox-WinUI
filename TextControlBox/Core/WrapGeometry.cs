using System;

namespace TextControlBoxNS.Core;

/// <summary>
/// Pure (WinUI-free) geometry for mapping a pointer Y coordinate to a visual row and to a within-line
/// hit-test Y, in word-wrap mode. Extracted so the click math is unit-testable without a GPU canvas.
/// </summary>
internal static class WrapGeometry
{
    /// <summary>
    /// Visual row under pointer Y. <paramref name="startVisualRow"/> is the first visual row currently
    /// scrolled into view; the small top inset (a fraction of a row) matches the vertical draw offset.
    /// </summary>
    public static int CalculateVisualRowFromPointY(double y, int startVisualRow, float singleLineHeight, int defaultVerticalScrollSensitivity)
    {
        double rowHeight = Math.Max(1, singleLineHeight);
        double topInset = rowHeight / Math.Max(1, defaultVerticalScrollSensitivity);
        int relativeRow = (int)Math.Floor(Math.Max(0, y - topInset) / rowHeight);
        return startVisualRow + relativeRow;
    }

    /// <summary>
    /// Y coordinate (relative to a wrapped line's own layout) to hit-test against, clamped to that line's
    /// wrapped extent so a click below the last row maps to the last row rather than past it.
    /// </summary>
    public static float CalculateWrappedLineHitTestYFromPointY(double y, float lineTopY, float singleLineHeight, int defaultVerticalScrollSensitivity, int wrappedRowCount)
    {
        double rowHeight = Math.Max(1, singleLineHeight);
        double topInset = rowHeight / Math.Max(1, defaultVerticalScrollSensitivity);
        double maxHitTestY = Math.Max(0, wrappedRowCount * rowHeight - 0.001);
        return (float)Math.Clamp(y - lineTopY - topInset, 0, maxHitTestY);
    }
}
