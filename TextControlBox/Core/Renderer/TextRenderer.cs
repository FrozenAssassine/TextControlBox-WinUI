using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Models;
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
    public float HorizontalOffset => (float)-scrollManager.HorizontalScroll;
    public int NumberOfStartLine = 0;
    public int NumberOfRenderedLines = 0;
    public string RenderedText = "";
    public string OldRenderedText = null;
    public float OldLayoutWidth = -1f;
    public VisualLineMap VisualLineMap { get; } = new VisualLineMap();

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

    public bool WordWrapEnabled => coreTextbox.WordWrap;
    public float VerticalScrollPixels => (float)(scrollManager.VerticalScroll * scrollManager.DefaultVerticalScrollSensitivity);
    public float TextRenderOffsetY => SingleLineHeight;

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
    }

    public void CheckDispose()
    {
        TextFormat?.Dispose();
        DrawnTextLayout?.Dispose();
        CurrentLineTextLayout?.Dispose();
        invisibleCharactersRenderer.CheckDispose();
    }

    //Check whether the current line is outside the bounds of the visible area
    public bool OutOfRenderedArea(int line)
    {
        if (WordWrapEnabled)
        {
            int visualLineIndex = VisualLineMap.GetVisualLineIndex(line);
            if (visualLineIndex < 0)
                return true;

            return visualLineIndex < NumberOfStartLine || visualLineIndex >= NumberOfStartLine + NumberOfRenderedLines;
        }

        return line < NumberOfStartLine || line >= NumberOfStartLine + NumberOfRenderedLines;
    }

    public Point ToLayoutPoint(Point canvasPoint)
    {
        return new Point(
            canvasPoint.X - HorizontalOffset,
            canvasPoint.Y - TextRenderOffsetY + VerticalScrollPixels);
    }

    public bool TryGetCursorPositionFromPoint(Point canvasPoint, out int line, out int character)
    {
        line = 0;
        character = 0;

        if (!WordWrapEnabled || DrawnTextLayout == null)
            return false;

        // Verwende das Layout selbst für das HitTesting.
        // Das ist deutlich robuster als eine Division durch `SingleLineHeight`, da tatsächliche
        // Zeilenhöhen (Font-Metriken, LineSpacing, DPI) sonst zu kumulativen Offsets führen können.

        var layoutPoint = ToLayoutPoint(canvasPoint);

        float x = (float)Math.Max(0, layoutPoint.X);
        float y = (float)Math.Max(0, layoutPoint.Y);

        DrawnTextLayout.HitTest(x, y, out var region);

        int globalIndex = region.CharacterIndex;
        if (RenderedText != null)
            globalIndex = Math.Clamp(globalIndex, 0, RenderedText.Length);

        (line, character) = textManager.GetLinePositionFromGlobalIndex(globalIndex);
        return true;
    }

    public bool TryGetCursorCaretPosition(out System.Numerics.Vector2 caretPosition)
    {
        caretPosition = default;
        if (DrawnTextLayout == null)
            return false;

        int globalIndex = textManager.GetGlobalIndex(cursorManager.LineNumber, cursorManager.CharacterPosition);
        caretPosition = DrawnTextLayout.GetCaretPosition(globalIndex, false);
        return true;
    }

    public bool MoveCursorByVisualLines(int visualLineDelta)
    {
        if (!WordWrapEnabled || DrawnTextLayout == null)
            return false;

        int globalIndex = textManager.GetGlobalIndex(cursorManager.LineNumber, cursorManager.CharacterPosition);

        if (!VisualLineMap.TryGetVisualPosition(globalIndex, out int visualLineIndex, out int column))
            return false;

        int targetVisualLine = visualLineIndex + visualLineDelta;

        if (targetVisualLine < 0)
        {
            cursorManager.SetCursorPosition(0, 0);
            return true;
        }

        if (targetVisualLine >= VisualLineMap.TotalVisualLines)
        {
            int lastLine = Math.Max(0, textManager.LinesCount - 1);
            cursorManager.SetCursorPosition(lastLine, textManager.GetLineLength(lastLine));
            return true;
        }

        var caret = DrawnTextLayout.GetCaretPosition(globalIndex, false);
        int targetGlobalStart = VisualLineMap.VisualLines[targetVisualLine].GlobalStartIndex;
        var targetLineCaret = DrawnTextLayout.GetCaretPosition(targetGlobalStart, false);

        float targetLineHeight = DrawnTextLayout.LineMetrics[targetVisualLine].Height;
        float targetY = targetLineCaret.Y + (targetLineHeight / 2);
        DrawnTextLayout.HitTest(caret.X, targetY, out var region);

        int resultIndex = region.CharacterIndex;
        if (RenderedText != null)
            resultIndex = Math.Clamp(resultIndex, 0, RenderedText.Length);

        if (resultIndex == globalIndex && VisualLineMap.TryGetLogicalPosition(targetVisualLine, column, out int fallbackLine, out int fallbackCharacter))
        {
            cursorManager.SetCursorPosition(fallbackLine, fallbackCharacter);
            return true;
        }

        var (line, character) = textManager.GetLinePositionFromGlobalIndex(resultIndex);
        cursorManager.SetCursorPosition(line, character);
        return true;
    }

    public Point GetCursorCanvasPosition(CursorPosition cursorPosition)
    {
        if (!WordWrapEnabled || DrawnTextLayout == null)
        {
            return new Point
            {
                Y = (float)((cursorPosition.LineNumber - NumberOfStartLine) * SingleLineHeight) + SingleLineHeight / scrollManager.DefaultVerticalScrollSensitivity,
                X = CursorHelper.GetCursorPositionInLine(CurrentLineTextLayout, cursorPosition, 0)
            };
        }

        int globalIndex = textManager.GetGlobalIndex(cursorPosition.LineNumber, cursorPosition.CharacterPosition);
        var caret = DrawnTextLayout.GetCaretPosition(globalIndex, false);
        return new Point(caret.X + HorizontalOffset, caret.Y - VerticalScrollPixels);
    }

    public void UpdateCurrentLineTextLayout(CanvasControl canvasText)
    {
        if (WordWrapEnabled)
            return;

        CurrentLineTextLayout?.Dispose();
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
        int linesToRenderCount;
        int startLine;
        int linesToRender;

        if (WordWrapEnabled)
        {
            int totalVisualLines = VisualLineMap.TotalVisualLines;
            if (totalVisualLines == 0)
                return (0, 0);

            scrollManager.verticalScrollBar.Maximum = ((totalVisualLines + 1) * singleLineHeight - scrollGrid.ActualHeight) / scrollManager.DefaultVerticalScrollSensitivity;
            scrollManager.verticalScrollBar.ViewportSize = coreTextbox.canvasText.ActualHeight;

            linesToRenderCount = (int)(coreTextbox.canvasText.ActualHeight / singleLineHeight) + 1;
            startLine = (int)((scrollManager.VerticalScroll * scrollManager.DefaultVerticalScrollSensitivity) / singleLineHeight);
            startLine = Math.Clamp(startLine, 0, Math.Max(0, totalVisualLines - 1));
            linesToRender = Math.Min(linesToRenderCount, Math.Max(0, totalVisualLines - startLine));
            return (startLine, linesToRender);
        }

        //Measure text position and apply the value to the scrollbar
        scrollManager.verticalScrollBar.Maximum = ((textManager.LinesCount + 1) * singleLineHeight - scrollGrid.ActualHeight) / scrollManager.DefaultVerticalScrollSensitivity;
        scrollManager.verticalScrollBar.ViewportSize = coreTextbox.canvasText.ActualHeight;

        //Calculate number of lines that need to be rendered
        linesToRenderCount = (int)(coreTextbox.canvasText.ActualHeight / singleLineHeight);
        linesToRenderCount = Math.Min(linesToRenderCount, textManager.LinesCount);

        startLine = (int)((scrollManager.VerticalScroll * scrollManager.DefaultVerticalScrollSensitivity) / singleLineHeight);
        startLine = Math.Min(startLine, textManager.LinesCount);

        linesToRender = Math.Min(linesToRenderCount, textManager.LinesCount - startLine);

        return (startLine, linesToRender);
    }

    public void Draw(CanvasControl canvasText, CanvasDrawEventArgs args)
    {
        //Create resources and layouts:
        if (NeedsTextFormatUpdate || TextFormat == null || lineNumberRenderer.LineNumberTextFormat == null)
        {
            lineNumberRenderer.CreateLineNumberTextFormat();

            TextFormat?.Dispose();
            TextFormat = textLayoutManager.CreateCanvasTextFormat(
                zoomManager.ZoomedFontSize,
                zoomManager.ZoomedFontSize + 2,
                textManager._FontFamily,
                WordWrapEnabled ? CanvasWordWrapping.Wrap : CanvasWordWrapping.NoWrap);

            invisibleCharactersRenderer.UpdateTextFormat(canvasText, TextFormat);

            designHelper.CreateColorResources(args.DrawingSession);
        }

        LineSliceResult renderTextData;
        if (WordWrapEnabled)
        {
            RenderedText = textManager.GetLinesAsString();
            renderTextData = new LineSliceResult(RenderedText, textManager.totalLines.Span);
        }
        else
        {
            (NumberOfStartLine, NumberOfRenderedLines) = CalculateLinesToRender();
            renderTextData = textManager.GetLinesForRendering(NumberOfStartLine, NumberOfRenderedLines);
            RenderedText = renderTextData.Text;
        }

        using CanvasCommandList canvasCommandList = new CanvasCommandList(args.DrawingSession);

        float currentLayoutWidth = (float)Math.Max(0, canvasText.ActualWidth);

        if ((OldRenderedText != null && OldRenderedText.Length != RenderedText.Length)
            || !RenderedText.Equals(OldRenderedText, StringComparison.Ordinal)
            || NeedsUpdateTextLayout
            || (WordWrapEnabled && OldLayoutWidth != currentLayoutWidth)
        )
        {
            NeedsUpdateTextLayout = false;
            OldRenderedText = RenderedText;
            OldLayoutWidth = currentLayoutWidth;

            float layoutWidth = currentLayoutWidth;
            float layoutHeight = WordWrapEnabled ? float.MaxValue : (float)canvasText.Size.Height;
            DrawnTextLayout = textLayoutManager.CreateTextResource(canvasText, DrawnTextLayout, TextFormat, RenderedText, new Size { Height = layoutHeight, Width = layoutWidth });
            if (WordWrapEnabled)
                VisualLineMap.Update(DrawnTextLayout, textManager);

            SyntaxHighlightingRenderer.UpdateSyntaxHighlighting(renderTextData, textManager.NewLineCharacter, DrawnTextLayout, designHelper._AppTheme, textManager._SyntaxHighlighting, coreTextbox.EnableSyntaxHighlighting);
        }

        if (WordWrapEnabled)
            (NumberOfStartLine, NumberOfRenderedLines) = CalculateLinesToRender();

        lineNumberRenderer.CheckGenerateLineNumberText();

        if (!WordWrapEnabled)
        {
            scrollManager.EnsureHorizontalScrollBounds(canvasText, longestLineManager, false, zoomManager.ZoomNeedsRecalculateLongestLine);
            if (zoomManager.ZoomNeedsRecalculateLongestLine)
                zoomManager.ZoomNeedsRecalculateLongestLine = false;
        }

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
                    WordWrapEnabled ? TextRenderOffsetY - VerticalScrollPixels : SingleLineHeight / scrollManager.DefaultVerticalScrollSensitivity,
                    designHelper._Design.SearchHighlightColor
                    );

            float textOffsetY = WordWrapEnabled ? TextRenderOffsetY - VerticalScrollPixels : SingleLineHeight;
            ccls.DrawTextLayout(DrawnTextLayout, (float)-scrollManager.HorizontalScroll, textOffsetY, designHelper.TextColorBrush);

            invisibleCharactersRenderer.DrawTabsAndSpaces(args, ccls, RenderedText, DrawnTextLayout, textOffsetY);
        }
        args.DrawingSession.DrawImage(canvasCommandList);

        //Only update if needed, to reduce updates when scrolling
        //if (lineNumberRenderer.CanUpdateCanvas())
        //{
            canvasUpdateManager.UpdateLineNumbers();
        //}
    }
}
