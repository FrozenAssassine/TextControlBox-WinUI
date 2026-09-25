using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Helper;

namespace TextControlBoxNS.Core;

internal class ScrollManager
{

    public double _HorizontalScrollSensitivity = 1;
    public double _VerticalScrollSensitivity = 1;
    public int DefaultVerticalScrollSensitivity = 4;
    public float OldHorizontalScrollValue = 0;

    // The single pixel-based scroll-position seam (see IScrollOffsetSource). Phase 1 backs it with the
    // two ScrollBar primitives; VerticalScroll / HorizontalScroll below keep their legacy scrollbar-unit /
    // pixel API semantics for the public surface but store through the pixel offset source.
    public IScrollOffsetSource OffsetSource { get; private set; }

    public double VerticalScroll { get => OffsetSource.VerticalOffset / DefaultVerticalScrollSensitivity; set { zoomManager.ZoomAnchorLine = null; OffsetSource.VerticalOffset = (value < 0 ? 0 : value) * DefaultVerticalScrollSensitivity; canvasHelper.UpdateAll(); } }
    public double HorizontalScroll { get => OffsetSource.HorizontalOffset; set { OffsetSource.HorizontalOffset = value < 0 ? 0 : value; canvasHelper.UpdateAll(); } }

    public ScrollBar verticalScrollBar;
    public ScrollBar horizontalScrollBar;
    private CanvasUpdateManager canvasHelper;
    private TextRenderer textRenderer;
    private CursorManager cursorManager;
    private TextManager textManager;
    private CoreTextControlBox coreTextbox;
    private Grid scrollGrid;
    private ZoomManager zoomManager;
    public void Init(CoreTextControlBox coreTextbox, CanvasUpdateManager canvasHelper, TextManager textManager, TextRenderer textRenderer, CursorManager cursorManager, ZoomManager zoomManager, ScrollBar verticalScrollBar, ScrollBar horizontalScrollBar)
    {
        this.verticalScrollBar = coreTextbox.verticalScrollBar;
        this.horizontalScrollBar = coreTextbox.horizontalScrollBar;
        scrollGrid = coreTextbox.scrollGrid;
        this.canvasHelper = canvasHelper;
        this.textRenderer = textRenderer;
        this.cursorManager = cursorManager;
        this.textManager = textManager;
        this.coreTextbox = coreTextbox;
        this.zoomManager = zoomManager;
        OffsetSource = new ScrollBarOffsetSource(this.verticalScrollBar, this.horizontalScrollBar, DefaultVerticalScrollSensitivity);
        verticalScrollBar.Loaded += VerticalScrollbar_Loaded;
        verticalScrollBar.Scroll += VerticalScrollBar_Scroll;
        horizontalScrollBar.Scroll += HorizontalScrollBar_Scroll;
    }

    internal void VerticalScrollbar_Loaded(object sender, RoutedEventArgs e)
    {
        if (textRenderer.IsWordWrapEnabled)
        {
            textRenderer.CalculateLinesToRender();
            return;
        }
        verticalScrollBar.Maximum = ((textManager.LinesCount + 1) * textRenderer.SingleLineHeight - scrollGrid.ActualHeight) / DefaultVerticalScrollSensitivity;
        verticalScrollBar.ViewportSize = coreTextbox.ActualHeight;
    }
    internal void VerticalScrollBar_Scroll(object sender, ScrollEventArgs e)
    {
        zoomManager.ZoomAnchorLine = null;
        if (textRenderer.IsWordWrapEnabled)
        {
            canvasHelper.UpdateAll();
            return;
        }

        //only update when a line was scrolled
        if ((int)(OffsetSource.VerticalOffset / textRenderer.SingleLineHeight) != textRenderer.NumberOfStartLine)
        {
            canvasHelper.UpdateAll();
        }
    }

    internal void HorizontalScrollBar_Scroll(object sender, ScrollEventArgs e)
    {
        canvasHelper.UpdateAll();
    }

