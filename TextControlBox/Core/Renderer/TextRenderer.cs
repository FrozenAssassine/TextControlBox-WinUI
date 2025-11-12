using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using TextControlBoxNS.Core.Text;
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
    public string RenderedText = "";
    public string OldRenderedText = null;

    // Smooth scrolling properties
    private double targetVerticalScroll = 0;
    private double currentVerticalScroll = 0;
    private const double SMOOTH_SCROLL_SPEED = 0.15; // Interpolation factor (0-1, higher = faster)
    private bool isSmoothScrolling = false;
    private int lastRenderedStartLine = -1; // Track which line we last rendered from
    private TranslateTransform contentTransform;

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
        WhitespaceCharactersRenderer invisibleCharactersRenderer)
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

        currentVerticalScroll = scrollManager.VerticalScroll * scrollManager.DefaultVerticalScrollSensitivity;
        targetVerticalScroll = currentVerticalScroll;

        contentTransform = new TranslateTransform();
        scrollGrid.RenderTransform = contentTransform;
    }

    public void SetTargetScroll(double scrollValue)
    {
        targetVerticalScroll = scrollValue * scrollManager.DefaultVerticalScrollSensitivity;

        if (Math.Abs(targetVerticalScroll - currentVerticalScroll) > 0.1)
        {
            isSmoothScrolling = true;
            coreTextbox.canvasText.Invalidate();
        }
    }

    // Update smooth scroll interpolation
    private void UpdateSmoothScroll()
    {
        if (!isSmoothScrolling)
            return;

        double diff = targetVerticalScroll - currentVerticalScroll;

        if (Math.Abs(diff) < 0.5)
        {
            currentVerticalScroll = targetVerticalScroll;
            isSmoothScrolling = false;
        }
        else
        {
            currentVerticalScroll += diff * SMOOTH_SCROLL_SPEED;
            coreTextbox.canvasText.Invalidate();
        }
    }

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
    public (int startLine, int linesToRender, float verticalOffset) CalculateLinesToRender()
    {
        var singleLineHeight = SingleLineHeight;

        // Update scrollbar bounds
        double totalTextHeight = textManager.LinesCount * singleLineHeight;
        scrollManager.verticalScrollBar.Maximum =
            Math.Max(0, (totalTextHeight - scrollGrid.ActualHeight + singleLineHeight) / scrollManager.DefaultVerticalScrollSensitivity);
        scrollManager.verticalScrollBar.ViewportSize = coreTextbox.canvasText.ActualHeight;

        // Smooth scroll pixel offset
        float scrollPixels = (float)currentVerticalScroll;

        // Starting line index
        int startLine = (int)(scrollPixels / singleLineHeight);
        startLine = Math.Max(0, Math.Min(startLine, textManager.LinesCount));

        // Fractional offset (negative for proper alignment)
        float verticalOffset = -(scrollPixels % singleLineHeight);

        // Number of visible lines (include small buffer)
        int linesToRenderCount = (int)Math.Ceiling(coreTextbox.canvasText.ActualHeight / singleLineHeight) + 3;
        linesToRenderCount = Math.Min(linesToRenderCount, textManager.LinesCount - startLine);

        return (startLine, linesToRenderCount, verticalOffset);
    }

    public void Draw(CanvasControl canvasText, CanvasDrawEventArgs args)
    {
        SetTargetScroll(scrollManager.verticalScrollBar.Value);

        // Update smooth scrolling animation
        UpdateSmoothScroll();

        //Create resources and layouts:
        if (NeedsTextFormatUpdate || TextFormat == null || lineNumberRenderer.LineNumberTextFormat == null)
        {
            lineNumberRenderer.CreateLineNumberTextFormat();
            TextFormat = textLayoutManager.CreateCanvasTextFormat();

            invisibleCharactersRenderer.UpdateTextFormat(canvasText, TextFormat);

            designHelper.CreateColorResources(args.DrawingSession);
        }

        (NumberOfStartLine, NumberOfRenderedLines, float verticalOffset) = CalculateLinesToRender();
        RenderedText = textManager.GetLinesAsString(NumberOfStartLine, NumberOfRenderedLines);

        if (contentTransform != null)
        {
            contentTransform.Y = verticalOffset;
        }


        //check rendering and calculation updates
        lineNumberRenderer.CheckGenerateLineNumberText();

        // Only regenerate the text layout when we've crossed into a new line boundary
        // or when text content actually changed
        bool needsRegenerateLayout =
            lastRenderedStartLine != NumberOfStartLine ||
            (OldRenderedText != null &&
             (OldRenderedText.Length != RenderedText.Length ||
              !RenderedText.Equals(OldRenderedText, System.StringComparison.Ordinal))) ||
            NeedsUpdateTextLayout;

        CanvasCommandList canvasCommandList = new CanvasCommandList(args.DrawingSession);

        if (needsRegenerateLayout)
        {
            NeedsUpdateTextLayout = false;
            OldRenderedText = RenderedText;
            lastRenderedStartLine = NumberOfStartLine;

            DrawnTextLayout = textLayoutManager.CreateTextResource(canvasText, DrawnTextLayout, TextFormat, RenderedText, new Size { Height = canvasText.Size.Height, Width = coreTextbox.ActualWidth });
            SyntaxHighlightingRenderer.UpdateSyntaxHighlighting(DrawnTextLayout, designHelper._AppTheme, textManager._SyntaxHighlighting, coreTextbox.EnableSyntaxHighlighting, RenderedText);
        }

        scrollManager.EnsureHorizontalScrollBounds(canvasText, longestLineManager, false, zoomManager.ZoomNeedsRecalculateLongestLine);
        if (zoomManager.ZoomNeedsRecalculateLongestLine)
            zoomManager.ZoomNeedsRecalculateLongestLine = false;

        using (var ccls = canvasCommandList.CreateDrawingSession())
        {
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
                    SingleLineHeight / scrollManager.DefaultVerticalScrollSensitivity,
                    designHelper._Design.SearchHighlightColor
                    );

            // Draw text at fixed position - the offset will be applied to the entire image
            ccls.DrawTextLayout(
                DrawnTextLayout,
                (float)-scrollManager.HorizontalScroll,
                SingleLineHeight,  // Fixed Y position
                designHelper.TextColorBrush
            );

            invisibleCharactersRenderer.DrawTabsAndSpaces(args, ccls, RenderedText, DrawnTextLayout, SingleLineHeight);
        }

        // Apply the smooth vertical offset by translating the entire rendered image
        args.DrawingSession.DrawImage(canvasCommandList);

        //Only update if needed, to reduce updates when scrolling
        if (lineNumberRenderer.CanUpdateCanvas())
        {
            canvasUpdateManager.UpdateLineNumbers();
        }
    }
}