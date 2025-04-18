﻿using System;
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

    public void Init(
        TextManager textManager,
        TextRenderer textRenderer,
        CanvasUpdateManager canvasHelper,
        EventsManager eventsManager,
        LineNumberRenderer lineNumberRenderer
        )
    {
        this.textManager = textManager;
        this.textRenderer = textRenderer;
        this.canvasHelper = canvasHelper;
        this.eventsManager = eventsManager;
        this.lineNumberRenderer = lineNumberRenderer;
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
            
            lineNumberRenderer.NeedsUpdateLineNumbers();

            ZoomNeedsRecalculateLongestLine = true;
            textRenderer.NeedsTextFormatUpdate = true;
            canvasHelper.UpdateAll();
        }
    }
}
