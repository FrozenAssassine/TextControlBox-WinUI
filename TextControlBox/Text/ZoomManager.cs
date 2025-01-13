using System;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Renderer;

namespace TextControlBoxNS.Text;

internal class ZoomManager
{
    public float ZoomedFontSize = 0;
    public int _ZoomFactor = 100; //%
    public int OldZoomFactor = 0;

    private readonly TextManager textManager;
    private readonly TextRenderer textRenderer;
    private readonly ScrollManager scrollManager;
    private readonly CanvasHelper canvasHelper;
    private readonly LineNumberRenderer lineNumberRenderer;
    private readonly CursorManager cursorManager;
    private readonly EventsManager eventsManager;

    public ZoomManager(TextManager textManager, TextRenderer textRenderer, CanvasHelper canvasHelper, LineNumberRenderer lineNumberRenderer, CursorManager cursorManager, EventsManager changedEventManager)
    {
        this.textManager = textManager;
        this.canvasHelper = canvasHelper;
        this.lineNumberRenderer = lineNumberRenderer;
        this.cursorManager = cursorManager;
    }

    public void UpdateZoom()
    {
        ZoomedFontSize = Math.Clamp(textManager._FontSize * (float)_ZoomFactor / 100, textManager.MinFontSize, textManager.MaxFontsize);
        _ZoomFactor = Math.Clamp(_ZoomFactor, 4, 400);

        if (_ZoomFactor != OldZoomFactor)
        {
            textRenderer.NeedsUpdateTextLayout = true;
            OldZoomFactor = _ZoomFactor;
            eventsManager.CallZoomChanged(_ZoomFactor);
        }

        textRenderer.NeedsTextFormatUpdate = true;

        scrollManager.ScrollLineIntoView(cursorManager.LineNumber);
        lineNumberRenderer.NeedsUpdateLineNumbers();
        canvasHelper.UpdateAll();
    }
}