    public void UpdateWhenScrolled()
    {
        canvasHelper.UpdateAll();
    }

    public void ScrollLineIntoViewIfOutside(int line, bool update = true)
    {
        if (textRenderer.OutOfRenderedArea(line))
            ScrollLineIntoView(line, update);
    }

    public void ScrollOneLineUp(bool update = true)
    {
        zoomManager.ZoomAnchorLine = null;
        OffsetSource.VerticalOffset -= textRenderer.SingleLineHeight;
        if(update)
            canvasHelper.UpdateAll();
    }
    public void ScrollOneLineDown(bool update = true)
    {
        zoomManager.ZoomAnchorLine = null;
        OffsetSource.VerticalOffset += textRenderer.SingleLineHeight;
        if(update)
            canvasHelper.UpdateAll();
    }

    public void ScrollLineIntoView(int line, bool update = true)
    {
        zoomManager.ZoomAnchorLine = null;
        OffsetSource.VerticalOffset = (line - textRenderer.NumberOfRenderedLines / 2) * textRenderer.SingleLineHeight;
        
        if(update)
            canvasHelper.UpdateAll();
    }

    public void ScrollTopIntoView(bool update = true)
    {
        zoomManager.ZoomAnchorLine = null;
        OffsetSource.VerticalOffset = (cursorManager.LineNumber - 1) * textRenderer.SingleLineHeight;
        if(update)
            canvasHelper.UpdateAll();
    }
    public void ScrollBottomIntoView(bool update = true)
    {
        zoomManager.ZoomAnchorLine = null;
        OffsetSource.VerticalOffset = (cursorManager.LineNumber - textRenderer.NumberOfRenderedLines + 1) * textRenderer.SingleLineHeight;
        if(update)
            canvasHelper.UpdateAll();
    }

    public void ScrollPageUp()
    {
        zoomManager.ZoomAnchorLine = null;
        if (!cursorManager.PreferredCharacterPosition.HasValue)
            cursorManager.PreferredCharacterPosition = cursorManager.CharacterPosition;

        cursorManager.LineNumber -= textRenderer.NumberOfRenderedLines;
        if (cursorManager.LineNumber < 0)
            cursorManager.LineNumber = 0;

        cursorManager.CharacterPosition = Math.Clamp(cursorManager.PreferredCharacterPosition.Value, 0, textManager.GetLineLength(cursorManager.LineNumber));
        OffsetSource.VerticalOffset -= textRenderer.NumberOfRenderedLines * textRenderer.SingleLineHeight;
        canvasHelper.UpdateAll();
    }


    public void ScrollPageDown()
    {
        zoomManager.ZoomAnchorLine = null;
        if (!cursorManager.PreferredCharacterPosition.HasValue)
            cursorManager.PreferredCharacterPosition = cursorManager.CharacterPosition;

        cursorManager.LineNumber += textRenderer.NumberOfRenderedLines;
        if (cursorManager.LineNumber > textManager.LinesCount - 1)
            cursorManager.LineNumber = textManager.LinesCount - 1;

        cursorManager.CharacterPosition = Math.Clamp(cursorManager.PreferredCharacterPosition.Value, 0, textManager.GetLineLength(cursorManager.LineNumber));
        OffsetSource.VerticalOffset += textRenderer.NumberOfRenderedLines * textRenderer.SingleLineHeight;
        canvasHelper.UpdateAll();
    }

