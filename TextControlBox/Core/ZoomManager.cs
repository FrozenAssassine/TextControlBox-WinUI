﻿using System;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Core.Text;

namespace TextControlBoxNS.Core;

internal class ZoomManager
{
    public float ZoomedFontSize = 0;
    public int _ZoomFactor = 100; //%
    public int OldZoomFactor = 0;

    private TextManager textManager;
    private TextRenderer textRenderer;
    private CanvasUpdateManager canvasHelper;
    private LineNumberRenderer lineNumberRenderer;
    private EventsManager eventsManager;

    public void Init(
        TextManager textManager,
        TextRenderer textRenderer,
        ScrollManager scrollManager,
        CanvasUpdateManager canvasHelper,
        LineNumberRenderer lineNumberRenderer,
        EventsManager eventsManager)
    {
        this.textManager = textManager;
        this.textRenderer = textRenderer;
        this.canvasHelper = canvasHelper;
        this.lineNumberRenderer = lineNumberRenderer;
        this.eventsManager = eventsManager;
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
        lineNumberRenderer.NeedsUpdateLineNumbers();
        canvasHelper.UpdateAll();
    }
}
