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

    public int? ZoomAnchorLine { get; set; } = null;
    public double ZoomAnchorHorizontalRatio { get; set; } = 0;

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
        float oldZoomedFontSize = ZoomedFontSize > 0 ? ZoomedFontSize : Math.Max(1, textManager._FontSize);

        _ZoomFactor = Math.Clamp(_ZoomFactor, MinZoom, MaxZoom);
        ZoomedFontSize = Math.Clamp(textManager._FontSize * (float)_ZoomFactor / 100, textManager.MinFontSize, textManager.MaxFontsize);

        if (_ZoomFactor != OldZoomFactor)
        {
            // Capture the top visible line on the first zoom tick so it stays rigidly anchored
            // throughout the entire zoom gesture.
            if (!ZoomAnchorLine.HasValue)
            {
                ZoomAnchorLine = textRenderer.NumberOfStartLine;
                IScrollOffsetSource src = scrollManager?.OffsetSource;
                ZoomAnchorHorizontalRatio = (src != null && src.HorizontalOffset > 0.5 && oldZoomedFontSize > 0.5f)
                    ? (src.HorizontalOffset / oldZoomedFontSize)
                    : 0;
            }

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
}
