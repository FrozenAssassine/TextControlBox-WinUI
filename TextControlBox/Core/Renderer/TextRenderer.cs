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
    private bool _isEnsuringTextFormat = false;
    public void EnsureTextFormat()
    {
        if (_isEnsuringTextFormat)
            return;

        if (NeedsTextFormatUpdate || TextFormat == null)
        {
            _isEnsuringTextFormat = true;
            try
            {
                TextFormat?.Dispose();
                TextFormat = textLayoutManager.CreateCanvasTextFormat();
                NeedsTextFormatUpdate = false;
                ClearLineLayoutCache();
                InvalidateCurrentLineLayout();
            }
            finally
            {
                _isEnsuringTextFormat = false;
            }
        }
    }

    public float SingleLineHeight
    {
        get
        {
            if (TextFormat == null && !_isEnsuringTextFormat)
                EnsureTextFormat();
            return TextFormat == null ? 0 : TextFormat.LineSpacing;
        }
    }
    public float TopInset => (float)Math.Round(2f * ((zoomManager == null ? 100f : zoomManager._ZoomFactor) / 100f));
    public float GetSelectionTopMargin()
    {
        if (!IsWordWrapEnabled || IsVirtualizedWrappedLine)
            return TopInset;
        return TopInset - (WrappedStartRowOffset * SingleLineHeight);
    }
    public float HorizontalOffset => (float)-scrollManager.HorizontalScroll + HorizontalSlicePixelOffset;
    public int NumberOfStartLine = 0;
    public int NumberOfRenderedLines = 0;
    private string _renderedText = "";
    private bool _renderedTextDirty = false;
    public string RenderedText
    {
        get => _renderedText;
        set
        {
            _renderedText = value;
            _renderedTextDirty = false;
        }
    }
    public string OldRenderedText = null;

    // ── Horizontal virtualization (non-wrap, very long lines) ──────────────────────────
    // Files with pathologically long lines (minified JS/CSS/JSON, JSONL logs) turn every frame into a
    // multi-megabyte string join + text layout, which stutters horizontal scrolling. When any visible line
    // exceeds the threshold we lay out ONLY a sliced window of each visible line instead of the whole line,
    // and record enough offsets to map document positions in and out of that sliced layout for the caret,
    // click hit-testing and selection.
    internal const int HorizontalVirtualizationThreshold = 50_000;
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
    internal float CachedCharWidth => _cachedCharWidth > 0 ? _cachedCharWidth : Math.Max(1, zoomManager.ZoomedFontSize * 0.6f);
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
    public int StartVisualRow { get; internal set; }
    /// <summary>How many visual rows of the first visible document line are scrolled above the viewport top.</summary>
    public int WrappedStartRowOffset { get; internal set; }
    /// <summary>True when word wrap is enabled (the layout wraps long lines into multiple visual rows).</summary>
    public bool IsWordWrapEnabled => textLayoutManager?.WordWrap == true;
    private readonly WrapRowMetrics wrapMetrics = new();
    private readonly HashSet<int> dirtyWrapLines = new();
    private readonly System.Collections.Generic.Dictionary<int, (CanvasTextLayout layout, string text, float wrapWidth)> _lineLayoutCache = new();
    private const int MaxCachedLineLayouts = 32;

    private int _currentLineLayoutLineIndex = -1;
    private string _currentLineLayoutText = null;
    private float _currentLineLayoutWrapWidth = -1f;
    private bool _currentLineLayoutIsWrapped = false;
    private int _currentLineLayoutSliceStart = -1;
    private int _currentLineLayoutSliceLength = -1;

    public void InvalidateCurrentLineLayout()
    {
        bool inCache = false;
        foreach (var entry in _lineLayoutCache.Values)
        {
            if (entry.layout == CurrentLineTextLayout)
            {
                inCache = true;
                break;
            }
        }
        if (!inCache)
        {
            CurrentLineTextLayout?.Dispose();
        }
        CurrentLineTextLayout = null;
        _currentLineLayoutLineIndex = -1;
        _currentLineLayoutText = null;
        _currentLineLayoutWrapWidth = -1f;
        _currentLineLayoutIsWrapped = false;
        _currentLineLayoutSliceStart = -1;
        _currentLineLayoutSliceLength = -1;
    }

    public void ClearLineLayoutCache()
    {
        foreach (var entry in _lineLayoutCache.Values)
        {
            if (entry.layout != null && entry.layout != CurrentLineTextLayout)
            {
                entry.layout.Dispose();
            }
        }
        _lineLayoutCache.Clear();
    }

    public void InvalidateLineLayout(int lineIndex)
    {
        if (_lineLayoutCache.Remove(lineIndex, out var entry))
        {
            if (entry.layout != null && entry.layout != CurrentLineTextLayout)
                entry.layout.Dispose();
        }
        if (_currentLineLayoutLineIndex == lineIndex)
        {
            InvalidateCurrentLineLayout();
        }
    }

    public void MarkLineWrapDirty(int lineIndex)
    {
        if (lineIndex >= 0 && lineIndex < textManager.LinesCount)
        {
            dirtyWrapLines.Add(lineIndex);
            _renderedTextDirty = true;
            InvalidateLineLayout(lineIndex);
        }
    }

    internal CanvasTextLayout GetOrCreateWrappedLineLayout(CanvasControl canvasText, int lineIndex)
    {
        EnsureTextFormat();
        if (lineIndex < 0 || lineIndex >= textManager.LinesCount)
            return null;

        float wrapWidth = cachedWrapWidth > 1 ? cachedWrapWidth : GetWrapWidth(canvasText);
        string lineText = textManager.GetLineText(lineIndex);

        if (CurrentLineTextLayout != null &&
            _currentLineLayoutLineIndex == lineIndex &&
            _currentLineLayoutIsWrapped &&
            _currentLineLayoutText == lineText &&
            Math.Abs(_currentLineLayoutWrapWidth - wrapWidth) < 0.5f)
        {
            return CurrentLineTextLayout;
        }

        if (_lineLayoutCache.TryGetValue(lineIndex, out var cached))
        {
            if (cached.layout != null &&
                cached.text == lineText &&
                Math.Abs(cached.wrapWidth - wrapWidth) < 0.5f)
            {
                return cached.layout;
            }

            if (cached.layout != null && cached.layout != CurrentLineTextLayout)
                cached.layout.Dispose();
            _lineLayoutCache.Remove(lineIndex);
        }

        if (_lineLayoutCache.Count >= MaxCachedLineLayouts)
        {
            int keyToEvict = -1;
            foreach (var kvp in _lineLayoutCache)
            {
                if (kvp.Value.layout != CurrentLineTextLayout)
                    {
                    keyToEvict = kvp.Key;
                    break;
                }
            }
            if (keyToEvict >= 0 && _lineLayoutCache.Remove(keyToEvict, out var evicted))
            {
                if (evicted.layout != CurrentLineTextLayout)
                    evicted.layout?.Dispose();
            }
        }

        CanvasTextLayout newLayout = CreateWrappedLineTextLayout(canvasText, lineIndex);
        _lineLayoutCache[lineIndex] = (newLayout, lineText, wrapWidth);
        return newLayout;
    }
    private float cachedWrapWidth;
    private bool wrapMetricsDirty = true;
    // Above this many dirty lines a full rebuild is cheaper than many incremental patches.
    private const int IncrementalWrapRemeasureLimit = 64;
    // Above this line length, estimate the wrapped row count instead of laying the whole line out (a
    // multi-megabyte line would otherwise create a giant measurement layout).
    private const int LongLineRowEstimateThreshold = 20_000;
    // Above this line count, whole-document measurement delegates to row-count estimation
    // to avoid UI freezes during window resizing on huge files (e.g. 2 million lines).
    private const int LargeDocumentWrapEstimateThreshold = 10_000;

    // ── Wrapped-line virtualization (a single line so long it wraps to more rows than fit) ──────────
    // At/above LongLineRowEstimateThreshold a single wrapped line can span tens of thousands of rows, so
    // laying the whole line out is prohibitive. When such a line is the only visible one we render just the
    // visible slice of its rows (estimated char-per-row grid) and map caret/click within it.
    /// <summary>True when the current frame renders a single very-long wrapped line as a row slice.</summary>
    public bool IsVirtualizedWrappedLine { get; internal set; }
    /// <summary>Line index of the virtualized wrapped line, or -1 when none is virtualized.</summary>
    public int VirtualizedLineIndex { get; internal set; } = -1;
    /// <summary>Rows of the virtualized line materialized this frame.</summary>
    public int VirtualizedWrappedRowsToRender { get; internal set; }
    /// <summary>Document char offset where the rendered row slice starts.</summary>
    public int VirtualizedLineSliceStart { get; internal set; }
    /// <summary>Estimated characters per wrapped row for the virtualized line.</summary>
    public int VirtualizedLineCharsPerRow { get; internal set; }
    private int _virtualizedSliceTextLength = 0;
    private const int VirtualizedWrappedLinePaddingRows = 2;



    private CursorManager cursorManager;
    private TextManager textManager;
    public int LinesCount => textManager?.LinesCount ?? 0;
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

        if (coreTextbox?.eventsManager != null)
        {
            coreTextbox.eventsManager.TextChanged += () =>
            {
                MarkLineWrapDirty(cursorManager.LineNumber);
            };
        }
    }

    public void CheckDispose()
    {
        TextFormat?.Dispose();
        DrawnTextLayout?.Dispose();
        ClearLineLayoutCache();
        InvalidateCurrentLineLayout();
        invisibleCharactersRenderer.CheckDispose();
    }

    //Check whether the current line is outside the bounds of the visible area
    public bool OutOfRenderedArea(int line)
    {
        return line < NumberOfStartLine || line >= NumberOfStartLine + NumberOfRenderedLines;
    }

    public void UpdateCurrentLineTextLayout(CanvasControl canvasText)
    {
        if (cursorManager.LineNumber >= textManager.LinesCount || cursorManager.LineNumber < 0)
        {
            InvalidateCurrentLineLayout();
            return;
        }

        EnsureTextFormat();
        if (IsWordWrapEnabled)
            EnsureWrapMetrics(canvasText);

        float wrapWidth = GetWrapWidth(canvasText);
        string currentLineText = textManager.GetLineText(cursorManager.LineNumber);
        bool shouldVirtualize = ShouldVirtualizeWrappedLine(cursorManager.LineNumber);

        // Check if current cached layout is still valid
        if (CurrentLineTextLayout != null &&
            !NeedsTextFormatUpdate &&
            !_renderedTextDirty &&
            _currentLineLayoutLineIndex == cursorManager.LineNumber &&
            _currentLineLayoutIsWrapped == IsWordWrapEnabled &&
            _currentLineLayoutText == currentLineText &&
            (!IsWordWrapEnabled || Math.Abs(_currentLineLayoutWrapWidth - wrapWidth) < 0.5f) &&
            (!shouldVirtualize || (_currentLineLayoutSliceStart == VirtualizedLineSliceStart && _currentLineLayoutSliceLength == VirtualizedWrappedRowsToRender)) &&
            (IsWordWrapEnabled || (!IsHorizontallyVirtualized && _currentLineLayoutSliceStart == -1) ||
             (IsHorizontallyVirtualized && _currentLineLayoutSliceStart == HorizontalSliceStart && _currentLineLayoutSliceLength == HorizontalSliceLength)))
        {
            return;
        }

        // A very-long wrapped line backs its caret/click with a virtualized row-slice layout
        if (shouldVirtualize)
        {
            InvalidateCurrentLineLayout();
            int vLine = VirtualizedLineIndex >= 0 ? VirtualizedLineIndex : NumberOfStartLine;
            bool canReuseRenderedText = !_renderedTextDirty
                && IsVirtualizedWrappedLine
                && cursorManager.LineNumber == vLine
                && cursorManager.LineNumber == NumberOfStartLine
                && !string.IsNullOrEmpty(RenderedText);

            LineSliceResult virtualizedText = canReuseRenderedText
                ? new LineSliceResult(RenderedText, ReadOnlySpan<string>.Empty)
                : BuildVirtualizedWrappedLineRenderData(canvasText, cursorManager.LineNumber);

            CurrentLineTextLayout = textLayoutManager.CreateTextLayout(
                canvasText,
                TextFormat,
                virtualizedText.Text,
                GetWrapWidth(canvasText),
                (float)Math.Max(canvasText.Size.Height, (VirtualizedWrappedRowsToRender + 1) * Math.Max(1, SingleLineHeight)));
            _currentLineLayoutLineIndex = cursorManager.LineNumber;
            _currentLineLayoutText = currentLineText;
            _currentLineLayoutWrapWidth = wrapWidth;
            _currentLineLayoutIsWrapped = true;
            _currentLineLayoutSliceStart = VirtualizedLineSliceStart;
            _currentLineLayoutSliceLength = VirtualizedWrappedRowsToRender;
            return;
        }

        if (IsWordWrapEnabled)
        {
            CanvasTextLayout cachedLayout = GetOrCreateWrappedLineLayout(canvasText, cursorManager.LineNumber);
            if (CurrentLineTextLayout != cachedLayout)
            {
                bool oldInCache = false;
                foreach (var entry in _lineLayoutCache.Values)
                {
                    if (entry.layout == CurrentLineTextLayout)
                    {
                        oldInCache = true;
                        break;
                    }
                }
                if (!oldInCache)
                    CurrentLineTextLayout?.Dispose();

                CurrentLineTextLayout = cachedLayout;
            }
            _currentLineLayoutLineIndex = cursorManager.LineNumber;
            _currentLineLayoutText = currentLineText;
            _currentLineLayoutWrapWidth = wrapWidth;
            _currentLineLayoutIsWrapped = true;
            return;
        }

        // Non-wrapped mode
        InvalidateCurrentLineLayout();
        string lineText = currentLineText + "|";

        // Slice the current line to the SAME horizontal window as the main text (non-wrap only) so the caret
        // and click hit-testing line up with the sliced layout: the caret's rendered index is
        // (documentChar - HorizontalSliceStart) and its x adds HorizontalSlicePixelOffset. A window computed
        // independently here would misplace the caret.
        int sliceStartVal = -1;
        int sliceLenVal = -1;
        if (IsHorizontallyVirtualized && !IsWordWrapEnabled && HorizontalSliceLength > 0)
        {
            int sliceStart = Math.Min(HorizontalSliceStart, lineText.Length);
            int len = Math.Min(HorizontalSliceLength, lineText.Length - sliceStart);
            lineText = len > 0 ? lineText.Substring(sliceStart, len) : string.Empty;
            sliceStartVal = HorizontalSliceStart;
            sliceLenVal = HorizontalSliceLength;
        }

        CurrentLineTextLayout = textLayoutManager.CreateTextLayout(
            canvasText,
            TextFormat,
            lineText,
            canvasText.Size);
        _currentLineLayoutLineIndex = cursorManager.LineNumber;
        _currentLineLayoutText = currentLineText;
        _currentLineLayoutWrapWidth = wrapWidth;
        _currentLineLayoutIsWrapped = false;
        _currentLineLayoutSliceStart = sliceStartVal;
        _currentLineLayoutSliceLength = sliceLenVal;
    }

    // ── Horizontal virtualization helpers ──────────────────────────────────────────────

    /// <summary>Rendered index (inside the current line's sliced layout) for a document column, clamped to
    /// this line's sliced length. When not sliced the rendered index equals the document column, so the
    /// caret/click paths can call this unconditionally.</summary>
    public int GetRenderedCharacterIndexForDocumentCharacter(int lineIndex, int characterPosition)
    {
        // Virtualized wrapped line: the layout is a row slice on an estimated char-per-row grid, so map the
        // document column into that grid (row * (charsPerRow + newline) + column). -1 = outside the slice.
        int vLine = VirtualizedLineIndex >= 0 ? VirtualizedLineIndex : NumberOfStartLine;
        if (IsVirtualizedWrappedLine && lineIndex == vLine && VirtualizedLineCharsPerRow > 0)
        {
            int relative = characterPosition - VirtualizedLineSliceStart;
            if (relative < 0)
                return -1;
            int maxDocumentChars = VirtualizedWrappedRowsToRender * VirtualizedLineCharsPerRow;
            if (relative > maxDocumentChars)
                return -1;
            int row = Math.Min(relative / VirtualizedLineCharsPerRow, Math.Max(0, VirtualizedWrappedRowsToRender - 1));
            return relative + row * textManager.NewLineCharacter.Length;
        }
        if (IsHorizontallyVirtualized)
            return HorizontalSliceMath.RenderedIndexForColumn(characterPosition, HorizontalSliceStart, SlicedLineLength(lineIndex));
        return characterPosition;
    }

    /// <summary>Document column for a rendered index inside the current line's sliced layout, clamped to the
    /// document line length (or line length + 1 when allowNewline is true). When not sliced the document column equals the rendered index.</summary>
    public int GetDocumentCharacterIndexFromRenderedIndex(int lineIndex, int renderedIndex, bool allowNewline = false)
    {
        int maxLen = textManager.GetLineLength(lineIndex) + (allowNewline && lineIndex < textManager.LinesCount - 1 ? 1 : 0);
        int vLine = VirtualizedLineIndex >= 0 ? VirtualizedLineIndex : NumberOfStartLine;
        if (IsVirtualizedWrappedLine && lineIndex == vLine && VirtualizedLineCharsPerRow > 0)
        {
            int rowStride = VirtualizedLineCharsPerRow + textManager.NewLineCharacter.Length;
            int row = Math.Max(0, renderedIndex / rowStride);
            int column = Math.Clamp(renderedIndex % rowStride, 0, VirtualizedLineCharsPerRow);
            int characterPosition = VirtualizedLineSliceStart + row * VirtualizedLineCharsPerRow + column;
            return Math.Clamp(characterPosition, 0, maxLen);
        }
        if (IsHorizontallyVirtualized)
            return Math.Clamp(renderedIndex + HorizontalSliceStart, 0, maxLen);
        return Math.Clamp(renderedIndex, 0, maxLen);
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

    /// <summary>Maps a document character position inside a virtualized wrapped line to its character index
    /// inside <see cref="DrawnTextLayout"/> via row-stride arithmetic. Clamps to slice bounds.</summary>
    public int GetRenderedLayoutIndexForVirtualizedWrappedLine(int characterPosition)
        => GetRenderedLayoutIndexForVirtualizedWrappedLine(VirtualizedLineIndex >= 0 ? VirtualizedLineIndex : NumberOfStartLine, characterPosition);

    /// <summary>Maps a document position (lineIndex, characterPosition) in a frame containing a virtualized
    /// wrapped line to its character index inside <see cref="DrawnTextLayout"/>, correctly handling preceding
    /// lines, row-stride slicing on the virtual line, and clamping.</summary>
    public int GetRenderedLayoutIndexForVirtualizedWrappedLine(int lineIndex, int characterPosition)
    {
        if (!IsVirtualizedWrappedLine || VirtualizedLineCharsPerRow <= 0)
            return -1;

        int vLine = VirtualizedLineIndex >= 0 ? VirtualizedLineIndex : NumberOfStartLine;
        int newlineLen = textManager.NewLineCharacter.Length;
        int renderedTextLen = RenderedText?.Length ?? 0;

        if (lineIndex < NumberOfStartLine)
            return 0;

        if (lineIndex < vLine)
        {
            int prefix = 0;
            for (int i = NumberOfStartLine; i < lineIndex && i < textManager.LinesCount; i++)
            {
                prefix += textManager.GetLineLength(i) + newlineLen;
            }
            int inLine = Math.Clamp(characterPosition, 0, textManager.GetLineLength(lineIndex));
            return Math.Clamp(prefix + inLine, 0, renderedTextLen);
        }
        else if (lineIndex == vLine)
        {
            int prefix = 0;
            for (int i = NumberOfStartLine; i < vLine && i < textManager.LinesCount; i++)
            {
                prefix += textManager.GetLineLength(i) + newlineLen;
            }

            int lineLen = textManager.GetLineLength(vLine);
            int clampedChar = Math.Clamp(characterPosition, 0, lineLen);
            int offsetInSlice = clampedChar - VirtualizedLineSliceStart;
            if (offsetInSlice <= 0)
                return Math.Clamp(prefix, 0, renderedTextLen);

            if (VirtualizedWrappedRowsToRender > 0)
            {
                int maxSliceChars = VirtualizedWrappedRowsToRender * VirtualizedLineCharsPerRow;
                if (offsetInSlice > maxSliceChars)
                    offsetInSlice = maxSliceChars;
            }

            int row = offsetInSlice / VirtualizedLineCharsPerRow;
            int col = offsetInSlice % VirtualizedLineCharsPerRow;
            int rowStride = VirtualizedLineCharsPerRow + newlineLen;
            int indexInVirtual = row * rowStride + col;
            return Math.Clamp(prefix + indexInVirtual, 0, renderedTextLen);
        }
        else
        {
            int offset = _virtualizedSliceTextLength;
            for (int i = vLine + 1; i < lineIndex && i < textManager.LinesCount; i++)
            {
                offset += newlineLen + textManager.GetLineLength(i);
            }
            offset += newlineLen + Math.Clamp(characterPosition, 0, textManager.GetLineLength(lineIndex));
            return Math.Clamp(offset, 0, renderedTextLen);
        }
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

    private float GetWrapWidth(CanvasControl canvasText)
    {
        if (canvasText != null && canvasText.ActualWidth > 10)
            return (float)canvasText.ActualWidth;
        if (coreTextbox != null && coreTextbox.ActualWidth > 10)
            return (float)coreTextbox.ActualWidth;
        if (coreTextbox != null && coreTextbox.Width > 10)
            return (float)coreTextbox.Width;
        if (canvasText != null && canvasText.Width > 10)
            return (float)canvasText.Width;
        return 800f;
    }

    /// <summary>Forces a full wrap-metrics rebuild on the next <see cref="EnsureWrapMetrics"/>.</summary>
    public void InvalidateWrapMetrics()
    {
        wrapMetricsDirty = true;
        _renderedTextDirty = true;
        dirtyWrapLines.Clear();
        ClearLineLayoutCache();
        InvalidateCurrentLineLayout();
    }

    /// <summary>Ensures the document-line ↔ visual-row mapping is current for the wrap width. Rebuilds on a
    /// width change or when the line count changed; otherwise re-measures dirty lines incrementally.</summary>
    public void EnsureWrapMetrics(CanvasControl canvasText)
    {
        EnsureTextFormat();
        if (!IsWordWrapEnabled || canvasText == null || TextFormat == null)
            return;

        float wrapWidth = GetWrapWidth(canvasText);
        if (Math.Abs(cachedWrapWidth - wrapWidth) >= 0.5f)
        {
            cachedWrapWidth = wrapWidth;
            wrapMetricsDirty = true;
            NeedsUpdateTextLayout = true;
            lineNumberRenderer.NeedsUpdateLineNumbers();
            ClearLineLayoutCache();
            InvalidateCurrentLineLayout();
        }

        if (wrapMetricsDirty || !wrapMetrics.IsValidFor(textManager.LinesCount))
        {
            int charsPerRow = EstimateWrappedCharsPerRow(canvasText);
            bool isLargeDoc = textManager.LinesCount >= LargeDocumentWrapEstimateThreshold;

            wrapMetrics.Rebuild(textManager.LinesCount, i => MeasureWrappedRowCount(canvasText, i, charsPerRow, isLargeDoc));
            wrapMetricsDirty = false;
            dirtyWrapLines.Clear();
            ClearLineLayoutCache();
            InvalidateCurrentLineLayout();
            return;
        }

        if (dirtyWrapLines.Count > 0)
        {
            int charsPerRow = EstimateWrappedCharsPerRow(canvasText);
            bool isLargeDoc = textManager.LinesCount >= LargeDocumentWrapEstimateThreshold;

            bool patched = wrapMetrics.ApplyIncremental(
                textManager.LinesCount, dirtyWrapLines, dirtyWrapLines.Count, IncrementalWrapRemeasureLimit,
                i => MeasureWrappedRowCount(canvasText, i, charsPerRow, isLargeDoc));
            if (!patched)
            {
                wrapMetrics.Rebuild(textManager.LinesCount, i => MeasureWrappedRowCount(canvasText, i, charsPerRow, isLargeDoc));
                wrapMetricsDirty = false;
                ClearLineLayoutCache();
                InvalidateCurrentLineLayout();
            }
            dirtyWrapLines.Clear();
        }
    }

    private int MeasureWrappedRowCount(CanvasControl canvasText, int lineIndex, int charsPerRow = 0, bool isLargeDoc = false)
    {
        if (lineIndex < 0 || lineIndex >= textManager.LinesCount)
            return 1;

        int lineLength = textManager.GetLineLength(lineIndex);
        if (lineLength == 0)
            return 1;

        if (charsPerRow <= 0)
            charsPerRow = EstimateWrappedCharsPerRow(canvasText);

        // Fast path: if the line length is less than or equal to the maximum characters that can
        // fit on one row, it is mathematically impossible for the line to wrap.
        if (lineLength <= charsPerRow)
            return 1;

        if (lineLength >= LongLineRowEstimateThreshold || isLargeDoc)
            return EstimateWrappedRowCount(canvasText, lineLength, charsPerRow);

        string lineText = textManager.GetLineText(lineIndex);
        float singleLineHeight = Math.Max(1, SingleLineHeight);
        float wrapWidth = cachedWrapWidth > 1 ? cachedWrapWidth : GetWrapWidth(canvasText);
        float layoutHeight = Math.Max(singleLineHeight, (lineText.Length + 1) * singleLineHeight);
        using CanvasTextLayout lineLayout = textLayoutManager.CreateTextLayout(canvasText, TextFormat, lineText, wrapWidth, layoutHeight);
        // Avoid CanvasTextLayout.LineMetrics (CanvasLineMetrics is non-blittable and throws
        // NotSupportedException on newer .NET). Also avoid lineLayout.LayoutBounds.Height / singleLineHeight
        // because DirectWrite LayoutBounds.Height reflects font ascent/descent leading rather than LineSpacing,
        // which rounds up and incorrectly measures 1-row lines as 2 rows.
        // The distance between the last character's caret Y and the first character's caret Y is an exact
        // multiple of SingleLineHeight (LineSpacing).
        float baseRowY = lineLayout.GetCaretPosition(0, false).Y;
        float endRowY = lineLayout.GetCaretPosition(lineText.Length, false).Y;
        int rowCount = 1 + (int)Math.Round(Math.Max(0, endRowY - baseRowY) / singleLineHeight);
        return Math.Max(1, rowCount);
    }

    // ── Wrapped-line virtualization helpers ──────────────────────────────────────────────

    private void ResetVirtualizedWrappedLineState()
    {
        IsVirtualizedWrappedLine = false;
        VirtualizedWrappedRowsToRender = 0;
        VirtualizedLineSliceStart = 0;
        VirtualizedLineCharsPerRow = 0;
        VirtualizedLineIndex = -1;
        _virtualizedSliceTextLength = 0;
    }

    internal bool ShouldVirtualizeWrappedLine(int lineIndex)
        => IsWordWrapEnabled
           && lineIndex >= 0
           && lineIndex < textManager.LinesCount
           && textManager.GetLineLength(lineIndex) >= LongLineRowEstimateThreshold;

    private bool HasVirtualizedLineInRenderRange(int startLine, int lineCount, out int virtualLineIndex)
    {
        for (int i = startLine; i < startLine + lineCount && i < textManager.LinesCount; i++)
        {
            if (ShouldVirtualizeWrappedLine(i))
            {
                virtualLineIndex = i;
                return true;
            }
        }
        virtualLineIndex = -1;
        return false;
    }

    private LineSliceResult BuildMultiLineWithVirtualizedWrappedLineRenderData(CanvasControl canvasText, int startLine, int lineCount, int virtualLineIndex)
    {
        var builder = new StringBuilder();
        int rowsRenderedSoFar = 0;

        for (int i = startLine; i < virtualLineIndex; i++)
        {
            if (i > startLine)
                builder.Append(textManager.NewLineCharacter);
            builder.Append(textManager.GetLineText(i));
            rowsRenderedSoFar += GetWrappedRowCount(i);
        }

        if (virtualLineIndex > startLine)
            builder.Append(textManager.NewLineCharacter);

        string virtualLineText = textManager.GetLineText(virtualLineIndex);
        int charsPerRow = EstimateWrappedCharsPerRow(canvasText);
        int totalVirtualRows = Math.Max(1, (int)Math.Ceiling(virtualLineText.Length / (double)charsPerRow));
        int remainingVisibleRows = Math.Max(1, GetVisibleVisualRowCount(canvasText, VirtualizedWrappedLinePaddingRows) - rowsRenderedSoFar);
        int rowsToRender = Math.Min(totalVirtualRows, remainingVisibleRows);

        int offset = 0;
        int rowsRendered = 0;
        while (rowsRendered < rowsToRender && offset < virtualLineText.Length)
        {
            if (rowsRendered > 0)
                builder.Append(textManager.NewLineCharacter);

            int length = Math.Min(charsPerRow, virtualLineText.Length - offset);
            builder.Append(virtualLineText, offset, length);
            offset += length;
            rowsRendered++;
        }

        if (rowsRendered == 0)
            rowsRendered = 1;

        IsVirtualizedWrappedLine = true;
        VirtualizedLineIndex = virtualLineIndex;
        VirtualizedWrappedRowsToRender = rowsRenderedSoFar + rowsRendered;
        VirtualizedLineSliceStart = 0;
        VirtualizedLineCharsPerRow = charsPerRow;
        return new LineSliceResult(builder.ToString(), ReadOnlySpan<string>.Empty);
    }

    internal int EstimateWrappedCharsPerRow(CanvasControl canvasText)
    {
        float wrapWidth = cachedWrapWidth > 1 ? cachedWrapWidth : GetWrapWidth(canvasText);
        float charWidth = CachedCharWidth;
        return Math.Max(1, (int)Math.Floor(wrapWidth / charWidth));
    }

    private int EstimateWrappedRowCount(CanvasControl canvasText, int textLength, int charsPerRow = 0)
    {
        if (charsPerRow <= 0)
            charsPerRow = EstimateWrappedCharsPerRow(canvasText);
        return Math.Max(1, (int)Math.Ceiling(textLength / (double)charsPerRow));
    }

    /// <summary>Builds the visible ROW slice of a very-long wrapped line (never materializing the whole
    /// multi-megabyte line): starting at <see cref="WrappedStartRowOffset"/>, laid out on an estimated
    /// char-per-row grid and joined by the newline. Records the slice metrics for caret/click mapping.</summary>
    private LineSliceResult BuildVirtualizedWrappedLineRenderData(CanvasControl canvasText, int lineIndex, int lineCount = 1)
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

        int virtualizedRows = rowsRendered;
        _virtualizedSliceTextLength = builder.Length;

        for (int i = lineIndex + 1; i < lineIndex + lineCount && i < textManager.LinesCount; i++)
        {
            builder.Append(textManager.NewLineCharacter);
            builder.Append(textManager.GetLineText(i));
        }

        IsVirtualizedWrappedLine = true;
        VirtualizedLineIndex = lineIndex;
        VirtualizedWrappedRowsToRender = virtualizedRows;
        VirtualizedLineSliceStart = sliceStart;
        VirtualizedLineCharsPerRow = charsPerRow;
        return new LineSliceResult(builder.ToString(), ReadOnlySpan<string>.Empty);
    }

    private CanvasTextLayout CreateWrappedLineTextLayout(CanvasControl canvasText, int lineIndex)
    {
        EnsureTextFormat();
        string lineText = textManager.GetLineText(lineIndex);

        float singleLineHeight = Math.Max(1, SingleLineHeight);
        int rowCount = GetWrappedRowCount(lineIndex);
        float layoutHeight = (float)Math.Max(Math.Max(singleLineHeight, (float)canvasText.Size.Height), (rowCount + 1) * singleLineHeight);
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
    {
        float height = canvasText != null && canvasText.ActualHeight > 10
            ? (float)canvasText.ActualHeight
            : (coreTextbox != null && coreTextbox.ActualHeight > 10
                ? (float)coreTextbox.ActualHeight
                : (scrollGrid != null && scrollGrid.ActualHeight > 10 ? (float)scrollGrid.ActualHeight : 600f));
        return Math.Max(1, (int)Math.Ceiling(height / Math.Max(1, SingleLineHeight))) + extraRows;
    }

    public int GetRenderedVisualRowCount(int startLine, int lineCount)
    {
        if (!IsWordWrapEnabled)
            return lineCount;
        return wrapMetrics.GetRenderedVisualRowCount(startLine, lineCount, textManager.LinesCount);
    }

    public int GetVisualRowFromPointY(double y)
        => WrapGeometry.CalculateVisualRowFromPointY(y, StartVisualRow, SingleLineHeight, scrollManager.DefaultVerticalScrollSensitivity, TopInset);

    public float GetWrappedLineHitTestYFromPointY(int lineIndex, double y)
    {
        // A virtualized wrapped line renders pinned to the viewport top when it's the start line,
        // or below preceding lines when multi-line. Hit-test relative to its visual line top.
        int vLine = VirtualizedLineIndex >= 0 ? VirtualizedLineIndex : NumberOfStartLine;
        if (IsVirtualizedWrappedLine && lineIndex == vLine)
        {
            float lineTopY = GetCurrentLineLayoutTopY(lineIndex);
            return WrapGeometry.CalculateWrappedLineHitTestYFromPointY(y, lineTopY, SingleLineHeight, scrollManager.DefaultVerticalScrollSensitivity, Math.Max(1, VirtualizedWrappedRowsToRender), TopInset);
        }
        return WrapGeometry.CalculateWrappedLineHitTestYFromPointY(y, GetLineTopY(lineIndex), SingleLineHeight, scrollManager.DefaultVerticalScrollSensitivity, GetWrappedRowCount(lineIndex), TopInset);
    }

    /// <summary>
    /// Gets the Y offset of the layout used to render the current line (or hit-test it).
    /// For normal lines this is <see cref="GetLineTopY(int)"/>.
    /// For a virtualized wrapped line, the layout is sliced to the visible rows, so when it is
    /// the start line, the layout starts at the viewport top (Y = 0).
    /// </summary>
    public float GetCurrentLineLayoutTopY(int lineIndex)
    {
        int vLine = VirtualizedLineIndex >= 0 ? VirtualizedLineIndex : NumberOfStartLine;
        if (IsWordWrapEnabled && IsVirtualizedWrappedLine && lineIndex == vLine)
        {
            return lineIndex == NumberOfStartLine ? 0 : GetLineTopY(lineIndex);
        }
        return GetLineTopY(lineIndex);
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
            if (!cursorManager.PreferredCharacterPosition.HasValue)
                cursorManager.PreferredCharacterPosition = characterPosition % charsPerRow;

            int currentColumn = cursorManager.PreferredCharacterPosition.Value;
            if (!cursorManager.PreferredCaretX.HasValue)
                cursorManager.PreferredCaretX = currentColumn * _cachedCharWidth;

            int virtualCurrentVisualRow = GetLineVisualStartRow(lineIndex) + characterPosition / charsPerRow;
            int virtualTargetVisualRow = Math.Clamp(virtualCurrentVisualRow + rowDelta, 0, Math.Max(0, wrapMetrics.TotalVisualRows - 1));
            if (virtualTargetVisualRow == virtualCurrentVisualRow)
                return false;

            int virtualTargetLine = GetDocumentLineFromVisualRow(virtualTargetVisualRow);
            int virtualTargetRowOffset = virtualTargetVisualRow - GetLineVisualStartRow(virtualTargetLine);
            int targetLength = textManager.GetLineLength(virtualTargetLine);
            int targetCharsPerRow = ShouldVirtualizeWrappedLine(virtualTargetLine) ? EstimateWrappedCharsPerRow(canvasText) : charsPerRow;

            cursorPosition.LineNumber = virtualTargetLine;
            cursorPosition.CharacterPosition = Math.Clamp(virtualTargetRowOffset * targetCharsPerRow + currentColumn, 0, targetLength);
            return true;
        }

        CanvasTextLayout currentLayout = GetOrCreateWrappedLineLayout(canvasText, lineIndex);
        if (currentLayout == null)
            return false;

        float baseRowY = currentLayout.GetCaretPosition(0, false).Y;
        var currentCaret = currentLayout.GetCaretPosition(characterPosition, false);
        int withinLineRow = (int)Math.Round((currentCaret.Y - baseRowY) / Math.Max(1, SingleLineHeight));
        withinLineRow = Math.Clamp(withinLineRow, 0, Math.Max(0, GetWrappedRowCount(lineIndex) - 1));
        int currentVisualRow = GetLineVisualStartRow(lineIndex) + withinLineRow;
        int targetVisualRow = Math.Clamp(currentVisualRow + rowDelta, 0, Math.Max(0, wrapMetrics.TotalVisualRows - 1));
        if (targetVisualRow == currentVisualRow)
            return false;

        // Remember the preferred X position for vertical navigation
        if (!cursorManager.PreferredCaretX.HasValue)
            cursorManager.PreferredCaretX = currentCaret.X;

        float targetCaretX = cursorManager.PreferredCaretX.Value;

        int targetLine = GetDocumentLineFromVisualRow(targetVisualRow);
        int targetRowOffset = targetVisualRow - GetLineVisualStartRow(targetLine);
        targetRowOffset = Math.Clamp(targetRowOffset, 0, Math.Max(0, GetWrappedRowCount(targetLine) - 1));

        if (ShouldVirtualizeWrappedLine(targetLine))
        {
            int targetCharsPerRow = EstimateWrappedCharsPerRow(canvasText);
            int targetLength = textManager.GetLineLength(targetLine);
            int targetCol = (int)Math.Round(targetCaretX / Math.Max(1, _cachedCharWidth));
            cursorPosition.LineNumber = targetLine;
            cursorPosition.CharacterPosition = Math.Clamp(targetRowOffset * targetCharsPerRow + targetCol, 0, targetLength);
            return true;
        }

        CanvasTextLayout targetLayout = (targetLine == lineIndex)
            ? currentLayout
            : GetOrCreateWrappedLineLayout(canvasText, targetLine);
        if (targetLayout == null)
            return false;

        float targetHitY = (targetRowOffset + 0.5f) * Math.Max(1, SingleLineHeight);
        targetLayout.HitTest(targetCaretX, targetHitY, out var targetRegion, out bool isTrailingHit);

        int targetCharIndex = targetRegion.CharacterIndex + (isTrailingHit ? 1 : 0);
        if (targetCharIndex > targetRegion.CharacterIndex)
        {
            float baseRowYTarget = targetLayout.GetCaretPosition(0, false).Y;
            var caretPos = targetLayout.GetCaretPosition(targetCharIndex, false);
            int resolvedRow = (int)Math.Round((caretPos.Y - baseRowYTarget) / Math.Max(1, SingleLineHeight));
            if (resolvedRow > targetRowOffset)
            {
                // Trailing hit crossed to the next visual row; keep cursor on targetRowOffset
                targetCharIndex = targetRegion.CharacterIndex;
            }
        }

        cursorPosition.LineNumber = targetLine;
        cursorPosition.CharacterPosition = Math.Clamp(targetCharIndex, 0, textManager.GetLineLength(targetLine));
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

        float viewportHeight = coreTextbox.canvasText != null && coreTextbox.canvasText.ActualHeight > 10
            ? (float)coreTextbox.canvasText.ActualHeight
            : (coreTextbox != null && coreTextbox.ActualHeight > 10
                ? (float)coreTextbox.ActualHeight
                : (scrollGrid != null && scrollGrid.ActualHeight > 10 ? (float)scrollGrid.ActualHeight : 600f));

        //Measure text position and apply the value to the scrollbar
        scrollManager.verticalScrollBar.Maximum = Math.Max(0, ((textManager.LinesCount + 1) * singleLineHeight - viewportHeight) / scrollManager.DefaultVerticalScrollSensitivity);
        scrollManager.verticalScrollBar.ViewportSize = viewportHeight;

        //Calculate number of lines that need to be rendered
        int linesToRenderCount = (int)(viewportHeight / singleLineHeight);
        linesToRenderCount = Math.Min(linesToRenderCount, textManager.LinesCount);

        int startLine;
        if (zoomManager.ZoomAnchorLine.HasValue)
        {
            startLine = Math.Clamp(zoomManager.ZoomAnchorLine.Value, 0, Math.Max(0, textManager.LinesCount - 1));
            double targetScrollValue = (startLine * singleLineHeight) / scrollManager.DefaultVerticalScrollSensitivity;
            scrollManager.verticalScrollBar.Value = Math.Clamp(targetScrollValue, 0, scrollManager.verticalScrollBar.Maximum);
            startLine = (int)((scrollManager.verticalScrollBar.Value * scrollManager.DefaultVerticalScrollSensitivity) / singleLineHeight);
            startLine = Math.Min(startLine, textManager.LinesCount);
        }
        else
        {
            startLine = (int)((scrollManager.VerticalScroll * scrollManager.DefaultVerticalScrollSensitivity) / singleLineHeight);
            startLine = Math.Min(startLine, textManager.LinesCount);
        }

        if (zoomManager.ZoomAnchorHorizontalRatio > 0 && scrollManager.horizontalScrollBar != null)
        {
            scrollManager.horizontalScrollBar.Value = Math.Clamp(zoomManager.ZoomAnchorHorizontalRatio * zoomManager.ZoomedFontSize, scrollManager.horizontalScrollBar.Minimum, scrollManager.horizontalScrollBar.Maximum);
        }

        int linesToRender = Math.Min(linesToRenderCount, textManager.LinesCount - startLine);

        return (startLine, linesToRender);
    }

    public const int WrappedBottomBufferRows = 2;

    private (int startLine, int linesToRender) CalculateWrappedLinesToRender(CanvasControl canvasText, float singleLineHeight)
    {
        EnsureWrapMetrics(canvasText);
        int totalVisualRows = wrapMetrics.TotalVisualRows;

        double viewportHeight = canvasText != null && canvasText.ActualHeight > 0 ? canvasText.ActualHeight : scrollGrid.ActualHeight;

        // In word-wrap mode, the layout is drawn with a top offset of SingleLineHeight.
        // Adding WrappedBottomBufferRows (5) visual rows to the scroll extent ensures:
        // 1) The SingleLineHeight draw offset is accounted for so the final visual row is fully visible.
        // 2) The fractional sub-line remainder (viewportHeight % singleLineHeight) is absorbed across any window height.
        // 3) A generous bottom buffer (3-4 visual rows) is provided below the last line so the final row
        //    can always be scrolled completely into view with clear breathing room.
        scrollManager.verticalScrollBar.Maximum = Math.Max(0, ((totalVisualRows + WrappedBottomBufferRows) * singleLineHeight - viewportHeight) / scrollManager.DefaultVerticalScrollSensitivity);
        scrollManager.verticalScrollBar.ViewportSize = viewportHeight;

        if (zoomManager.ZoomAnchorLine.HasValue)
        {
            int anchorDocLine = Math.Clamp(zoomManager.ZoomAnchorLine.Value, 0, Math.Max(0, textManager.LinesCount - 1));
            int targetVisualRow = GetLineVisualStartRow(anchorDocLine);
            double targetScrollValue = (targetVisualRow * singleLineHeight) / scrollManager.DefaultVerticalScrollSensitivity;
            scrollManager.verticalScrollBar.Value = Math.Clamp(targetScrollValue, 0, scrollManager.verticalScrollBar.Maximum);
        }

        StartVisualRow = GetStartVisualRowFromScroll();

        int startLine = GetDocumentLineFromVisualRow(StartVisualRow);
        int startLineVisualRow = GetLineVisualStartRow(startLine);
        WrappedStartRowOffset = Math.Max(0, StartVisualRow - startLineVisualRow);

        int visibleRows = GetVisibleVisualRowCount(canvasText, 2);
        if (ShouldVirtualizeWrappedLine(startLine))
        {
            int startLineRowCount = GetWrappedRowCount(startLine);
            int remainingRowsInStartLine = Math.Max(0, startLineRowCount - WrappedStartRowOffset);
            if (remainingRowsInStartLine >= visibleRows)
            {
                return (startLine, 1);
            }

            int rowsToCover = visibleRows - remainingRowsInStartLine;
            int linesToRender = 1;
            for (int i = startLine + 1; i < textManager.LinesCount && rowsToCover > 0; i++)
            {
                rowsToCover -= GetWrappedRowCount(i);
                linesToRender++;
                if (ShouldVirtualizeWrappedLine(i))
                    break;
            }
            return (startLine, linesToRender);
        }

        int rowsToCoverFromStart = visibleRows + WrappedStartRowOffset;
        int normalLinesToRender = 0;
        for (int i = startLine; i < textManager.LinesCount && rowsToCoverFromStart > 0; i++)
        {
            if (i > startLine && ShouldVirtualizeWrappedLine(i))
            {
                normalLinesToRender++;
                break;
            }
            rowsToCoverFromStart -= GetWrappedRowCount(i);
            normalLinesToRender++;
        }

        return (startLine, Math.Max(0, normalLinesToRender));
    }

    public void Draw(CanvasControl canvasText, CanvasDrawEventArgs args)
    {
        //Create resources and layouts:
        if (NeedsTextFormatUpdate || TextFormat == null || lineNumberRenderer.LineNumberTextFormat == null)
        {
            lineNumberRenderer.CreateLineNumberTextFormat();

            EnsureTextFormat();
            longestLineManager.needsRecalculation = true;

            invisibleCharactersRenderer.UpdateTextFormat(canvasText, TextFormat);

            designHelper.CreateColorResources(args.DrawingSession);

            // Measure the actual character width (monospace assumption) for the horizontal-virtualization
            // slice-to-pixel offset. Re-measured whenever the format is rebuilt (font/zoom change).
            using (var measureLayout = new CanvasTextLayout(args.DrawingSession, "M", TextFormat, 0, 0))
            {
                var regions = measureLayout.GetCharacterRegions(0, 1);
                _cachedCharWidth = Math.Max(1, (float)(regions.Length > 0 ? regions[0].LayoutBounds.Width : measureLayout.LayoutBounds.Width));
            }
        }

        (NumberOfStartLine, NumberOfRenderedLines) = CalculateLinesToRender();
        coreTextbox.SyncScrollTrackerToOffsetNow();

        scrollManager.EnsureHorizontalScrollBounds(canvasText, longestLineManager, false, zoomManager.ZoomNeedsRecalculateLongestLine);
        if (zoomManager.ZoomNeedsRecalculateLongestLine)
            zoomManager.ZoomNeedsRecalculateLongestLine = false;

        // Decide horizontal virtualization BEFORE materializing the visible text. Joining many very long
        // lines (megabytes) into one string every frame — then laying it out — is the actual scroll-stutter
        // cost on files like JSONL logs (hundreds of 50k+ char lines). When any visible line is very long we
        // build ONLY the sliced text (a few KB) instead of the full join, and record a per-line prefix so the
        // caret/selection can still map document positions into the sliced multi-line layout.
        LineSliceResult renderTextData;
        ResetVirtualizedWrappedLineState();
        if (IsWordWrapEnabled && ShouldVirtualizeWrappedLine(NumberOfStartLine))
        {
            // A wrapped line so long it is virtualized: render only its visible ROW
            // slice (a few KB) instead of laying out the whole multi-megabyte line.
            renderTextData = BuildVirtualizedWrappedLineRenderData(canvasText, NumberOfStartLine, NumberOfRenderedLines);
            RenderedText = renderTextData.Text;
            IsHorizontallyVirtualized = false;
            HorizontalSliceStart = 0;
            HorizontalSliceLength = 0;
            _hSliceVisibleStart = 0;
            _hSliceVisibleEnd = 0;
        }
        else if (IsWordWrapEnabled && HasVirtualizedLineInRenderRange(NumberOfStartLine, NumberOfRenderedLines, out int virtualLineIndex))
        {
            renderTextData = BuildMultiLineWithVirtualizedWrappedLineRenderData(canvasText, NumberOfStartLine, NumberOfRenderedLines, virtualLineIndex);
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
        float zoomFactor = (zoomManager == null ? 100f : zoomManager._ZoomFactor) / 100f;
        float textVerticalAdjustment = Math.Max(1f, (float)Math.Round(1.5f * zoomFactor));

        float drawTextOffsetX = IsWordWrapEnabled ? 0 : HorizontalOffset;
        float drawTextOffsetY = (IsWordWrapEnabled
            ? (IsVirtualizedWrappedLine ? SingleLineHeight : SingleLineHeight - (WrappedStartRowOffset * SingleLineHeight))
            : SingleLineHeight) - textVerticalAdjustment;
        float searchHighlightOffsetY = GetSelectionTopMargin();

        int renderedVisualRows = IsVirtualizedWrappedLine
            ? VirtualizedWrappedRowsToRender + (NumberOfRenderedLines > 1 ? GetRenderedVisualRowCount(NumberOfStartLine + 1, NumberOfRenderedLines - 1) : 0)
            : GetRenderedVisualRowCount(NumberOfStartLine, NumberOfRenderedLines);
        float computedLayoutHeight = IsVirtualizedWrappedLine
            ? Math.Max((float)canvasText.Size.Height + (VirtualizedWrappedLinePaddingRows * SingleLineHeight), (renderedVisualRows + 2) * SingleLineHeight)
            : Math.Max((float)canvasText.Size.Height + (WrappedStartRowOffset + 2) * SingleLineHeight, (renderedVisualRows + 2) * SingleLineHeight);
        computedLayoutHeight = Math.Min(16000f, computedLayoutHeight);

        Size layoutSize = IsWordWrapEnabled
            ? new Size
            {
                Height = computedLayoutHeight,
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
            if (IsVirtualizedWrappedLine && _currentLineLayoutLineIndex == NumberOfStartLine)
            {
                InvalidateCurrentLineLayout();
            }
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
                    drawTextOffsetX,
                    searchHighlightOffsetY,
                    designHelper._Design.SearchHighlightColor
                    );

            ccls.DrawTextLayout(DrawnTextLayout, drawTextOffsetX, drawTextOffsetY, designHelper.TextColorBrush);

            invisibleCharactersRenderer.DrawTabsAndSpaces(args, ccls, RenderedText, DrawnTextLayout, drawTextOffsetY);
        }
        args.DrawingSession.DrawImage(canvasCommandList);

        //Only update if needed, to reduce updates when scrolling
        if (lineNumberRenderer.CanUpdateCanvas())
        {
            canvasUpdateManager.UpdateLineNumbers();
        }
    }
}