    public void UpdateScrollToShowCursor(bool update = true)
    {
        zoomManager.ZoomAnchorLine = null;
        if (textRenderer.IsWordWrapEnabled)
        {
            UpdateScrollToShowCursorWrapped(update);
            return;
        }

        if (textRenderer.NumberOfStartLine + textRenderer.NumberOfRenderedLines - 1 <= cursorManager.LineNumber)
        {
            OffsetSource.VerticalOffset =
                (cursorManager.LineNumber - textRenderer.NumberOfRenderedLines + 2) *
                textRenderer.SingleLineHeight;
        }
        else if (textRenderer.NumberOfStartLine > cursorManager.LineNumber)
        {
            OffsetSource.VerticalOffset =
                cursorManager.LineNumber *
                textRenderer.SingleLineHeight;
        }

        if (coreTextbox.canvasText != null && coreTextbox.longestLineManager != null)
        {
            EnsureHorizontalScrollBounds(coreTextbox.canvasText, coreTextbox.longestLineManager, false);
        }

        if (update)
            canvasHelper.UpdateAll();
    }

    private void UpdateScrollToShowCursorWrapped(bool update)
    {
        float singleLine = textRenderer.SingleLineHeight;
        if (singleLine <= 0)
        {
            if (update) canvasHelper.UpdateAll();
            return;
        }

        float canvasHeight = (float)(coreTextbox.canvasText?.ActualHeight ?? 0);
        if (canvasHeight <= 10 && coreTextbox != null && coreTextbox.ActualHeight > 10)
            canvasHeight = (float)coreTextbox.ActualHeight;
        if (canvasHeight <= 10 && coreTextbox?.scrollGrid != null && coreTextbox.scrollGrid.ActualHeight > 10)
            canvasHeight = (float)coreTextbox.scrollGrid.ActualHeight;
        if (canvasHeight <= 10)
            canvasHeight = 600f;

        int lineIndex = Math.Clamp(cursorManager.LineNumber, 0, Math.Max(0, textManager.LinesCount - 1));

        // If the current line layout is available and contains the cursor, check screen visibility directly
        textRenderer.UpdateCurrentLineTextLayout(coreTextbox.canvasText);
        if (textRenderer.CurrentLineTextLayout != null)
        {
            int renderedPos = textRenderer.GetRenderedCharacterIndexForDocumentCharacter(lineIndex, cursorManager.CharacterPosition);
            if (renderedPos >= 0)
            {
                float baseRowY = textRenderer.CurrentLineTextLayout.GetCaretPosition(0, false).Y;
                var vector = textRenderer.CurrentLineTextLayout.GetCaretPosition(renderedPos, false);
                int visualRow = (int)Math.Round((vector.Y - baseRowY) / Math.Max(1, singleLine));
                float caretY = textRenderer.GetCurrentLineLayoutTopY(lineIndex) + visualRow * singleLine + textRenderer.TopInset;

                // If the caret is already visible within the viewport bounds, don't scroll at all!
                if (caretY >= 0 && caretY + singleLine <= canvasHeight)
                {
                    if (update)
                    {
                        if (coreTextbox.selectionManager.HasSelection)
                            canvasHelper.UpdateSelection();
                        canvasHelper.UpdateCursor();
                    }
                    return;
                }

                if (caretY + singleLine > canvasHeight)
                {
                    // Caret is below viewport bottom: scroll down just enough to reveal it
                    OffsetSource.VerticalOffset += (caretY + singleLine - canvasHeight);
                    if (update) canvasHelper.UpdateAll();
                    return;
                }
                else if (caretY < 0)
                {
                    // Caret is above viewport top: scroll up just enough to reveal it
                    OffsetSource.VerticalOffset += caretY;
                    if (update) canvasHelper.UpdateAll();
                    return;
                }
            }
        }

        // Fallback: cursor is outside the currently rendered slice/layout, compute target offset from estimated visual row
        int lineVisualStart = textRenderer.GetLineVisualStartRow(lineIndex);
        int withinLineRow = 0;
        if (textRenderer.ShouldVirtualizeWrappedLine(lineIndex))
        {
            int charsPerRow = textRenderer.EstimateWrappedCharsPerRow(coreTextbox.canvasText);
            withinLineRow = charsPerRow > 0 ? cursorManager.CharacterPosition / charsPerRow : 0;
        }
        int cursorVisualRow = lineVisualStart + withinLineRow;

        int startVR = (int)Math.Floor(OffsetSource.VerticalOffset / Math.Max(1, singleLine));
        int visibleRows = Math.Max(1, (int)(canvasHeight / singleLine));

        if (cursorVisualRow >= startVR + visibleRows)
        {
            // Scroll down: place cursor visual row at the bottom of the viewport
            OffsetSource.VerticalOffset = (cursorVisualRow - visibleRows + 1) * singleLine;
        }
        else if (cursorVisualRow < startVR)
        {
            // Scroll up: place cursor visual row at the top of the viewport
            OffsetSource.VerticalOffset = cursorVisualRow * singleLine;
        }

        if (update)
            canvasHelper.UpdateAll();
    }

