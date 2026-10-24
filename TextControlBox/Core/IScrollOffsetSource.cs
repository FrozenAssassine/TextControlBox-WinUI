using System;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace TextControlBoxNS.Core;

/// <summary>
/// The single funnel for the editor's scroll position. Every consumer that used to read or write
/// <c>verticalScrollBar.Value</c> / <c>horizontalScrollBar.Value</c> now goes through this seam.
///
/// <para><b>All offsets and extents are PIXELS.</b> This is deliberate (see
/// PLANS/EDITOR_DIAGONAL_SCROLL_REWRITE_PLAN.md, Phase 1 / finding #2): the legacy vertical scroll was
/// stored in <c>SingleLineHeight / DefaultVerticalScrollSensitivity</c> scrollbar units, so exposing the
/// seam in pixels from the start means the Phase 2 backend swap (two <see cref="ScrollBar"/> primitives ΓåÆ
/// a real <c>ScrollViewer</c>) does not become a whole-consumer unit rewrite. The initial
/// <see cref="ScrollBarOffsetSource"/> adapter does the pixelΓåöscrollbar-unit conversion internally.</para>
/// </summary>
internal interface IScrollOffsetSource
{
    /// <summary>Vertical scroll position, in pixels (0 = top).</summary>
    double VerticalOffset { get; set; }

    /// <summary>Horizontal scroll position, in pixels (0 = left).</summary>
    double HorizontalOffset { get; set; }

    /// <summary>Total content height in pixels (ScrollViewer.ExtentHeight semantics).</summary>
    double VerticalExtent { get; set; }

    /// <summary>Total content width in pixels (ScrollViewer.ExtentWidth semantics).</summary>
    double HorizontalExtent { get; set; }

    /// <summary>Visible viewport width in pixels.</summary>
    double ViewportWidth { get; set; }

    /// <summary>Visible viewport height in pixels.</summary>
    double ViewportHeight { get; set; }

    /// <summary>Sets one or both offsets (pixels). Pass <c>null</c> to leave that axis unchanged.
    /// Mirrors <c>ScrollViewer.ChangeView</c> so the Phase 2 backend is a drop-in.</summary>
    void ChangeView(double? horizontalOffset, double? verticalOffset);

    /// <summary>Raised whenever the scroll position changes ΓÇö a user drag of a scrollbar thumb OR a
    /// programmatic write to <see cref="VerticalOffset"/>/<see cref="HorizontalOffset"/> (caret-follow,
    /// page keys, go-to-line, find reveal, match hand-off, wheel), or ΓÇö after the Phase 2 swap ΓÇö native
    /// wheel/touchpad panning. The Phase 1 <see cref="ScrollBarOffsetSource"/> raises it from the two
    /// scrollbars' <c>Scroll</c> events and from the offset setters. The diagonal-scroll
    /// <c>InteractionTracker</c> subscribes so it snaps to any programmatic scroll immediately.</summary>
    event EventHandler ViewChanged;
}

/// <summary>
/// Phase 1 backend for <see cref="IScrollOffsetSource"/>: the existing two standalone <see cref="ScrollBar"/>
/// primitives. Vertical <c>ScrollBar.Value</c>/<c>ScrollBar.Maximum</c> are stored in legacy
/// units of <c>SingleLineHeight / verticalSensitivity</c>; this adapter multiplies/divides by
/// <c>verticalSensitivity</c> so the seam is pixel-based. Horizontal values are already pixels.
/// </summary>
internal sealed class ScrollBarOffsetSource : IScrollOffsetSource
{
    private readonly ScrollBar _vertical;
    private readonly ScrollBar _horizontal;
    private readonly int _verticalSensitivity;

    public ScrollBarOffsetSource(ScrollBar vertical, ScrollBar horizontal, int verticalSensitivity)
    {
        _vertical = vertical;
        _horizontal = horizontal;
        _verticalSensitivity = Math.Max(1, verticalSensitivity);
        _vertical.Scroll += OnScrollBarScroll;
        _horizontal.Scroll += OnScrollBarScroll;
    }

    public event EventHandler ViewChanged;

    private void OnScrollBarScroll(object sender, ScrollEventArgs e) => ViewChanged?.Invoke(this, EventArgs.Empty);

    public double VerticalOffset
    {
        get => ScrollOffsetMath.VerticalValueToPixels(_vertical.Value, _verticalSensitivity);
        // ScrollBar.Value is clamped to [Minimum, Maximum] by the control, matching the legacy behavior
        // where every scroll mutator relied on the scrollbar to clamp an out-of-range assignment.
        set
        {
            _vertical.Value = ScrollOffsetMath.PixelsToVerticalValue(value, _verticalSensitivity);
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public double HorizontalOffset
    {
        get => _horizontal.Value;
        set
        {
            _horizontal.Value = ScrollOffsetMath.ClampHorizontalOffset(value);
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public double VerticalExtent
    {
        get => ScrollOffsetMath.VerticalMaximumToExtentPixels(_vertical.Maximum, _verticalSensitivity, ViewportHeight);
        set => _vertical.Maximum = ScrollOffsetMath.VerticalExtentPixelsToMaximum(value, _verticalSensitivity, ViewportHeight);
    }

    public double HorizontalExtent
    {
        get => ScrollOffsetMath.HorizontalMaximumToExtentPixels(_horizontal.Maximum, ViewportWidth);
        set => _horizontal.Maximum = ScrollOffsetMath.HorizontalExtentPixelsToMaximum(value, ViewportWidth);
    }

    public double ViewportWidth
    {
        get => _horizontal.ViewportSize;
        set => _horizontal.ViewportSize = value;
    }

    public double ViewportHeight
    {
        get => _vertical.ViewportSize;
        set => _vertical.ViewportSize = value;
    }

    public void ChangeView(double? horizontalOffset, double? verticalOffset)
    {
        if (horizontalOffset.HasValue)
            HorizontalOffset = horizontalOffset.Value;
        if (verticalOffset.HasValue)
            VerticalOffset = verticalOffset.Value;
    }
}
