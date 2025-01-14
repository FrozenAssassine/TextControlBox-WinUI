using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Extensions;
using TextControlBoxNS.Helper;
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
    public IEnumerable<string> RenderedLines;
    public string RenderedText = "";
    public string OldRenderedText = null;

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
        CanvasUpdateManager canvasUpdateManager)
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

    public void Draw(CanvasControl canvasText, CanvasDrawEventArgs args)
    {
        //Create resources and layouts:
        if (NeedsTextFormatUpdate || TextFormat == null || lineNumberRenderer.LineNumberTextFormat == null)
        {
            lineNumberRenderer.CreateLineNumberTextFormat();
            TextFormat = textLayoutManager.CreateCanvasTextFormat();
        }

        designHelper.CreateColorResources(args.DrawingSession);

        //Measure textposition and apply the value to the scrollbar
        scrollManager.verticalScrollBar.Maximum = ((textManager.LinesCount + 1) * SingleLineHeight - scrollGrid.ActualHeight) / scrollManager.DefaultVerticalScrollSensitivity;
        scrollManager.verticalScrollBar.ViewportSize = canvasText.ActualHeight;

        //Calculate number of lines that needs to be rendered
        NumberOfRenderedLines = (int)(canvasText.ActualHeight / SingleLineHeight);
        NumberOfStartLine = (int)((scrollManager.VerticalScroll * scrollManager.DefaultVerticalScrollSensitivity) / SingleLineHeight);

        //Get all the lines, that need to be rendered, from the list
        NumberOfRenderedLines = NumberOfRenderedLines + NumberOfStartLine > textManager.LinesCount ? textManager.LinesCount : NumberOfRenderedLines;

        RenderedLines = textManager.GetLines(NumberOfStartLine, NumberOfRenderedLines);
        RenderedText = RenderedLines.GetString("\n");

        lineNumberRenderer.CheckGenerateLineNumberText();

        longestLineManager.CheckRecalculateLongestLine();

        string longestLineText = textManager.totalLines.GetLineText(longestLineManager.longestIndex);
        longestLineManager.longestLineLength = longestLineText.Length;
        Size lineLength = Utils.MeasureLineLenght(CanvasDevice.GetSharedDevice(), longestLineText, TextFormat);

        //Measure horizontal Width of longest line and apply to scrollbar
        scrollManager.horizontalScrollBar.Maximum = (lineLength.Width <= canvasText.ActualWidth ? 0 : lineLength.Width - canvasText.ActualWidth + 50);
        scrollManager.horizontalScrollBar.ViewportSize = canvasText.ActualWidth;
        scrollManager.ScrollIntoViewHorizontal(canvasText);

        //Only update the textformat when the text changes:
        if (OldRenderedText != RenderedText || NeedsUpdateTextLayout)
        {
            NeedsUpdateTextLayout = false;
            OldRenderedText = RenderedText;

            DrawnTextLayout = textLayoutManager.CreateTextResource(canvasText, DrawnTextLayout, TextFormat, RenderedText, new Size { Height = canvasText.Size.Height, Width = coreTextbox.ActualWidth });
            SyntaxHighlightingRenderer.UpdateSyntaxHighlighting(DrawnTextLayout, designHelper._AppTheme, textManager._CodeLanguage, coreTextbox.SyntaxHighlighting, RenderedText);
        }

        //render the search highlights
        if (searchManager.IsSearchOpen)
            SearchHighlightsRenderer.RenderHighlights(
                args, 
                DrawnTextLayout,
                RenderedText, 
                searchManager.MatchingSearchLines, 
                searchManager.searchParameter.SearchExpression, 
                (float)-scrollManager.HorizontalScroll, 
                SingleLineHeight / scrollManager.DefaultVerticalScrollSensitivity, 
                designHelper._Design.SearchHighlightColor
                );

        args.DrawingSession.DrawTextLayout(DrawnTextLayout, (float)-scrollManager.HorizontalScroll, SingleLineHeight, designHelper.TextColorBrush);

        //Only update if needed, to reduce updates when scrolling
        if (lineNumberRenderer.CanUpdateCanvas())
        {
            canvasUpdateManager.UpdateLineNumbers();
        }
    }


}
