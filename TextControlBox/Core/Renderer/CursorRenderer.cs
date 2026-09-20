using System;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Diagnostics;
using System.Numerics;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Helper;

namespace TextControlBoxNS.Core.Renderer;

internal class CursorRenderer
{
    public CursorSize _CursorSize = null;

    private CursorManager cursorManager;
    private CurrentLineManager currentLineManager;
    private TextRenderer textRenderer;
    private FocusManager focusManager;
    private TextManager textManager;
    private ScrollManager scrollManager;
    private ZoomManager zoomManager;
    private DesignHelper designHelper;
    private LineHighlighterRenderer lineHighlighterRenderer;
    private EventsManager eventsManager;
    private LongestLineManager longestLineManager;
    private CaretBlinkManager caretBlinkManager;

    public void Init(
        CursorManager cursorManager,
        CurrentLineManager currentLineManager,
        TextRenderer textRenderer,
        FocusManager focusManager,
        TextManager textManager,
        ScrollManager scrollManager,
        ZoomManager zoomManager,
        DesignHelper designHelper,
        LineHighlighterRenderer lineHighlighterRenderer,
        EventsManager eventsManager,
        LongestLineManager longestLineManager,
        CaretBlinkManager caretBlinkManager)
    {
        this.cursorManager = cursorManager;
        this.currentLineManager = currentLineManager;
        this.textRenderer = textRenderer;
        this.focusManager = focusManager;
        this.textManager = textManager;
        this.scrollManager = scrollManager;
        this.zoomManager = zoomManager;
        this.designHelper = designHelper;
        this.lineHighlighterRenderer = lineHighlighterRenderer;
        this.eventsManager = eventsManager;
        this.longestLineManager = longestLineManager;
        this.caretBlinkManager = caretBlinkManager;
    }

    public void RenderCursor(CanvasTextLayout textLayout, int characterPosition, float xOffset, float y, float fontSize, CursorSize customSize, CanvasDrawEventArgs args, CanvasSolidColorBrush cursorColorBrush)
    {
        if (textLayout == null)
            return;

        Vector2 vector = textLayout.GetCaretPosition(characterPosition < 0 ? 0 : characterPosition, false);
        if (customSize == null)
            args.DrawingSession.FillRectangle(vector.X + xOffset, y, 2, fontSize, cursorColorBrush);
        else
            args.DrawingSession.FillRectangle(vector.X + xOffset + customSize.OffsetX, y + customSize.OffsetY, (float)customSize.Width, (float)customSize.Height, cursorColorBrush);
    }

    public void Draw(CanvasControl canvasText, CanvasControl canvasCursor, CanvasDrawEventArgs args)
    {
        currentLineManager.UpdateCurrentLine(cursorManager.LineNumber);
        if (textRenderer.DrawnTextLayout == null)
            return;

        int currentLineLength = currentLineManager.Length;
        if (cursorManager.LineNumber >= textManager.LinesCount)
        {
            cursorManager.LineNumber = textManager.LinesCount - 1;
            cursorManager.CharacterPosition = currentLineLength;
        }

        textRenderer.UpdateCurrentLineTextLayout(canvasText);

        scrollManager.EnsureHorizontalScrollBounds(canvasText, longestLineManager, true);

        int characterPos = cursorManager.CharacterPosition;
        if (characterPos > currentLineLength)
            characterPos = currentLineLength;

        int renderedCharacterPos = textRenderer.GetRenderedCharacterIndexForDocumentCharacter(cursorManager.LineNumber, characterPos);
        if (textRenderer.IsWordWrapEnabled && textRenderer.IsVirtualizedWrappedLine && renderedCharacterPos < 0)
            return;

        float withinLineRowOffset = 0;
        if (textRenderer.IsWordWrapEnabled && textRenderer.CurrentLineTextLayout != null && renderedCharacterPos >= 0)
        {
            float baseRowY = textRenderer.CurrentLineTextLayout.GetCaretPosition(0, false).Y;
            var vector = textRenderer.CurrentLineTextLayout.GetCaretPosition(renderedCharacterPos, false);
            int visualRow = (int)Math.Round((vector.Y - baseRowY) / Math.Max(1, textRenderer.SingleLineHeight));
            withinLineRowOffset = visualRow * textRenderer.SingleLineHeight;
        }

        float renderPosY = textRenderer.GetCurrentLineLayoutTopY(cursorManager.LineNumber) + withinLineRowOffset;
        
        bool offscreen = renderPosY > canvasCursor.ActualHeight || renderPosY + textRenderer.SingleLineHeight < 0;
        if (offscreen)
            return;

        // Draw the current line highlighter background first so it does not overdraw/tint the caret
        if (lineHighlighterRenderer.CanRender(focusManager))
            lineHighlighterRenderer.Render((float)canvasCursor.ActualWidth, renderPosY, textRenderer.SingleLineHeight, args, designHelper.LineHighlighterBrush);

        if (focusManager.HasFocus)
        {
            // Only paint the caret during the "on" phase of the blink.
            if (caretBlinkManager.IsCaretVisible && renderedCharacterPos >= 0)
            {
                float caretY = renderPosY + (textRenderer.SingleLineHeight - zoomManager.ZoomedFontSize) / 2f;
                RenderCursor(
                    textRenderer.CurrentLineTextLayout,
                    renderedCharacterPos,
                    textRenderer.IsWordWrapEnabled ? 0 : textRenderer.HorizontalOffset,
                    caretY,
                    zoomManager.ZoomedFontSize,
                    _CursorSize,
                    args,
                    designHelper.CursorColorBrush);
            }

            if (!cursorManager.Equals(cursorManager.currentCursorPosition, cursorManager.oldCursorPosition))
            {
                cursorManager.oldCursorPosition.SetChangeValues(cursorManager.currentCursorPosition);
                eventsManager.CallSelectionChanged();
            }
        }
    }
}
