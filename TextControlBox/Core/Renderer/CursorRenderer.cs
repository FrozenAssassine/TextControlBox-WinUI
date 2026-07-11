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
        // vector.Y is the caret's row within the (possibly wrapped) layout — 0 for a single-row line, so this
        // is unchanged in non-wrap mode and follows the wrapped row in wrap mode.
        if (customSize == null)
            args.DrawingSession.FillRectangle(vector.X + xOffset, y + vector.Y, 2, fontSize, cursorColorBrush);
        else
            args.DrawingSession.FillRectangle(vector.X + xOffset + customSize.OffsetX, y + vector.Y + customSize.OffsetY, (float)customSize.Width, (float)customSize.Height, cursorColorBrush);
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

        // In wrap mode the caret's line sits at its visual-row top (GetLineTopY), matching the wrapped text
        // draw offset; in non-wrap mode this reduces to the original document-line position.
        float topInset = textRenderer.SingleLineHeight / scrollManager.DefaultVerticalScrollSensitivity;
        float renderPosY = textRenderer.IsWordWrapEnabled
            ? textRenderer.GetLineTopY(cursorManager.LineNumber) + textRenderer.SingleLineHeight + topInset
            : (float)((cursorManager.LineNumber - textRenderer.NumberOfStartLine) * textRenderer.SingleLineHeight) + topInset;
        bool offscreen = textRenderer.IsWordWrapEnabled
            ? (renderPosY > canvasCursor.ActualHeight || renderPosY + textRenderer.SingleLineHeight < 0)
            : (renderPosY > textRenderer.NumberOfRenderedLines * textRenderer.SingleLineHeight || renderPosY < 0);
        if (offscreen)
            return;

        textRenderer.UpdateCurrentLineTextLayout(canvasText);

        scrollManager.EnsureHorizontalScrollBounds(canvasText, longestLineManager, true);


        if (focusManager.HasFocus)
        {
            int characterPos = cursorManager.CharacterPosition;
            if (characterPos > currentLineLength)
                characterPos = currentLineLength;

            // Map the caret's document column into the (possibly sliced) current-line layout and shift its x
            // by the slice pixel offset via HorizontalOffset. Both are no-ops when horizontal virtualization
            // is inactive, so this path is unchanged for ordinary files.
            int renderedCharacterPos = textRenderer.GetRenderedCharacterIndexForDocumentCharacter(cursorManager.LineNumber, characterPos);

            // Only paint the caret during the "on" phase of the blink. The current-line highlighter
            // below stays unconditional so the highlighted line does not flicker while the caret blinks.
            if (caretBlinkManager.IsCaretVisible && renderedCharacterPos >= 0)
            {
                RenderCursor(
                    textRenderer.CurrentLineTextLayout,
                    renderedCharacterPos,
                    textRenderer.IsWordWrapEnabled ? 0 : textRenderer.HorizontalOffset,
                    renderPosY,
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

        if (lineHighlighterRenderer.CanRender(focusManager))
            lineHighlighterRenderer.Render((float)canvasCursor.ActualWidth, renderPosY, zoomManager.ZoomedFontSize, args, designHelper.LineHighlighterBrush);
    }
}
