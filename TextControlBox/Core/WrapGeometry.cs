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
    public static int CalculateVisualRowFromPointY(double y, int startVisualRow, float singleLineHeight, int defaultVerticalScrollSensitivity, float topInset = 0, float subLineOffset = 0)
    {
        double rowHeight = Math.Max(1, singleLineHeight);
        int relativeRow = (int)Math.Floor(Math.Max(0, y - topInset + subLineOffset) / rowHeight);
        return startVisualRow + relativeRow;
    }

    /// <summary>
    /// Y coordinate (relative to a wrapped line's own layout) to hit-test against, clamped to that line's
    /// wrapped extent so a click below the last row maps to the last row rather than past it.
    /// </summary>
    public static float CalculateWrappedLineHitTestYFromPointY(double y, float lineTopY, float singleLineHeight, int defaultVerticalScrollSensitivity, int wrappedRowCount, float topInset = 0)
    {
        double rowHeight = Math.Max(1, singleLineHeight);
        double maxHitTestY = Math.Max(0, wrappedRowCount * rowHeight - 0.001);
        return (float)Math.Clamp(y - lineTopY - topInset, 0, maxHitTestY);
    }

    /// <summary>
    /// Number of visual rows beyond the first that a caret X lands on when it sits past the wrap width. DirectWrite
    /// never wraps trailing whitespace: spaces at the end of a wrapped line hang past the width on the same row, so
    /// the caret keeps moving right without ever starting a new row. Returns 0 when the position fits the first row.
    /// </summary>
    public static int CalculateHangingRows(float x, float wrapWidth)
    {
        if (wrapWidth <= 1 || x <= wrapWidth)
            return 0;
        return (int)Math.Ceiling(x / (double)wrapWidth) - 1;
    }

    /// <summary>
    /// Maps a DirectWrite caret of a hanging whitespace position onto the visual row it belongs to: the X is folded
    /// back into the wrap width and the Y moves down by the number of hanging rows. Positions that fit are returned
    /// unchanged.
    /// </summary>
    public static (float X, float Y) MapHangingCaret(float x, float y, float wrapWidth, float singleLineHeight)
    {
        int rows = CalculateHangingRows(x, wrapWidth);
        if (rows == 0)
            return (x, y);
        return (x - rows * wrapWidth, y + rows * Math.Max(1, singleLineHeight));
    }
}
