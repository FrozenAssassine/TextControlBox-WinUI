using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Models.Structs;
using Windows.Foundation;

namespace TextControlBoxNS.Core.Renderer;

internal class TextRenderer
{
    public CanvasTextFormat TextFormat = null;
    public CanvasTextLayout DrawnTextLayout = null;
    public CanvasTextLayout CurrentLineTextLayout = null;


    public bool NeedsUpdateTextLayout = true;
    public bool NeedsTextFormatUpdate = true;
    public float SingleLineHeight { get => TextFormat == null ? 0 : TextFormat.LineSpacing; }

    public int NumberOfStartLine = 0;
    public int NumberOfRenderedLines = 0;
    public string RenderedText = "";
    public string OldRenderedText = null;
    public float VerticalDrawOffset { get; private set; } = 0;

    public double TopScrollOffset { get; private set; } = 0;
    public double BottomScrollOffset { get; private set; } = 0;

    private CursorManager cursorManager;
    private TextManager textManager;
    private ScrollManager scrollManager;
    private LineNumberRenderer lineNumberRenderer;
    private TextLayoutManager textLayoutManager;
    private DesignHelper designHelper;
    private Grid scrollGrid;
    private LongestLineManager longestLineManager;
    private SearchManager searchManager;
    private CoreTextControlBox coreTextbox;
    private CanvasUpdateManager canvasUpdateManager;
    private ZoomManager zoomManager;
    private WhitespaceCharactersRenderer invisibleCharactersRenderer;
    private LinkRenderer linkRenderer;
    private LinkHighlightManager linkHighlightManager;

    public void Init(
        CursorManager cursorManager,
        DesignHelper designHelper,
        TextLayoutManager textLayoutManager,
        TextManager textManager,
        ScrollManager scrollManager,
        LineNumberRenderer lineNumberRenderer,
        LongestLineManager longestLineManager,
        CoreTextControlBox textbox,
        SearchManager searchManager,
        CanvasUpdateManager canvasUpdateManager,
        ZoomManager zoomManager,
        WhitespaceCharactersRenderer invisibleCharactersRenderer,
        LinkRenderer linkRenderer,
        LinkHighlightManager linkHighlightManager)
    {
        this.cursorManager = cursorManager;
        this.textManager = textManager;
        this.designHelper = designHelper;
        this.textLayoutManager = textLayoutManager;
        this.scrollManager = scrollManager;
        this.lineNumberRenderer = lineNumberRenderer;
        this.longestLineManager = longestLineManager;
        this.searchManager = searchManager;
        this.coreTextbox = textbox;
        this.scrollGrid = textbox.scrollGrid;
        this.canvasUpdateManager = canvasUpdateManager;
        this.zoomManager = zoomManager;
        this.invisibleCharactersRenderer = invisibleCharactersRenderer;
        this.linkRenderer = linkRenderer;
        this.linkHighlightManager = linkHighlightManager;

        UpdateScrollOffset(coreTextbox.ContentVerticalScrollOffset);
    }

    public void UpdateScrollOffset(VerticalScrollOffset verticalScrollOffset)
    {
        this.TopScrollOffset = verticalScrollOffset.Top;
        this.BottomScrollOffset = verticalScrollOffset.Bottom;
    }

    //Check whether the current line is outside the bounds of the visible area
    public bool OutOfRenderedArea(int line)
    {
        return line < NumberOfStartLine || line >= NumberOfStartLine + NumberOfRenderedLines;
    }

    public void UpdateCurrentLineTextLayout(CanvasControl canvasText)
    {
        CurrentLineTextLayout =
            cursorManager.LineNumber < textManager.LinesCount ?
            textLayoutManager.CreateTextLayout(
            canvasText,
            TextFormat,
            textManager.GetLineText(cursorManager.LineNumber) + "|",
            canvasText.Size) :
            null;
    }
    public (int startLine, int linesToRender) CalculateLinesToRender()
    {
        var singleLineHeight = SingleLineHeight;

        //Measure text position and apply the value to the scrollbar
        scrollManager.verticalScrollBar.Maximum = ((textManager.LinesCount + 1) * singleLineHeight - scrollGrid.ActualHeight + BottomScrollOffset + TopScrollOffset) / scrollManager.DefaultVerticalScrollSensitivity;
        scrollManager.verticalScrollBar.ViewportSize = coreTextbox.canvasText.ActualHeight;

        //Calculate number of lines that need to be rendered
        int linesToRenderCount = (int)(coreTextbox.canvasText.ActualHeight / singleLineHeight);
        linesToRenderCount = Math.Min(Math.Max(linesToRenderCount, 1), textManager.LinesCount);

        int startLine = (int)(((scrollManager.VerticalScroll - VerticalDrawOffset) * scrollManager.DefaultVerticalScrollSensitivity - TopScrollOffset) / singleLineHeight);
        startLine = Math.Min(startLine, textManager.LinesCount);

        if (startLine < 0) startLine = 0;

        int linesToRender = Math.Min(linesToRenderCount, textManager.LinesCount - startLine);

        return (startLine, linesToRender);
    }

