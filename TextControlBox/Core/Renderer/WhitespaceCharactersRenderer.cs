
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using TextControlBoxNS.Helper;

namespace TextControlBoxNS.Core.Renderer;

internal class WhitespaceCharactersRenderer
{
    private DesignHelper designHelper;
    private ScrollManager scrollManager;
    private ZoomManager zoomManager;
    private TextLayoutManager textLayoutManager;
    private WhitespaceCharactersManager whitespaceCharactersManager;

    private CanvasTextLayout SpaceGlyph = null;
    private CanvasTextLayout TabGlyph = null;

    public void Init(DesignHelper designHelper, 
        ScrollManager scrollManager, 
        ZoomManager zoomManager, 
        TextLayoutManager textLayoutManager,
        WhitespaceCharactersManager whitespaceCharactersManager
        )
    {
        this.designHelper = designHelper;
        this.scrollManager = scrollManager;
        this.zoomManager = zoomManager;
        this.textLayoutManager = textLayoutManager;
        this.whitespaceCharactersManager = whitespaceCharactersManager;
    }

    public void UpdateTextFormat(CanvasControl canvasText, CanvasTextFormat canvasTextFormat)
    {
        (SpaceGlyph, TabGlyph) = textLayoutManager.CreateGlyphs(canvasText, canvasTextFormat);
    }

    public void DrawTabsAndSpaces(
        CanvasDrawEventArgs args, 
        CanvasDrawingSession drawingSession,
        string renderedText, 
        CanvasTextLayout drawnTextLayout, 
        float SingleLineHeight
        )
    {
        if (!whitespaceCharactersManager.ShowWhitespaceCharacters)
            return;

        //do not render the images directly, add all of them to the
        //drawingSession which consists out of a CanvasCommandList 

        var color = designHelper._Design.InvisibleCharacterColor;

        float x, y;
        char c;
        for (int i = 0; i < renderedText.Length; i++)
        {
            c = renderedText[i];
            if (c == ' ' || c == '\t')
            {
                var caretPos = drawnTextLayout.GetCaretPosition(i, false);
                x = caretPos.X - (float)scrollManager.HorizontalScroll;
                y = caretPos.Y + (SingleLineHeight + zoomManager.ZoomedFontSize - zoomManager.ZoomedFontSize / 8);

                if (c == ' ')
                {
                    drawingSession.DrawTextLayout(SpaceGlyph, x, y, color);
                }
                else if (c == '\t')
                {
                    drawingSession.DrawTextLayout(TabGlyph, x + (zoomManager._ZoomFactor / 50), y, color);
                }
            }
        }
    }
}
