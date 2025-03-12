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

    private TextManager textManager;
    private TextRenderer textRenderer;
    private CanvasUpdateManager canvasHelper;
    private LineNumberRenderer lineNumberRenderer;
    private EventsManager eventsManager;
    private LongestLineManager longestLineManager;
    private ScrollManager scrollManager;
    private CoreTextControlBox coreTextBox;
    
    public void Init(
        TextManager textManager,
        TextRenderer textRenderer,
        ScrollManager scrollManager,
        CanvasUpdateManager canvasHelper,
        LineNumberRenderer lineNumberRenderer,
        EventsManager eventsManager,
        LongestLineManager longestLineManager,
        CoreTextControlBox coreTextBox
        )
    {
        this.textManager = textManager;
        this.textRenderer = textRenderer;
        this.canvasHelper = canvasHelper;
        this.lineNumberRenderer = lineNumberRenderer;
        this.eventsManager = eventsManager;
        this.longestLineManager = longestLineManager;
        this.scrollManager = scrollManager;
        this.coreTextBox = coreTextBox;
    }

    public void UpdateZoom()
    {
        ZoomedFontSize = Math.Clamp(textManager._FontSize * (float)_ZoomFactor / 100, textManager.MinFontSize, textManager.MaxFontsize);
        _ZoomFactor = Math.Clamp(_ZoomFactor, MinZoom, MaxZoom);

        if (_ZoomFactor != OldZoomFactor)
        {
            textRenderer.NeedsUpdateTextLayout = true;
            OldZoomFactor = _ZoomFactor;
            eventsManager.CallZoomChanged(_ZoomFactor);

            textRenderer.NeedsTextFormatUpdate = true;
            lineNumberRenderer.NeedsUpdateLineNumbers();
            canvasHelper.UpdateAll();
        }
    }
}