    public float CalculateDrawOffset()
    {
        double verticalScroll = scrollManager.VerticalScroll;

        double scrollCoeff = scrollManager.verticalScrollBar.Maximum / scrollManager.VerticalScroll;

        double realScrollPosition = verticalScroll * scrollManager.DefaultVerticalScrollSensitivity - TopScrollOffset;
        double preCalcOffset = realScrollPosition < 0 ? -realScrollPosition : SingleLineHeight;

        float drawOffset = (float)(preCalcOffset < SingleLineHeight ? SingleLineHeight : Math.Floor(preCalcOffset / SingleLineHeight) * SingleLineHeight);

        if (drawOffset > SingleLineHeight)
        {
            if (scrollCoeff == 1)
            {
                drawOffset = SingleLineHeight;
            }
        }
        return drawOffset - SingleLineHeight;
    }


    public void Draw(CanvasControl canvasText, CanvasDrawEventArgs args)
    {
        VerticalDrawOffset = CalculateDrawOffset();

        //Create resources and layouts:
        if (NeedsTextFormatUpdate || TextFormat == null || lineNumberRenderer.LineNumberTextFormat == null)
        {
            lineNumberRenderer.CreateLineNumberTextFormat();
            TextFormat = textLayoutManager.CreateCanvasTextFormat();

            invisibleCharactersRenderer.UpdateTextFormat(canvasText, TextFormat);

            designHelper.CreateColorResources(args.DrawingSession);
        }

        (NumberOfStartLine, NumberOfRenderedLines) = CalculateLinesToRender();
        RenderedText = textManager.GetLinesAsString(NumberOfStartLine, NumberOfRenderedLines);

        //check rendering and calculation updates
        lineNumberRenderer.CheckGenerateLineNumberText();

        CanvasCommandList canvasCommandList = new CanvasCommandList(args.DrawingSession);
        if (OldRenderedText != null &&
            OldRenderedText.Length != RenderedText.Length ||
            !RenderedText.Equals(OldRenderedText, System.StringComparison.Ordinal) ||
            NeedsUpdateTextLayout
        )
        {
            NeedsUpdateTextLayout = false;
            OldRenderedText = RenderedText;

            DrawnTextLayout = textLayoutManager.CreateTextResource(canvasText, DrawnTextLayout, TextFormat, RenderedText, new Size { Height = canvasText.Size.Height, Width = coreTextbox.ActualWidth });
            SyntaxHighlightingRenderer.UpdateSyntaxHighlighting(DrawnTextLayout, designHelper._AppTheme, textManager._SyntaxHighlighting, coreTextbox.EnableSyntaxHighlighting, RenderedText);
        }

        scrollManager.EnsureHorizontalScrollBounds(canvasText, longestLineManager, false, zoomManager.ZoomNeedsRecalculateLongestLine);
        if (zoomManager.ZoomNeedsRecalculateLongestLine)
            zoomManager.ZoomNeedsRecalculateLongestLine = false;

        if (linkHighlightManager.HighlightLinks)
        {
            linkHighlightManager.FindAndComputeLinkPositions();
            linkRenderer.HighlightLinks();
        }

        using (var ccls = canvasCommandList.CreateDrawingSession())
        {
            //Only update the textformat when the text changes:
            //render the search highlights
            if (searchManager.IsSearchOpen)
                SearchHighlightsRenderer.RenderHighlights(
                    args,
                    ccls,
                    DrawnTextLayout,
                    RenderedText,
                    searchManager.MatchingSearchLines,
                    searchManager.searchParameter.SearchExpression,
                    (float)-scrollManager.HorizontalScroll,
                    SingleLineHeight / scrollManager.DefaultVerticalScrollSensitivity + VerticalDrawOffset,
                    designHelper._Design.SearchHighlightColor
                    );

            ccls.DrawTextLayout(DrawnTextLayout, (float)-scrollManager.HorizontalScroll, VerticalDrawOffset + SingleLineHeight, designHelper.TextColorBrush);

            invisibleCharactersRenderer.DrawTabsAndSpaces(args, ccls, RenderedText, DrawnTextLayout, VerticalDrawOffset + SingleLineHeight);
        }
        args.DrawingSession.DrawImage(canvasCommandList);




        //Only update if needed, to reduce updates when scrolling
        if (lineNumberRenderer.CanUpdateCanvas())
        {
            canvasUpdateManager.UpdateLineNumbers();
        }
        canvasUpdateManager.UpdateSelection(); // Possible bad for performanse
        canvasUpdateManager.UpdateCursor(); // Possible bad for performanse
    }
}
