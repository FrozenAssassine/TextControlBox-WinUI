using System;
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
    private float _lastGlyphFontSize = 0;

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

    public void UpdateTextFormat(ICanvasResourceCreator resourceCreator, CanvasTextFormat canvasTextFormat)
    {
        if (canvasTextFormat == null)
            return;

        SpaceGlyph?.Dispose();
        TabGlyph?.Dispose();
        SpaceGlyph = null;
        TabGlyph = null;
        _lastGlyphFontSize = 0;

        if (resourceCreator is CanvasControl cc && !cc.ReadyToDraw)
            return;

        try
        {
            (SpaceGlyph, TabGlyph) = textLayoutManager.CreateGlyphs(resourceCreator, canvasTextFormat);
            _lastGlyphFontSize = canvasTextFormat.FontSize;
        }
        catch
        {
            // CanvasControl or device is not ready yet during early layout/scrollbar load;
            // glyphs will be lazily created on draw using drawingSession.
            SpaceGlyph = null;
            TabGlyph = null;
            _lastGlyphFontSize = 0;
        }
    }
    
    public void CheckDispose()
    {
        SpaceGlyph?.Dispose();
        TabGlyph?.Dispose();
        SpaceGlyph = null;
        TabGlyph = null;
        _lastGlyphFontSize = 0;
    }

    public void DrawTabsAndSpaces(
        CanvasDrawEventArgs args, 
        CanvasDrawingSession drawingSession,
        string renderedText, 
        CanvasTextLayout drawnTextLayout, 
        float drawTextOffsetX,
        float topRenderingOffset,
        CanvasTextFormat canvasTextFormat
        )
    {
        if (!whitespaceCharactersManager.ShowWhitespaceCharacters)
            return;

        //do not render the images directly, add all of them to the
        //drawingSession which consists out of a CanvasCommandList 

        float currentFontSize = zoomManager?.ZoomedFontSize > 0 ? zoomManager.ZoomedFontSize : (canvasTextFormat?.FontSize ?? 14f);
        if (SpaceGlyph == null || TabGlyph == null || Math.Abs(_lastGlyphFontSize - currentFontSize) > 0.01f)
        {
            if (canvasTextFormat != null)
            {
                UpdateTextFormat(drawingSession, canvasTextFormat);
            }
            else
            {
                return;
            }
        }

        var color = designHelper._Design.InvisibleCharacterColor;

        float x, y;
        char c;
        for (int i = 0; i < renderedText.Length; i++)
        {
            c = renderedText[i];
            if (c == ' ' || c == '\t')
            {
                var caretPos = drawnTextLayout.GetCaretPosition(i, false);
                x = caretPos.X + drawTextOffsetX;
                y = caretPos.Y + (topRenderingOffset + currentFontSize - currentFontSize / 8);

                if (c == ' ')
                {
                    drawingSession.DrawTextLayout(SpaceGlyph, x, y, color);
                }
                else if (c == '\t')
                {
                    drawingSession.DrawTextLayout(TabGlyph, x + ((zoomManager?._ZoomFactor ?? 100) / 50f), y, color);
                }
            }
        }
    }
}