    public bool ScrollIntoViewHorizontal(CanvasControl canvasText, bool update = true)
    {
        if (coreTextbox.WordWrap)
            return false;

        float viewportWidth = canvasText != null && canvasText.ActualWidth > 10
            ? (float)canvasText.ActualWidth
            : (coreTextbox != null && coreTextbox.ActualWidth > 10 ? (float)coreTextbox.ActualWidth : 800f);

        textRenderer.UpdateCurrentLineTextLayout(canvasText);
        float curPosInLine = GetCurrentCursorPixelPositionInLine();

        if (curPosInLine == OldHorizontalScrollValue)
            return false;

        double visibleStart = OffsetSource.HorizontalOffset;
        double visibleEnd = visibleStart + viewportWidth;

        bool changed = false;
        if (curPosInLine < visibleStart + 3)
        {
            changed = true;
            OffsetSource.HorizontalOffset = Math.Max(curPosInLine - 3, horizontalScrollBar.Minimum);
        }
        else if (curPosInLine > visibleEnd)
        {
            changed = true;
            OffsetSource.HorizontalOffset = Math.Min(curPosInLine - viewportWidth + 5, horizontalScrollBar.Maximum + 5);
        }

        OldHorizontalScrollValue = curPosInLine;

        if (update)
            canvasHelper.UpdateAll();

        return changed;
    }

    /// <summary>
    /// Horizontally CENTERS the current cursor column in the viewport (NoWrap only). Unlike
    /// <see cref="ScrollIntoViewHorizontal"/> — which reveals the column at the nearest edge (so a match far
    /// to the right lands pinned against the right edge) — this places the column in the middle of the
    /// visible width, which is what "jump to a search match" wants. It records the resolved column in
    /// <see cref="OldHorizontalScrollValue"/> so the per-draw <see cref="EnsureHorizontalScrollBounds"/> →
    /// <see cref="ScrollIntoViewHorizontal"/> pass sees the column as already handled and keeps the centered
    /// offset instead of re-revealing it at the edge.
    /// </summary>
    public bool ScrollCursorIntoViewHorizontallyCentered(CanvasControl canvasText, bool update = true)
    {
        if (coreTextbox.WordWrap)
        {
            if (OffsetSource.HorizontalOffset != 0)
                OffsetSource.HorizontalOffset = 0;
            OldHorizontalScrollValue = 0;
            return false;
        }

        float viewportWidth = canvasText != null && canvasText.ActualWidth > 10
            ? (float)canvasText.ActualWidth
            : (coreTextbox != null && coreTextbox.ActualWidth > 10 ? (float)coreTextbox.ActualWidth : 800f);

        textRenderer.UpdateCurrentLineTextLayout(canvasText);
        float curPosInLine = GetCurrentCursorPixelPositionInLine();

        double maxOffset = Math.Max(horizontalScrollBar.Minimum, horizontalScrollBar.Maximum);
        double target = Math.Clamp(curPosInLine - viewportWidth / 2, horizontalScrollBar.Minimum, maxOffset);

        bool changed = Math.Abs(target - OffsetSource.HorizontalOffset) > 0.5;
        if (changed)
            OffsetSource.HorizontalOffset = target;

        OldHorizontalScrollValue = curPosInLine;

        if (update)
            canvasHelper.UpdateAll();

        return changed;
    }

