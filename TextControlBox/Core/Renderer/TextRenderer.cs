using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Text;
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
    public float HorizontalOffset => (float)-scrollManager.HorizontalScroll + HorizontalSlicePixelOffset;
    public int NumberOfStartLine = 0;
    public int NumberOfRenderedLines = 0;
    public string RenderedText = "";
    public string OldRenderedText = null;

    // ── Horizontal virtualization (non-wrap, very long lines) ──────────────────────────
    // Files with pathologically long lines (minified JS/CSS/JSON, JSONL logs) turn every frame into a
    // multi-megabyte string join + text layout, which stutters horizontal scrolling. When any visible line
    // exceeds the threshold we lay out ONLY a sliced window of each visible line instead of the whole line,
    // and record enough offsets to map document positions in and out of that sliced layout for the caret,
    // click hit-testing and selection.
    private const int HorizontalVirtualizationThreshold = 50_000;
    /// <summary>First document column kept by the current horizontal slice window.</summary>
    public int HorizontalSliceStart { get; private set; }
    /// <summary>Width (in chars) of the horizontal slice window; 0 when not sliced.</summary>
    public int HorizontalSliceLength { get; private set; }
    /// <summary>True while horizontal virtualization is active for the current frame.</summary>
    public bool IsHorizontallyVirtualized { get; private set; }
    /// <summary>Rendered-layout offset of each visible line's text start inside <see cref="DrawnTextLayout"/>
    /// when sliced (indexed by ordinal from <see cref="NumberOfStartLine"/>). Lets selection map a document
    /// (line, char) to a multi-line rendered index in O(1). Rebuilt on every re-slice.</summary>
    private readonly List<int> _renderedLineSlicePrefix = new();
    /// <summary>Measured width of one character in the current font (monospace assumption); falls back to an
    /// estimate until measured.</summary>
    private float _cachedCharWidth;
    private float CachedCharWidth => _cachedCharWidth > 0 ? _cachedCharWidth : Math.Max(1, zoomManager.ZoomedFontSize * 0.6f);
    /// <summary>Visible char range covered by the current window's buffered safe zone [start, end); while the
    /// viewport stays inside it the window is reused so small horizontal scrolls don't re-slice.</summary>
    private int _hSliceVisibleStart;
    private int _hSliceVisibleEnd;
    /// <summary>Pixel offset of the slice start. A sliced layout's char 0 is document column
    /// <see cref="HorizontalSliceStart"/>, so adding this to the horizontal draw offset re-aligns it to
    /// document coordinates. 0 when not sliced, which keeps <see cref="HorizontalOffset"/> unchanged.</summary>
    public float HorizontalSlicePixelOffset => IsHorizontallyVirtualized ? HorizontalSliceStart * CachedCharWidth : 0;

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
        return line < NumberOfStartLine || line >= NumberOfStartLine + NumberOfRenderedLines;
    }

    public void UpdateCurrentLineTextLayout(CanvasControl canvasText)
    {
        CurrentLineTextLayout?.Dispose();
        if (cursorManager.LineNumber >= textManager.LinesCount)
        {
            CurrentLineTextLayout = null;
            return;
        }

        string lineText = textManager.GetLineText(cursorManager.LineNumber) + "|";

        // Slice the current line to the SAME horizontal window as the main text (see Draw) so the caret and
        // click hit-testing line up with the sliced layout: the caret's rendered index is
        // (documentChar - HorizontalSliceStart) and its x adds HorizontalSlicePixelOffset. A window computed
        // independently here would misplace the caret.
        if (IsHorizontallyVirtualized && HorizontalSliceLength > 0)
        {
            int sliceStart = Math.Min(HorizontalSliceStart, lineText.Length);
            int len = Math.Min(HorizontalSliceLength, lineText.Length - sliceStart);
            lineText = len > 0 ? lineText.Substring(sliceStart, len) : string.Empty;
        }

        CurrentLineTextLayout = textLayoutManager.CreateTextLayout(
            canvasText,
            TextFormat,
            lineText,
            canvasText.Size);
    }

    // ── Horizontal virtualization helpers ──────────────────────────────────────────────

    /// <summary>Rendered index (inside the current line's sliced layout) for a document column, clamped to
    /// this line's sliced length. When not sliced the rendered index equals the document column, so the
    /// caret/click paths can call this unconditionally.</summary>
    public int GetRenderedCharacterIndexForDocumentCharacter(int lineIndex, int characterPosition)
    {
        if (IsHorizontallyVirtualized)
            return HorizontalSliceMath.RenderedIndexForColumn(characterPosition, HorizontalSliceStart, SlicedLineLength(lineIndex));
        return characterPosition;
    }

    /// <summary>Document column for a rendered index inside the current line's sliced layout, clamped to the
    /// document line length. When not sliced the document column equals the rendered index.</summary>
    public int GetDocumentCharacterIndexFromRenderedIndex(int lineIndex, int renderedIndex)
    {
        if (IsHorizontallyVirtualized)
            return HorizontalSliceMath.ColumnForRenderedIndex(renderedIndex, HorizontalSliceStart, textManager.GetLineLength(lineIndex));
        return renderedIndex;
    }

    /// <summary>Chars line <paramref name="lineIndex"/> contributes to the current slice: its length past
    /// <see cref="HorizontalSliceStart"/>, capped at the window width. 0 when the line ends before the
    /// window (nothing of it is visible at this scroll position).</summary>
    private int SlicedLineLength(int lineIndex)
    {
        if (HorizontalSliceLength <= 0 || lineIndex < 0 || lineIndex >= textManager.LinesCount)
            return 0;
        return HorizontalSliceMath.SlicedLineLength(textManager.GetLineLength(lineIndex), HorizontalSliceStart, HorizontalSliceLength);
    }

    /// <summary>Maps a document position (line, char) to its character index inside the multi-line sliced
    /// <see cref="DrawnTextLayout"/> via the per-line prefix offsets built by
    /// <see cref="BuildHorizontallySlicedText"/>. Returns -1 when the line is outside the rendered range.
    /// Used by selection rendering, which can span multiple rendered lines.</summary>
    public int GetRenderedLayoutIndexForDocument(int lineIndex, int characterPosition)
    {
        int ordinal = lineIndex - NumberOfStartLine;
        if (ordinal < 0 || ordinal >= _renderedLineSlicePrefix.Count)
            return -1;
        int inLine = HorizontalSliceMath.RenderedIndexForColumn(characterPosition, HorizontalSliceStart, SlicedLineLength(lineIndex));
        return _renderedLineSlicePrefix[ordinal] + inLine;
    }

    /// <summary>Decides whether to horizontally virtualize this frame and, if so, the slice window
    /// [<paramref name="sliceStart"/>, sliceStart + <paramref name="sliceLen"/>). Only triggers when at least
    /// one visible line exceeds <see cref="HorizontalVirtualizationThreshold"/>, so ordinary files keep the
    /// normal (untouched) render path. Reuses the previous window while the viewport stays inside its
    /// buffered safe zone so small horizontal scrolls don't re-slice.</summary>
    private bool ShouldHorizontallySlice(CanvasControl canvasText, out int sliceStart, out int sliceLen)
    {
        sliceStart = 0;
        sliceLen = 0;
        if (NumberOfRenderedLines <= 0)
            return false;

        int end = Math.Min(NumberOfStartLine + NumberOfRenderedLines, textManager.LinesCount);
        int maxLen = 0;
        for (int i = NumberOfStartLine; i < end; i++)
        {
            int len = textManager.GetLineLength(i);
            if (len > maxLen)
                maxLen = len;
        }
        if (maxLen <= HorizontalVirtualizationThreshold)
            return false;

        float charWidth = CachedCharWidth;
        float viewportWidth = (float)canvasText.Size.Width;
        float hScroll = (float)scrollManager.HorizontalScroll;
        (int visibleStartChar, int visibleEndChar) = HorizontalSliceMath.VisibleCharRange(hScroll, viewportWidth, charWidth);

        // Reuse the current window while the viewport is still inside its buffered safe zone.
        if (IsHorizontallyVirtualized && HorizontalSliceLength > 0
            && HorizontalSliceMath.CanReuseWindow(visibleStartChar, visibleEndChar, _hSliceVisibleStart, _hSliceVisibleEnd))
        {
            sliceStart = HorizontalSliceStart;
            sliceLen = HorizontalSliceLength;
            return true;
        }

        (sliceStart, sliceLen, _hSliceVisibleStart, _hSliceVisibleEnd) = HorizontalSliceMath.ComputeWindow(visibleStartChar, visibleEndChar);
        return true;
    }

    /// <summary>Builds the visible text with every line sliced to the same horizontal window, joined with the
    /// newline character, and (re)builds <see cref="_renderedLineSlicePrefix"/> so document positions can be
    /// mapped back into the resulting layout. Never materializes a full (multi-megabyte) line.</summary>
    private string BuildHorizontallySlicedText(int sliceStart, int sliceLen)
    {
        _renderedLineSlicePrefix.Clear();
        string newline = textManager.NewLineCharacter;
        int newlineLen = newline.Length;
        int end = Math.Min(NumberOfStartLine + NumberOfRenderedLines, textManager.LinesCount);

        int capacity = Math.Min(Math.Max(1, NumberOfRenderedLines) * (sliceLen + newlineLen), 1 << 21);
        var builder = new StringBuilder(capacity);
        int offset = 0;
        bool first = true;
        for (int i = NumberOfStartLine; i < end; i++)
        {
            if (!first)
            {
                builder.Append(newline);
                offset += newlineLen;
            }
            first = false;

            _renderedLineSlicePrefix.Add(offset); // rendered-layout offset of this line's text start

            string lineText = textManager.GetLineText(i);
            if (lineText.Length > sliceStart)
            {
                int len = Math.Min(sliceLen, lineText.Length - sliceStart);
                builder.Append(lineText, sliceStart, len);
                offset += len;
            }
        }
        return builder.ToString();
    }
    public (int startLine, int linesToRender) CalculateLinesToRender()
    {
        var singleLineHeight = SingleLineHeight;

        //Measure text position and apply the value to the scrollbar
        scrollManager.verticalScrollBar.Maximum = ((textManager.LinesCount + 1) * singleLineHeight - scrollGrid.ActualHeight) / scrollManager.DefaultVerticalScrollSensitivity;
        scrollManager.verticalScrollBar.ViewportSize = coreTextbox.canvasText.ActualHeight;

        //Calculate number of lines that need to be rendered
        int linesToRenderCount = (int)(coreTextbox.canvasText.ActualHeight / singleLineHeight);
        linesToRenderCount = Math.Min(linesToRenderCount, textManager.LinesCount);

        int startLine = (int)((scrollManager.VerticalScroll * scrollManager.DefaultVerticalScrollSensitivity) / singleLineHeight);
        startLine = Math.Min(startLine, textManager.LinesCount);

        int linesToRender = Math.Min(linesToRenderCount, textManager.LinesCount - startLine);

        return (startLine, linesToRender);
    }

    public void Draw(CanvasControl canvasText, CanvasDrawEventArgs args)
    {
        //Create resources and layouts:
        if (NeedsTextFormatUpdate || TextFormat == null || lineNumberRenderer.LineNumberTextFormat == null)
        {
            lineNumberRenderer.CreateLineNumberTextFormat();

            TextFormat?.Dispose();
            TextFormat = textLayoutManager.CreateCanvasTextFormat();

            invisibleCharactersRenderer.UpdateTextFormat(canvasText, TextFormat);

            designHelper.CreateColorResources(args.DrawingSession);

            // Measure the actual character width (monospace assumption) for the horizontal-virtualization
            // slice-to-pixel offset. Re-measured whenever the format is rebuilt (font/zoom change).
            using (var measureLayout = new CanvasTextLayout(args.DrawingSession, "M", TextFormat, 0, 0))
            {
                _cachedCharWidth = Math.Max(1, (float)measureLayout.DrawBounds.Width);
            }
        }

        (NumberOfStartLine, NumberOfRenderedLines) = CalculateLinesToRender();

        // Decide horizontal virtualization BEFORE materializing the visible text. Joining many very long
        // lines (megabytes) into one string every frame — then laying it out — is the actual scroll-stutter
        // cost on files like JSONL logs (hundreds of 50k+ char lines). When any visible line is very long we
        // build ONLY the sliced text (a few KB) instead of the full join, and record a per-line prefix so the
        // caret/selection can still map document positions into the sliced multi-line layout.
        LineSliceResult renderTextData;
        if (ShouldHorizontallySlice(canvasText, out int hSliceStart, out int hSliceLen))
        {
            RenderedText = BuildHorizontallySlicedText(hSliceStart, hSliceLen);
            HorizontalSliceStart = hSliceStart;
            HorizontalSliceLength = hSliceLen;
            IsHorizontallyVirtualized = true;
            // Syntax highlighting is skipped for a sliced layout (its char offsets would shift), so pass an
            // empty per-line span.
            renderTextData = new LineSliceResult(RenderedText, ReadOnlySpan<string>.Empty);
        }
        else
        {
            renderTextData = textManager.GetLinesForRendering(NumberOfStartLine, NumberOfRenderedLines);
            RenderedText = renderTextData.Text;
            IsHorizontallyVirtualized = false;
            HorizontalSliceStart = 0;
            HorizontalSliceLength = 0;
            _hSliceVisibleStart = 0;
            _hSliceVisibleEnd = 0;
        }

        //check rendering and calculation updates
        lineNumberRenderer.CheckGenerateLineNumberText();

        using CanvasCommandList canvasCommandList = new CanvasCommandList(args.DrawingSession);
        if ((OldRenderedText != null && OldRenderedText.Length != RenderedText.Length)
            || !RenderedText.Equals(OldRenderedText, StringComparison.Ordinal)
            || NeedsUpdateTextLayout
        )
        {
            NeedsUpdateTextLayout = false;
            OldRenderedText = RenderedText;

            DrawnTextLayout = textLayoutManager.CreateTextResource(canvasText, DrawnTextLayout, TextFormat, RenderedText, new Size { Height = canvasText.Size.Height, Width = coreTextbox.ActualWidth });
            SyntaxHighlightingRenderer.UpdateSyntaxHighlighting(renderTextData, textManager.NewLineCharacter, DrawnTextLayout, designHelper._AppTheme, textManager._SyntaxHighlighting, coreTextbox.EnableSyntaxHighlighting);
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
                    HorizontalOffset,
                    SingleLineHeight / scrollManager.DefaultVerticalScrollSensitivity,
                    designHelper._Design.SearchHighlightColor
                    );

            ccls.DrawTextLayout(DrawnTextLayout, HorizontalOffset, SingleLineHeight, designHelper.TextColorBrush);

            invisibleCharactersRenderer.DrawTabsAndSpaces(args, ccls, RenderedText, DrawnTextLayout, SingleLineHeight);
        }
        args.DrawingSession.DrawImage(canvasCommandList);

        //Only update if needed, to reduce updates when scrolling
        if (lineNumberRenderer.CanUpdateCanvas())
        {
            canvasUpdateManager.UpdateLineNumbers();
        }
    }
}
