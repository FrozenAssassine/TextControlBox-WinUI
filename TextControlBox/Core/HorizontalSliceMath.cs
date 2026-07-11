using System;

namespace TextControlBoxNS.Core;

/// <summary>
/// Pure (WinUI-free) arithmetic for horizontal virtualization of very long lines: computing the slice window
/// from the viewport, its buffered reuse "safe zone", and mapping document columns in and out of the sliced
/// layout. Kept separate from <see cref="Renderer.TextRenderer"/> so the index math (where an off-by-one
/// silently misplaces the caret or selection) is unit-testable without a GPU canvas.
/// </summary>
internal static class HorizontalSliceMath
{
    // A little padding each side of the raw viewport so the caret sitting at the very edge is still inside
    // the laid-out slice.
    private const int VisiblePaddingLeft = 50;
    private const int VisiblePaddingRight = 100;

    /// <summary>
    /// Visible character range [start, end) covered by the viewport at a given horizontal scroll (both in
    /// pixels), padded a little each side. Assumes a monospace <paramref name="charWidth"/>.
    /// </summary>
    public static (int visibleStart, int visibleEnd) VisibleCharRange(double horizontalScrollPixels, double viewportWidthPixels, double charWidth)
    {
        double cw = charWidth <= 0 ? 1 : charWidth;
        int visibleStart = Math.Max(0, (int)(horizontalScrollPixels / cw) - VisiblePaddingLeft);
        int visibleEnd = visibleStart + (int)(viewportWidthPixels / cw) + VisiblePaddingRight;
        return (visibleStart, visibleEnd);
    }

    /// <summary>
    /// True when the viewport [<paramref name="visibleStart"/>, <paramref name="visibleEnd"/>) is still inside
    /// the current window's buffered safe zone [<paramref name="safeStart"/>, <paramref name="safeEnd"/>), so
    /// the existing slice can be reused without rebuilding the layout.
    /// </summary>
    public static bool CanReuseWindow(int visibleStart, int visibleEnd, int safeStart, int safeEnd)
        => visibleStart >= safeStart && visibleEnd <= safeEnd;

    /// <summary>
    /// Computes a fresh slice window for the given viewport range, with a generous buffer either side for
    /// smooth scrolling. Returns the window [sliceStart, sliceStart + sliceLen) plus the inner safe zone
    /// [safeStart, safeEnd) to store for the next-frame <see cref="CanReuseWindow"/> check.
    /// </summary>
    public static (int sliceStart, int sliceLen, int safeStart, int safeEnd) ComputeWindow(int visibleStart, int visibleEnd)
    {
        int visibleChars = Math.Max(1, visibleEnd - visibleStart);
        int bufferChars = visibleChars * 3;
        int sliceStart = Math.Max(0, visibleStart - bufferChars);
        int sliceLen = visibleChars + (bufferChars * 2);
        int safeStart = visibleStart;
        int safeEnd = visibleEnd + bufferChars;
        return (sliceStart, sliceLen, safeStart, safeEnd);
    }

    /// <summary>
    /// Number of characters a line contributes to the window: its length past <paramref name="sliceStart"/>,
    /// capped at the window width <paramref name="sliceLen"/>. 0 when the line ends before the window.
    /// </summary>
    public static int SlicedLineLength(int fullLineLength, int sliceStart, int sliceLen)
    {
        if (sliceLen <= 0)
            return 0;
        return Math.Clamp(fullLineLength - sliceStart, 0, sliceLen);
    }

    /// <summary>Rendered index within a line's sliced layout for a document column, clamped to the sliced
    /// length. Inverse of <see cref="ColumnForRenderedIndex"/>.</summary>
    public static int RenderedIndexForColumn(int characterPosition, int sliceStart, int slicedLineLength)
        => Math.Clamp(characterPosition - sliceStart, 0, slicedLineLength);

    /// <summary>Document column for a rendered index within a line's sliced layout, clamped to the document
    /// line length. Inverse of <see cref="RenderedIndexForColumn"/>.</summary>
    public static int ColumnForRenderedIndex(int renderedIndex, int sliceStart, int fullLineLength)
        => Math.Clamp(renderedIndex + sliceStart, 0, fullLineLength);
}
