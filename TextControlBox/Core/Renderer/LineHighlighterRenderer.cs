﻿using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.UI.Xaml;
using TextControlBoxNS.Core.Selection;

namespace TextControlBoxNS.Core.Renderer;

internal class LineHighlighterRenderer
{
    private LineHighlighterManager lineHighlighterManager;
    private SelectionManager selectionManager;
    private TextRenderer textRenderer;
    public void Init(LineHighlighterManager lineHighlighterManager, SelectionManager selectionManager, TextRenderer textRenderer)
    {
        this.selectionManager = selectionManager;
        this.lineHighlighterManager = lineHighlighterManager;
        this.textRenderer = textRenderer;
    }

    public void Render(float canvasWidth, float y, float fontSize, CanvasDrawEventArgs args, CanvasSolidColorBrush backgroundBrush)
    {
        if (textRenderer.CurrentLineTextLayout == null)
            return;

        args.DrawingSession.FillRectangle(0, y, canvasWidth, fontSize, backgroundBrush);
    }

    public bool CanRender()
    {
        return lineHighlighterManager._ShowLineHighlighter && !selectionManager.HasSelection;
    }
}
