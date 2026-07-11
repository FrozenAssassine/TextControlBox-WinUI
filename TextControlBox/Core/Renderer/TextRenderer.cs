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

    // ── Word wrap (visual-row model) ────────────────────────────────────────────────────
    // In wrap mode a single document line can occupy several visual rows, so the vertical scrollbar, caret
    // and selection all work in "visual-row" space. WrapRowMetrics holds the document-line ↔ visual-row
    // mapping (a prefix sum of per-line row counts) and this renderer feeds it the per-line row measurements.
    /// <summary>First visual row currently scrolled into view (wrap mode).</summary>
    public int StartVisualRow { get; private set; }
    /// <summary>How many visual rows of the first visible document line are scrolled above the viewport top.</summary>
    public int WrappedStartRowOffset { get; private set; }
    /// <summary>True when word wrap is enabled (the layout wraps long lines into multiple visual rows).</summary>
    public bool IsWordWrapEnabled => textLayoutManager?.WordWrap == true;
    private readonly WrapRowMetrics wrapMetrics = new();
    private readonly HashSet<int> dirtyWrapLines = new();
    private float cachedWrapWidth;
    private bool wrapMetricsDirty = true;
    // Above this many dirty lines a full rebuild is cheaper than many incremental patches.
    private const int IncrementalWrapRemeasureLimit = 64;
    // Above this line length, estimate the wrapped row count instead of laying the whole line out (a
    // multi-megabyte line would otherwise create a giant measurement layout).
    private const int LongLineRowEstimateThreshold = 100_000;

    // ── Wrapped-line virtualization (a single line so long it wraps to more rows than fit) ──────────
    // At/above LongLineRowEstimateThreshold a single wrapped line can span tens of thousands of rows, so
    // laying the whole line out is prohibitive. When such a line is the only visible one we render just the
    // visible slice of its rows (estimated char-per-row grid) and map caret/click within it.
    /// <summary>True when the current frame renders a single very-long wrapped line as a row slice.</summary>
    public bool IsVirtualizedWrappedLine { get; private set; }
    /// <summary>Rows of the virtualized line materialized this frame.</summary>
    public int VirtualizedWrappedRowsToRender { get; private set; }
    /// <summary>Document char offset where the rendered row slice starts.</summary>
    public int VirtualizedLineSliceStart { get; private set; }
    /// <summary>Estimated characters per wrapped row for the virtualized line.</summary>
    public int VirtualizedLineCharsPerRow { get; private set; }
    private const int VirtualizedWrappedLinePaddingRows = 2;

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

        if (IsWordWrapEnabled)
            EnsureWrapMetrics(canvasText);

        // A very-long wrapped line backs its caret/click with the same virtualized row-slice layout the main
        // text uses (reusing the already-built RenderedText when this line is the render start).
        if (ShouldVirtualizeWrappedLine(cursorManager.LineNumber))
        {
            LineSliceResult virtualizedText = IsVirtualizedWrappedLine && cursorManager.LineNumber == NumberOfStartLine
                ? new LineSliceResult(RenderedText, ReadOnlySpan<string>.Empty)
                : BuildVirtualizedWrappedLineRenderData(canvasText, cursorManager.LineNumber);
            CurrentLineTextLayout = textLayoutManager.CreateTextLayout(
                canvasText,
                TextFormat,
                virtualizedText.Text,
                GetWrapWidth(canvasText),
                (float)Math.Max(canvasText.Size.Height, (VirtualizedWrappedRowsToRender + 1) * Math.Max(1, SingleLineHeight)));
            return;
        }

        string lineText = textManager.GetLineText(cursorManager.LineNumber) + "|";

        // Slice the current line to the SAME horizontal window as the main text (non-wrap only) so the caret
        // and click hit-testing line up with the sliced layout: the caret's rendered index is
        // (documentChar - HorizontalSliceStart) and its x adds HorizontalSlicePixelOffset. A window computed
        // independently here would misplace the caret.
        if (IsHorizontallyVirtualized && !IsWordWrapEnabled && HorizontalSliceLength > 0)
        {
            int sliceStart = Math.Min(HorizontalSliceStart, lineText.Length);
            int len = Math.Min(HorizontalSliceLength, lineText.Length - sliceStart);
            lineText = len > 0 ? lineText.Substring(sliceStart, len) : string.Empty;
        }

        // In wrap mode the current line is laid out at the wrap width across as many rows as it needs, so the
        // caret and click hit-testing resolve a wrapped row via the layout's own multi-row geometry.
        Size layoutSize = IsWordWrapEnabled
            ? new Size(GetWrapWidth(canvasText), Math.Max(canvasText.Size.Height, (GetWrappedRowCount(cursorManager.LineNumber) + 1) * Math.Max(1, SingleLineHeight)))
            : canvasText.Size;

        CurrentLineTextLayout = textLayoutManager.CreateTextLayout(
            canvasText,
            TextFormat,
            lineText,
            layoutSize);
    }

    // ── Horizontal virtualization helpers ──────────────────────────────────────────────

    /// <summary>Rendered index (inside the current line's sliced layout) for a document column, clamped to
    /// this line's sliced length. When not sliced the rendered index equals the document column, so the
    /// caret/click paths can call this unconditionally.</summary>
    public int GetRenderedCharacterIndexForDocumentCharacter(int lineIndex, int characterPosition)
    {
        // Virtualized wrapped line: the layout is a row slice on an estimated char-per-row grid, so map the
        // document column into that grid (row * (charsPerRow + newline) + column). -1 = outside the slice.
        if (IsVirtualizedWrappedLine && lineIndex == NumberOfStartLine && VirtualizedLineCharsPerRow > 0)
        {
            int relative = characterPosition - VirtualizedLineSliceStart;
            if (relative < 0)
                return -1;
            int maxDocumentChars = VirtualizedWrappedRowsToRender * VirtualizedLineCharsPerRow;
            if (relative > maxDocumentChars)
                return -1;
            int row = relative / VirtualizedLineCharsPerRow;
            return relative + row * textManager.NewLineCharacter.Length;
        }
        if (IsHorizontallyVirtualized)
            return HorizontalSliceMath.RenderedIndexForColumn(characterPosition, HorizontalSliceStart, SlicedLineLength(lineIndex));
        return characterPosition;
    }

    /// <summary>Document column for a rendered index inside the current line's sliced layout, clamped to the
    /// document line length. When not sliced the document column equals the rendered index.</summary>
    public int GetDocumentCharacterIndexFromRenderedIndex(int lineIndex, int renderedIndex)
    {
        if (IsVirtualizedWrappedLine && lineIndex == NumberOfStartLine && VirtualizedLineCharsPerRow > 0)
        {
            int rowStride = VirtualizedLineCharsPerRow + textManager.NewLineCharacter.Length;
            int row = Math.Max(0, renderedIndex / rowStride);
            int column = Math.Clamp(renderedIndex % rowStride, 0, VirtualizedLineCharsPerRow);
            int characterPosition = VirtualizedLineSliceStart + row * VirtualizedLineCharsPerRow + column;
            return Math.Clamp(characterPosition, 0, textManager.GetLineLength(lineIndex));
        }
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

    // ── Word-wrap visual-row metrics ─────────────────────────────────────────────────────

    private float GetWrapWidth(CanvasControl canvasText) => Math.Max(1, (float)canvasText.ActualWidth);

    /// <summary>Forces a full wrap-metrics rebuild on the next <see cref="EnsureWrapMetrics"/>.</summary>
    public void InvalidateWrapMetrics()
    {
        wrapMetricsDirty = true;
        dirtyWrapLines.Clear();
    }

    /// <summary>Ensures the document-line ↔ visual-row mapping is current for the wrap width. Rebuilds on a
    /// width change or when the line count changed; otherwise re-measures the current line so live typing
    /// reflows immediately (there is no per-line change event to subscribe to).</summary>
    public void EnsureWrapMetrics(CanvasControl canvasText)
    {
        if (!IsWordWrapEnabled || canvasText == null || TextFormat == null)
            return;

        float wrapWidth = GetWrapWidth(canvasText);
        if (Math.Abs(cachedWrapWidth - wrapWidth) >= 0.5f)
        {
            cachedWrapWidth = wrapWidth;
            wrapMetricsDirty = true;
            NeedsUpdateTextLayout = true;
            lineNumberRenderer.NeedsUpdateLineNumbers();
        }

        if (wrapMetricsDirty || !wrapMetrics.IsValidFor(textManager.LinesCount))
        {
            wrapMetrics.Rebuild(textManager.LinesCount, i => MeasureWrappedRowCount(canvasText, i));
            wrapMetricsDirty = false;
            dirtyWrapLines.Clear();
            return;
        }

        // Keep the current line fresh so typing reflows without a full rebuild. A line add/remove changes the
        // line count, which forces a rebuild via IsValidFor above.
        if (cursorManager.LineNumber >= 0 && cursorManager.LineNumber < textManager.LinesCount)
            dirtyWrapLines.Add(cursorManager.LineNumber);

        if (dirtyWrapLines.Count > 0)
        {
            bool patched = wrapMetrics.ApplyIncremental(
                textManager.LinesCount, dirtyWrapLines, dirtyWrapLines.Count, IncrementalWrapRemeasureLimit,
                i => MeasureWrappedRowCount(canvasText, i));
            if (!patched)
            {
                wrapMetrics.Rebuild(textManager.LinesCount, i => MeasureWrappedRowCount(canvasText, i));
                wrapMetricsDirty = false;
            }
            dirtyWrapLines.Clear();
        }
    }

    private int MeasureWrappedRowCount(CanvasControl canvasText, int lineIndex)
    {
        if (lineIndex < 0 || lineIndex >= textManager.LinesCount)
            return 1;

        string lineText = textManager.GetLineText(lineIndex);
        if (lineText.Length == 0)
            return 1;

        float singleLineHeight = Math.Max(1, SingleLineHeight);
        if (lineText.Length >= LongLineRowEstimateThreshold)
            return EstimateWrappedRowCount(canvasText, lineText.Length);

        float layoutHeight = Math.Max(singleLineHeight, (lineText.Length + 1) * singleLineHeight);
        using CanvasTextLayout lineLayout = textLayoutManager.CreateTextLayout(canvasText, TextFormat, lineText, cachedWrapWidth, layoutHeight);
        // Avoid CanvasTextLayout.LineMetrics (CanvasLineMetrics is non-blittable and throws
        // NotSupportedException on newer .NET). Use layout height / line height instead.
        int rowCount = (int)Math.Ceiling(lineLayout.LayoutBounds.Height / singleLineHeight);
        return Math.Max(1, rowCount);
    }

    // ── Wrapped-line virtualization helpers ──────────────────────────────────────────────

    private void ResetVirtualizedWrappedLineState()
    {
        IsVirtualizedWrappedLine = false;
        VirtualizedWrappedRowsToRender = 0;
        VirtualizedLineSliceStart = 0;
        VirtualizedLineCharsPerRow = 0;
    }

    private bool ShouldVirtualizeWrappedLine(int lineIndex)
        => IsWordWrapEnabled
           && lineIndex >= 0
           && lineIndex < textManager.LinesCount
           && textManager.GetLineLength(lineIndex) >= LongLineRowEstimateThreshold;

    private int EstimateWrappedCharsPerRow(CanvasControl canvasText)
    {
        float wrapWidth = GetWrapWidth(canvasText);
        float averageCharWidth = Math.Max(1, zoomManager.ZoomedFontSize * 0.58f);
        return Math.Max(1, (int)Math.Floor(wrapWidth / averageCharWidth));
    }

    private int EstimateWrappedRowCount(CanvasControl canvasText, int textLength)
        => Math.Max(1, (int)Math.Ceiling(textLength / (double)EstimateWrappedCharsPerRow(canvasText)));

    /// <summary>Builds the visible ROW slice of a very-long wrapped line (never materializing the whole
    /// multi-megabyte line): starting at <see cref="WrappedStartRowOffset"/>, laid out on an estimated
    /// char-per-row grid and joined by the newline. Records the slice metrics for caret/click mapping.</summary>
    private LineSliceResult BuildVirtualizedWrappedLineRenderData(CanvasControl canvasText, int lineIndex)
    {
        string lineText = textManager.GetLineText(lineIndex);
        int charsPerRow = EstimateWrappedCharsPerRow(canvasText);
        int totalRows = Math.Max(1, (int)Math.Ceiling(lineText.Length / (double)charsPerRow));
        int startRow = Math.Clamp(WrappedStartRowOffset, 0, totalRows - 1);
        int rowsToRender = Math.Min(totalRows - startRow, GetVisibleVisualRowCount(canvasText, VirtualizedWrappedLinePaddingRows));
        int sliceStart = Math.Min(lineText.Length, startRow * charsPerRow);
        int newlineLength = textManager.NewLineCharacter.Length;

        var builder = new StringBuilder(Math.Min(lineText.Length - sliceStart, rowsToRender * (charsPerRow + newlineLength)));
        int offset = sliceStart;
        int rowsRendered = 0;
        while (rowsRendered < rowsToRender && offset < lineText.Length)
        {
            if (rowsRendered > 0)
                builder.Append(textManager.NewLineCharacter);

            int length = Math.Min(charsPerRow, lineText.Length - offset);
            builder.Append(lineText, offset, length);
            offset += length;
            rowsRendered++;
        }

        if (rowsRendered == 0)
            rowsRendered = 1;

        IsVirtualizedWrappedLine = true;
        VirtualizedWrappedRowsToRender = rowsRendered;
        VirtualizedLineSliceStart = sliceStart;
        VirtualizedLineCharsPerRow = charsPerRow;
        return new LineSliceResult(builder.ToString(), ReadOnlySpan<string>.Empty);
    }

    private CanvasTextLayout CreateWrappedLineTextLayout(CanvasControl canvasText, int lineIndex, bool includeCaretMarker = false)
    {
        string lineText = textManager.GetLineText(lineIndex);
        if (includeCaretMarker)
            lineText += "|";

        float singleLineHeight = Math.Max(1, SingleLineHeight);
        int rowCount = GetWrappedRowCount(lineIndex);
        float layoutHeight = (float)Math.Max(canvasText.Size.Height, (rowCount + 1) * singleLineHeight);
        float wrapWidth = cachedWrapWidth > 1 ? cachedWrapWidth : GetWrapWidth(canvasText);

        return textLayoutManager.CreateTextLayout(canvasText, TextFormat, lineText, wrapWidth, layoutHeight);
    }

    /// <summary>Visual rows document line <paramref name="lineIndex"/> occupies (1 when wrap is off).</summary>
    public int GetWrappedRowCount(int lineIndex)
    {
        if (!IsWordWrapEnabled || lineIndex < 0 || lineIndex >= textManager.LinesCount)
            return 1;
        return wrapMetrics.GetRowCount(lineIndex);
    }

    /// <summary>First visual row of document line <paramref name="lineIndex"/> (the line index itself when off).</summary>
    public int GetLineVisualStartRow(int lineIndex)
    {
        if (!IsWordWrapEnabled)
            return Math.Clamp(lineIndex, 0, Math.Max(0, textManager.LinesCount - 1));
        return wrapMetrics.GetLineStartRow(lineIndex, textManager.LinesCount);
    }

    /// <summary>Document line that owns <paramref name="visualRow"/>.</summary>
    public int GetDocumentLineFromVisualRow(int visualRow)
    {
        if (!IsWordWrapEnabled)
            return Math.Clamp(visualRow, 0, Math.Max(0, textManager.LinesCount - 1));
        return wrapMetrics.GetDocumentLineFromVisualRow(visualRow, textManager.LinesCount);
    }

    public int GetStartVisualRowFromScroll()
    {
        if (!IsWordWrapEnabled)
            return NumberOfStartLine;
        int visualRow = (int)Math.Floor((scrollManager.VerticalScroll * scrollManager.DefaultVerticalScrollSensitivity) / Math.Max(1, SingleLineHeight));
        return Math.Clamp(visualRow, 0, Math.Max(0, wrapMetrics.TotalVisualRows - 1));
    }

    public int GetVisibleVisualRowCount(CanvasControl canvasText, int extraRows = 0)
        => Math.Max(1, (int)Math.Ceiling(canvasText.ActualHeight / Math.Max(1, SingleLineHeight)) + extraRows);

    public int GetRenderedVisualRowCount(int startLine, int lineCount)
    {
        if (!IsWordWrapEnabled)
            return lineCount;
        return wrapMetrics.GetRenderedVisualRowCount(startLine, lineCount, textManager.LinesCount);
    }

    public int GetVisualRowFromPointY(double y)
        => WrapGeometry.CalculateVisualRowFromPointY(y, StartVisualRow, SingleLineHeight, scrollManager.DefaultVerticalScrollSensitivity);

    public float GetWrappedLineHitTestYFromPointY(int lineIndex, double y)
    {
        // A virtualized wrapped line renders pinned to the viewport top, so hit-test relative to y=0 and its
        // materialized row count rather than the document line's full extent.
        if (IsVirtualizedWrappedLine && lineIndex == NumberOfStartLine)
            return WrapGeometry.CalculateWrappedLineHitTestYFromPointY(y, 0, SingleLineHeight, scrollManager.DefaultVerticalScrollSensitivity, Math.Max(1, VirtualizedWrappedRowsToRender));
        return WrapGeometry.CalculateWrappedLineHitTestYFromPointY(y, GetLineTopY(lineIndex), SingleLineHeight, scrollManager.DefaultVerticalScrollSensitivity, GetWrappedRowCount(lineIndex));
    }

    /// <summary>Y (relative to the viewport top) of document line <paramref name="lineIndex"/>'s first row.</summary>
    public float GetLineTopY(int lineIndex)
    {
        if (!IsWordWrapEnabled)
            return (lineIndex - NumberOfStartLine) * SingleLineHeight;
        return (GetLineVisualStartRow(lineIndex) - StartVisualRow) * SingleLineHeight;
    }

    /// <summary>Moves <paramref name="cursorPosition"/> up/down by <paramref name="rowDelta"/> VISUAL rows,
    /// preserving the target column via hit-testing the wrapped layout. Returns false when wrap is off (the
    /// caller should fall back to plain line movement).</summary>
    public bool MoveCursorByVisualRows(CanvasControl canvasText, CursorPosition cursorPosition, int rowDelta)
    {
        if (!IsWordWrapEnabled || cursorPosition == null || textManager.LinesCount == 0)
            return false;

        EnsureWrapMetrics(canvasText);
        int lineIndex = Math.Clamp(cursorPosition.LineNumber, 0, textManager.LinesCount - 1);
        int characterPosition = Math.Clamp(cursorPosition.CharacterPosition, 0, textManager.GetLineLength(lineIndex));

        // A virtualized (very-long) wrapped line can't be laid out to hit-test, so navigate on the estimated
        // char-per-row grid instead of building a giant layout.
        if (ShouldVirtualizeWrappedLine(lineIndex))
        {
            int charsPerRow = EstimateWrappedCharsPerRow(canvasText);
            int currentColumn = characterPosition % charsPerRow;
            int virtualCurrentVisualRow = GetLineVisualStartRow(lineIndex) + characterPosition / charsPerRow;
            int virtualTargetVisualRow = Math.Clamp(virtualCurrentVisualRow + rowDelta, 0, Math.Max(0, wrapMetrics.TotalVisualRows - 1));
            int virtualTargetLine = GetDocumentLineFromVisualRow(virtualTargetVisualRow);
            int virtualTargetRowOffset = virtualTargetVisualRow - GetLineVisualStartRow(virtualTargetLine);
            int targetLength = textManager.GetLineLength(virtualTargetLine);
            int targetCharsPerRow = ShouldVirtualizeWrappedLine(virtualTargetLine) ? EstimateWrappedCharsPerRow(canvasText) : charsPerRow;

            cursorPosition.LineNumber = virtualTargetLine;
            cursorPosition.CharacterPosition = Math.Clamp(virtualTargetRowOffset * targetCharsPerRow + currentColumn, 0, targetLength);
            return true;
        }

        using CanvasTextLayout currentLayout = CreateWrappedLineTextLayout(canvasText, lineIndex, true);
        var currentCaret = currentLayout.GetCaretPosition(characterPosition, false);
        int currentVisualRow = GetLineVisualStartRow(lineIndex) + (int)Math.Floor(Math.Max(0, currentCaret.Y) / Math.Max(1, SingleLineHeight));
        int targetVisualRow = Math.Clamp(currentVisualRow + rowDelta, 0, Math.Max(0, wrapMetrics.TotalVisualRows - 1));
        int targetLine = GetDocumentLineFromVisualRow(targetVisualRow);
        int targetRowOffset = targetVisualRow - GetLineVisualStartRow(targetLine);

        using CanvasTextLayout targetLayout = CreateWrappedLineTextLayout(canvasText, targetLine, true);
        targetLayout.HitTest(currentCaret.X, targetRowOffset * Math.Max(1, SingleLineHeight), out var targetRegion);

        cursorPosition.LineNumber = targetLine;
        cursorPosition.CharacterPosition = Math.Clamp(targetRegion.CharacterIndex, 0, textManager.GetLineLength(targetLine));
        return true;
    }

    public void UpdateRenderedLineRange(CanvasControl canvasText)
    {
        (NumberOfStartLine, NumberOfRenderedLines) = CalculateLinesToRender();
    }

    public (int startLine, int linesToRender) CalculateLinesToRender()
    {
        var singleLineHeight = SingleLineHeight;

        if (IsWordWrapEnabled)
            return CalculateWrappedLinesToRender(coreTextbox.canvasText, singleLineHeight);

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

    private (int startLine, int linesToRender) CalculateWrappedLinesToRender(CanvasControl canvasText, float singleLineHeight)
    {
        EnsureWrapMetrics(canvasText);
        int totalVisualRows = wrapMetrics.TotalVisualRows;

        scrollManager.verticalScrollBar.Maximum = Math.Max(0, (totalVisualRows * singleLineHeight - scrollGrid.ActualHeight) / scrollManager.DefaultVerticalScrollSensitivity);
        scrollManager.verticalScrollBar.ViewportSize = coreTextbox.canvasText.ActualHeight;

        StartVisualRow = GetStartVisualRowFromScroll();

        int startLine = GetDocumentLineFromVisualRow(StartVisualRow);
        int startLineVisualRow = GetLineVisualStartRow(startLine);
        WrappedStartRowOffset = Math.Max(0, StartVisualRow - startLineVisualRow);

        int visibleRows = GetVisibleVisualRowCount(canvasText, 2);
        int rowsToCover = visibleRows + WrappedStartRowOffset;
        int linesToRender = 0;
        for (int i = startLine; i < textManager.LinesCount && rowsToCover > 0; i++)
        {
            rowsToCover -= GetWrappedRowCount(i);
            linesToRender++;
        }

        return (startLine, Math.Max(0, linesToRender));
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
        ResetVirtualizedWrappedLineState();
        if (IsWordWrapEnabled && NumberOfRenderedLines == 1 && ShouldVirtualizeWrappedLine(NumberOfStartLine))
        {
            // A single wrapped line so long it fills the viewport by itself: render only its visible ROW
            // slice (a few KB) instead of laying out the whole multi-megabyte line.
            renderTextData = BuildVirtualizedWrappedLineRenderData(canvasText, NumberOfStartLine);
            RenderedText = renderTextData.Text;
            IsHorizontallyVirtualized = false;
            HorizontalSliceStart = 0;
            HorizontalSliceLength = 0;
            _hSliceVisibleStart = 0;
            _hSliceVisibleEnd = 0;
        }
        else if (!IsWordWrapEnabled && ShouldHorizontallySlice(canvasText, out int hSliceStart, out int hSliceLen))
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

        // Draw offsets. In wrap mode there is no horizontal scroll and the layout is nudged up by the rows of
        // the first visible line that are scrolled above the viewport top; a virtualized single line already
        // renders from its own row slice so it sits at the top. Both reduce to the non-wrap values when wrap
        // is off, so the normal render path is unchanged.
        float drawTextOffsetX = IsWordWrapEnabled ? 0 : HorizontalOffset;
        float drawTextOffsetY = IsVirtualizedWrappedLine
            ? SingleLineHeight
            : (IsWordWrapEnabled ? SingleLineHeight - (WrappedStartRowOffset * SingleLineHeight) : SingleLineHeight);
        float searchHighlightOffsetY = IsWordWrapEnabled
            ? drawTextOffsetY - SingleLineHeight + (SingleLineHeight / scrollManager.DefaultVerticalScrollSensitivity)
            : SingleLineHeight / scrollManager.DefaultVerticalScrollSensitivity;

        int renderedVisualRows = IsVirtualizedWrappedLine
            ? VirtualizedWrappedRowsToRender
            : GetRenderedVisualRowCount(NumberOfStartLine, NumberOfRenderedLines);
        Size layoutSize = IsWordWrapEnabled
            ? new Size
            {
                Height = IsVirtualizedWrappedLine
                    ? Math.Max(canvasText.Size.Height + (VirtualizedWrappedLinePaddingRows * SingleLineHeight), (renderedVisualRows + 2) * SingleLineHeight)
                    : Math.Max(canvasText.Size.Height + (WrappedStartRowOffset + 2) * SingleLineHeight, (renderedVisualRows + 2) * SingleLineHeight),
                Width = GetWrapWidth(canvasText)
            }
            : new Size { Height = canvasText.Size.Height, Width = coreTextbox.ActualWidth };

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

            DrawnTextLayout = textLayoutManager.CreateTextResource(canvasText, DrawnTextLayout, TextFormat, RenderedText, layoutSize);
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
                    drawTextOffsetX,
                    searchHighlightOffsetY,
                    designHelper._Design.SearchHighlightColor
                    );

            ccls.DrawTextLayout(DrawnTextLayout, drawTextOffsetX, drawTextOffsetY, designHelper.TextColorBrush);

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
