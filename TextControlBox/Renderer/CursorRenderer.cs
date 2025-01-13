using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Numerics;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Text;

namespace TextControlBoxNS.Renderer;

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
        EventsManager eventsManager)
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
    }

    public void RenderCursor(CanvasTextLayout textLayout, int characterPosition, float xOffset, float y, float fontSize, CursorSize customSize, CanvasDrawEventArgs args, CanvasSolidColorBrush cursorColorBrush)
    {
        if (textLayout == null)
            return;

        Vector2 vector = textLayout.GetCaretPosition(characterPosition < 0 ? 0 : characterPosition, false);
        if (customSize == null)
            args.DrawingSession.FillRectangle(vector.X + xOffset, y, 1, fontSize, cursorColorBrush);
        else
            args.DrawingSession.FillRectangle(vector.X + xOffset + customSize.OffsetX, y + customSize.OffsetY, (float)customSize.Width, (float)customSize.Height, cursorColorBrush);
    }

    public void Draw(CanvasControl canvasText, CanvasControl canvasCursor, CanvasDrawEventArgs args)
    {
        currentLineManager.UpdateCurrentLine(cursorManager.LineNumber);
        if (textRenderer.DrawnTextLayout == null || !focusManager.HasFocus)
            return;

        int currentLineLength = currentLineManager.Length;
        if (cursorManager.LineNumber >= textManager.LinesCount)
        {
            cursorManager.LineNumber = textManager.LinesCount - 1;
            cursorManager.CharacterPosition = currentLineLength;
        }

        //Calculate the distance to the top for the cursorposition and render the cursor
        float renderPosY = (float)((cursorManager.LineNumber - textRenderer.NumberOfStartLine) * textRenderer.SingleLineHeight) + textRenderer.SingleLineHeight / scrollManager.DefaultVerticalScrollSensitivity;

        //Out of display-region:
        if (renderPosY > textRenderer.NumberOfRenderedLines * textRenderer.SingleLineHeight || renderPosY < 0)
            return;

        textRenderer.UpdateCurrentLineTextLayout(canvasText);

        int characterPos = cursorManager.CharacterPosition;
        if (characterPos > currentLineLength)
            characterPos = currentLineLength;

        RenderCursor(
            textRenderer.CurrentLineTextLayout,
            characterPos,
            (float)-scrollManager.HorizontalScroll,
            renderPosY,
            zoomManager.ZoomedFontSize,
            _CursorSize,
            args,
            designHelper.CursorColorBrush);

        if(lineHighlighterRenderer.CanRender())
            lineHighlighterRenderer.Render((float)canvasCursor.ActualWidth, renderPosY, zoomManager.ZoomedFontSize, args, designHelper.LineHighlighterBrush);

        if (!cursorManager.Equals(cursorManager.currentCursorPosition, cursorManager.oldCursorPosition))
        {
            cursorManager.oldCursorPosition = new CursorPosition(cursorManager.currentCursorPosition);
            eventsManager.CallSelectionChanged();
        }
    }
}
