using System;

namespace TextControlBoxNS.Core;

/// <summary>
/// Pure, WinUI-free conversions between the editor's pixel scroll seam (<see cref="IScrollOffsetSource"/>)
/// and the legacy <c>ScrollBar</c> backing store. The VERTICAL scrollbar stores <c>Value</c>/<c>Maximum</c>
/// in units of <c>SingleLineHeight / DefaultVerticalScrollSensitivity</c>; the HORIZONTAL scrollbar is
/// already in pixels.
///
/// <para>Extracted from <see cref="ScrollBarOffsetSource"/> so the pixelΓåöscrollbar-unit arithmetic ΓÇö the
/// exact place a "scroll is 4├ù too fast" regression would hide ΓÇö is unit-testable in isolation (a
/// <c>ScrollBar</c> cannot be instantiated headless). See PLANS/EDITOR_DIAGONAL_SCROLL_REWRITE_PLAN.md,
/// Phase 1.</para>
/// </summary>
internal static class ScrollOffsetMath
{
    /// <summary>The vertical sensitivity is used as a divisor, so it must be at least 1.</summary>
    internal static int NormalizeSensitivity(int sensitivity) => Math.Max(1, sensitivity);

    /// <summary>Vertical <c>ScrollBar.Value</c> (legacy units) ΓåÆ pixels.</summary>
    internal static double VerticalValueToPixels(double scrollBarValue, int sensitivity)
        => scrollBarValue * NormalizeSensitivity(sensitivity);

    /// <summary>Pixels ΓåÆ vertical <c>ScrollBar.Value</c> (legacy units). Negative pixels clamp to 0; the
    /// <c>ScrollBar</c> itself further clamps to <c>[Minimum, Maximum]</c>.</summary>
    internal static double PixelsToVerticalValue(double pixels, int sensitivity)
        => Math.Max(0, pixels) / NormalizeSensitivity(sensitivity);

    /// <summary>Vertical <c>ScrollBar.Maximum</c> (legacy units) + viewport pixels ΓåÆ total content height
    /// in pixels (<c>ScrollViewer.ExtentHeight</c> semantics).</summary>
    internal static double VerticalMaximumToExtentPixels(double maximum, int sensitivity, double viewportHeightPixels)
        => (maximum * NormalizeSensitivity(sensitivity)) + viewportHeightPixels;

    /// <summary>Total content height in pixels ΓåÆ vertical <c>ScrollBar.Maximum</c> (legacy units), floored
    /// at 0 (content smaller than the viewport is not scrollable).</summary>
    internal static double VerticalExtentPixelsToMaximum(double extentPixels, int sensitivity, double viewportHeightPixels)
        => Math.Max(0, extentPixels - viewportHeightPixels) / NormalizeSensitivity(sensitivity);

    /// <summary>Clamps a horizontal pixel offset to be non-negative (horizontal is already pixels).</summary>
    internal static double ClampHorizontalOffset(double pixels) => Math.Max(0, pixels);

    /// <summary>Horizontal <c>ScrollBar.Maximum</c> (pixels) + viewport pixels ΓåÆ total content width in
    /// pixels (<c>ScrollViewer.ExtentWidth</c> semantics).</summary>
    internal static double HorizontalMaximumToExtentPixels(double maximum, double viewportWidthPixels)
        => maximum + viewportWidthPixels;

    /// <summary>Total content width in pixels ΓåÆ horizontal <c>ScrollBar.Maximum</c> (pixels), floored at
    /// 0 (content narrower than the viewport is not scrollable).</summary>
    internal static double HorizontalExtentPixelsToMaximum(double extentPixels, double viewportWidthPixels)
        => Math.Max(0, extentPixels - viewportWidthPixels);
}