    /// <summary>
    /// Pixel X position of the current cursor column within its line, accounting for horizontal
    /// virtualization (long lines rendered as a moving slice). Shared by the edge-reveal
    /// <see cref="ScrollIntoViewHorizontal"/> and the centering
    /// <see cref="ScrollCursorIntoViewHorizontallyCentered"/> so both agree on where the column is.
    /// </summary>
    private float GetCurrentCursorPixelPositionInLine()
    {
        int charPosForLayout = cursorManager.currentCursorPosition.CharacterPosition;
        if (textRenderer.IsHorizontallyVirtualized)
            charPosForLayout -= textRenderer.HorizontalSliceStart;

        if (textRenderer.IsHorizontallyVirtualized && (charPosForLayout < 0 || textRenderer.CurrentLineTextLayout == null))
        {
            // Cursor is before the slice or the layout is unavailable; estimate the absolute position.
            return cursorManager.currentCursorPosition.CharacterPosition * textRenderer.CachedCharWidth;
        }

        if (textRenderer.IsHorizontallyVirtualized && charPosForLayout > textRenderer.RenderedText.Length)
        {
            // Cursor is beyond the slice; estimate the absolute position.
            return cursorManager.currentCursorPosition.CharacterPosition * textRenderer.CachedCharWidth;
        }

        if (textRenderer.CurrentLineTextLayout == null)
        {
            return cursorManager.currentCursorPosition.CharacterPosition * textRenderer.CachedCharWidth;
        }

        float pos = CursorHelper.GetCursorPositionInLine(
            textRenderer.CurrentLineTextLayout,
            new CursorPosition(charPosForLayout, cursorManager.currentCursorPosition.LineNumber),
            textRenderer.HorizontalSlicePixelOffset
        );

        if (pos <= 0 && cursorManager.currentCursorPosition.CharacterPosition > 0)
        {
            return cursorManager.currentCursorPosition.CharacterPosition * textRenderer.CachedCharWidth;
        }

        return pos;
    }

    public void EnsureHorizontalScrollBounds(CanvasControl canvasText, LongestLineManager longestLineManager, bool triggeredByCursor, bool forceRecalculateLongestLine = false)
    {
        if (textRenderer.IsWordWrapEnabled)
        {
            horizontalScrollBar.ViewportSize = canvasText.ActualWidth;
            horizontalScrollBar.Maximum = 0;
            horizontalScrollBar.Value = 0;
            ScrollBarExpansionHelper.HideIndicator(horizontalScrollBar);
            return;
        }

        longestLineManager.CheckRecalculateLongestLine(forceRecalculateLongestLine);

        //Apply longest width to scrollbar
        float viewportWidth = canvasText != null && canvasText.ActualWidth > 10
            ? (float)canvasText.ActualWidth
            : (coreTextbox != null && coreTextbox.ActualWidth > 10 ? (float)coreTextbox.ActualWidth : 800f);
        horizontalScrollBar.ViewportSize = viewportWidth;
        double maxScroll = longestLineManager.longestLineWidth.Width <= viewportWidth ? 0 : longestLineManager.longestLineWidth.Width - viewportWidth + (zoomManager.ZoomedFontSize / 2);
        horizontalScrollBar.Maximum = maxScroll;

        if (maxScroll > 0)
        {
            ScrollBarExpansionHelper.ShowIndicator(horizontalScrollBar);
        }
        else
        {
            ScrollBarExpansionHelper.HideIndicator(horizontalScrollBar);
        }

        if (OffsetSource.HorizontalOffset > maxScroll)
        {
            OffsetSource.HorizontalOffset = maxScroll;
        }

        if(ScrollIntoViewHorizontal(canvasText, false))
        {
            if (triggeredByCursor)
                canvasHelper.UpdateText();
            else
                canvasHelper.UpdateCursor();
        }
    }
}
