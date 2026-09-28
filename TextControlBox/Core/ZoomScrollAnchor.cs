using System;

namespace TextControlBoxNS.Core;

/// <summary>
/// Pure (WinUI-free) arithmetic for keeping the scroll position anchored across a zoom (font-size) change,
/// so zooming feels stationary instead of drifting. The same raw pixel offset maps to a different row once
/// the line height changes, so the offsets have to be rescaled. Kept separate so it is unit-testable.
/// </summary>
internal static class ZoomScrollAnchor
{
    /// <summary>
    /// New vertical pixel offset that keeps the specified top document row stationary
    /// at the given line height.
    /// </summary>
    public static double AnchorVerticalOffset(int topRow, double newLineHeight)
    {
        double newOffset = Math.Max(0, topRow) * newLineHeight;
        return newOffset < 0 ? 0 : newOffset;
    }

    /// <summary>
    /// New vertical pixel offset that keeps the top document row stationary
    /// when the line height changes from <paramref name="oldLineHeight"/> to <paramref name="newLineHeight"/>.
    /// </summary>
    public static double AnchorVerticalOffset(double verticalOffset, double oldLineHeight, double newLineHeight)
    {
        if (oldLineHeight <= 0.5)
            return verticalOffset;

        int topRow = (int)Math.Round(verticalOffset / oldLineHeight);
        return AnchorVerticalOffset(topRow, newLineHeight);
    }

    /// <summary>
    /// Overload that accepts viewportHeight for backwards compatibility.
    /// </summary>
    public static double AnchorVerticalOffset(double verticalOffset, double viewportHeight, double oldLineHeight, double newLineHeight)
    {
        return AnchorVerticalOffset(verticalOffset, oldLineHeight, newLineHeight);
    }

    /// <summary>
    /// New horizontal pixel offset scaled by the font-size ratio (left-edge anchor), so the left-most visible
    /// column stays roughly stable across the zoom. Returns the input unchanged when the old size is unusable.
    /// </summary>
    public static double AnchorHorizontalOffset(double horizontalOffset, double oldFontSize, double newFontSize)
    {
        if (oldFontSize <= 0.5)
            return horizontalOffset;
        double scaled = horizontalOffset * (newFontSize / oldFontSize);
        return scaled < 0 ? 0 : scaled;
    }
}
