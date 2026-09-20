using System;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Core.Text;

namespace TextControlBoxNS.Core;

internal class ZoomManager
{
    private const int MinZoom = 4;
    private const int MaxZoom = 400;

    public float ZoomedFontSize = 0;
    public int _ZoomFactor = 100; //%
    private int OldZoomFactor = 0;
    public bool ZoomNeedsRecalculateLongestLine = false;
    private TextManager textManager;
    private TextRenderer textRenderer;
    private CanvasUpdateManager canvasHelper;
    private EventsManager eventsManager;
    private LineNumberRenderer lineNumberRenderer;
    private ScrollManager scrollManager;

    public void Init(
        TextManager textManager,
        TextRenderer textRenderer,
        CanvasUpdateManager canvasHelper,
        EventsManager eventsManager,
        LineNumberRenderer lineNumberRenderer,
        ScrollManager scrollManager
        )
    {
        this.textManager = textManager;
        this.textRenderer = textRenderer;
        this.canvasHelper = canvasHelper;
        this.eventsManager = eventsManager;
        this.lineNumberRenderer = lineNumberRenderer;
        this.scrollManager = scrollManager;
    }

    public void UpdateZoom()
    {
        // Capture the pre-zoom line height (TextFormat is only rebuilt on the next draw, so
        // textRenderer.SingleLineHeight still reflects the OLD font size here) so the scroll position can be
        // re-anchored once the new font size is known.
        float oldSingleLineHeight = textRenderer.SingleLineHeight;

        ZoomedFontSize = Math.Clamp(textManager._FontSize * (float)_ZoomFactor / 100, textManager.MinFontSize, textManager.MaxFontsize);
        _ZoomFactor = Math.Clamp(_ZoomFactor, MinZoom, MaxZoom);

        if (_ZoomFactor != OldZoomFactor)
        {
            // Keep the document row at the vertical viewport centre stationary across the zoom so zooming
            // feels anchored instead of drifting.
            AnchorScrollAcrossZoom(oldSingleLineHeight);

            textRenderer.NeedsUpdateTextLayout = true;
            OldZoomFactor = _ZoomFactor;
            eventsManager.CallZoomChanged(_ZoomFactor);
            
            lineNumberRenderer.NeedsUpdateLineNumbers();

            ZoomNeedsRecalculateLongestLine = true;
            textRenderer.NeedsTextFormatUpdate = true;
            textRenderer.InvalidateWrapMetrics();
            canvasHelper.UpdateAll();
        }
    }

    // Re-anchor the pixel scroll seam so the row under the vertical viewport centre stays put after the font
    // size (and therefore SingleLineHeight = ZoomedFontSize + LineSpacingPadding) changes. The horizontal
    // axis scales by the font-size ratio to keep the left-most visible column stable.
    private void AnchorScrollAcrossZoom(float oldSingleLineHeight)
    {
        IScrollOffsetSource src = scrollManager?.OffsetSource;
        if (src is null || oldSingleLineHeight <= 0.5f)
            return;

        float newSingleLineHeight = ZoomedFontSize + TextLayoutManager.LineSpacingPadding;
        if (newSingleLineHeight <= 0.5f)
            return;

        src.VerticalOffset = ZoomScrollAnchor.AnchorVerticalOffset(src.VerticalOffset, src.ViewportHeight, oldSingleLineHeight, newSingleLineHeight);

        double oldFontSize = oldSingleLineHeight - TextLayoutManager.LineSpacingPadding;
        if (src.HorizontalOffset > 0.5 && oldFontSize > 0.5f)
            src.HorizontalOffset = ZoomScrollAnchor.AnchorHorizontalOffset(src.HorizontalOffset, oldFontSize, ZoomedFontSize);
    }
}
