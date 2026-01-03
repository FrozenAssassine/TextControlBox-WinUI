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
        LongestLineManager longestLineManager)
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

        var (startLine, linesToRender) = textRenderer.CalculateLinesToRender();
        float singleLineHeight = textRenderer.SingleLineHeight;

        //Calculate the distance to the top for the cursorposition and render the cursor
        float renderPosY = (float)((cursorManager.LineNumber - startLine) * singleLineHeight) + singleLineHeight / scrollManager.DefaultVerticalScrollSensitivity + textRenderer.VerticalDrawOffset;

        //Out of display-region:
        if (renderPosY > linesToRender * singleLineHeight + textRenderer.VerticalDrawOffset || renderPosY < 0)
            return;

        textRenderer.UpdateCurrentLineTextLayout(canvasText);

        scrollManager.EnsureHorizontalScrollBounds(canvasText, longestLineManager, true);


        if (focusManager.HasFocus)
        {
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
