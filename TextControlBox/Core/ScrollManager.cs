using Microsoft.UI.Dispatching;
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

    public double VerticalScroll { get => OffsetSource.VerticalOffset / DefaultVerticalScrollSensitivity; set { CancelSmoothScroll(); zoomManager.ResetZoomAnchors(); OffsetSource.VerticalOffset = (value < 0 ? 0 : value) * DefaultVerticalScrollSensitivity; canvasHelper.UpdateAll(); } }
    public double HorizontalScroll { get => OffsetSource.HorizontalOffset; set { CancelSmoothScroll(); zoomManager.ResetZoomAnchors(); OffsetSource.HorizontalOffset = value < 0 ? 0 : value; canvasHelper.UpdateAll(); } }

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
        zoomManager.ResetZoomAnchors();
        CancelSmoothScroll();
        canvasHelper.UpdateAll();
    }

    internal void HorizontalScrollBar_Scroll(object sender, ScrollEventArgs e)
    {
        zoomManager.ResetZoomAnchors();
        CancelSmoothScroll();
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
        CancelSmoothScroll();
        zoomManager.ZoomAnchorLine = null;
        OffsetSource.VerticalOffset -= textRenderer.SingleLineHeight;
        if(update)
            canvasHelper.UpdateAll();
    }
    public void ScrollOneLineDown(bool update = true)
    {
        CancelSmoothScroll();
        zoomManager.ZoomAnchorLine = null;
        OffsetSource.VerticalOffset += textRenderer.SingleLineHeight;
        if(update)
            canvasHelper.UpdateAll();
    }

    public void ScrollLineIntoView(int line, bool update = true)
    {
        CancelSmoothScroll();
        zoomManager.ZoomAnchorLine = null;
        OffsetSource.VerticalOffset = (line - textRenderer.NumberOfRenderedLines / 2) * textRenderer.SingleLineHeight;
        
        if(update)
            canvasHelper.UpdateAll();
    }

    public void ScrollTopIntoView(bool update = true)
    {
        CancelSmoothScroll();
        zoomManager.ZoomAnchorLine = null;
        OffsetSource.VerticalOffset = (cursorManager.LineNumber - 1) * textRenderer.SingleLineHeight;
        if(update)
            canvasHelper.UpdateAll();
    }
    public void ScrollBottomIntoView(bool update = true)
    {
        CancelSmoothScroll();
        zoomManager.ZoomAnchorLine = null;
        OffsetSource.VerticalOffset = (cursorManager.LineNumber - textRenderer.NumberOfRenderedLines + 1) * textRenderer.SingleLineHeight;
        if(update)
            canvasHelper.UpdateAll();
    }

    public void ScrollPageUp()
    {
        CancelSmoothScroll();
        zoomManager.ZoomAnchorLine = null;
        if (!cursorManager.PreferredCharacterPosition.HasValue)
            cursorManager.PreferredCharacterPosition = cursorManager.CharacterPosition;

        int linesToScroll = Math.Max(1, textRenderer.NumberOfRenderedLines);
        cursorManager.LineNumber -= linesToScroll;
        if (cursorManager.LineNumber < 0)
            cursorManager.LineNumber = 0;

        cursorManager.CharacterPosition = Math.Clamp(cursorManager.PreferredCharacterPosition.Value, 0, textManager.GetLineLength(cursorManager.LineNumber));
        OffsetSource.VerticalOffset -= linesToScroll * textRenderer.SingleLineHeight;
        canvasHelper.UpdateAll();
    }


    public void ScrollPageDown()
    {
        CancelSmoothScroll();
        zoomManager.ZoomAnchorLine = null;
        if (!cursorManager.PreferredCharacterPosition.HasValue)
            cursorManager.PreferredCharacterPosition = cursorManager.CharacterPosition;

        int linesToScroll = Math.Max(1, textRenderer.NumberOfRenderedLines);
        cursorManager.LineNumber += linesToScroll;
        if (cursorManager.LineNumber > textManager.LinesCount - 1)
            cursorManager.LineNumber = textManager.LinesCount - 1;

        cursorManager.CharacterPosition = Math.Clamp(cursorManager.PreferredCharacterPosition.Value, 0, textManager.GetLineLength(cursorManager.LineNumber));
        OffsetSource.VerticalOffset += linesToScroll * textRenderer.SingleLineHeight;
        canvasHelper.UpdateAll();
    }

    public void UpdateScrollToShowCursor(bool update = true)
    {
        CancelSmoothScroll();
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
        else if (textRenderer.NumberOfStartLine > cursorManager.LineNumber ||
                 (textRenderer.NumberOfStartLine == cursorManager.LineNumber && textRenderer.VerticalSubLineOffset > 0))
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
                        if (coreTextbox.selectionManager.HasSelection || coreTextbox.selectionRenderer.renderedSelectionLength > 0 || !coreTextbox.selectionManager.OldTextSelection.StartPosition.IsNull)
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

    public bool SmoothScrolling { get; set; } = true;

    private DispatcherQueueTimer _smoothScrollTimer;
    private double _targetVerticalOffset;
    private double _targetHorizontalOffset;
    private bool _isSmoothScrollingVertical = false;
    private bool _isSmoothScrollingHorizontal = false;

    private void EnsureSmoothScrollTimer()
    {
        if (_smoothScrollTimer == null && coreTextbox?.DispatcherQueue != null)
        {
            _smoothScrollTimer = coreTextbox.DispatcherQueue.CreateTimer();
            _smoothScrollTimer.Interval = TimeSpan.FromMilliseconds(16);
            _smoothScrollTimer.Tick += OnSmoothScrollTimerTick;
        }

        if (_smoothScrollTimer != null && !_smoothScrollTimer.IsRunning)
        {
            _smoothScrollTimer.Start();
        }
    }

    private void OnSmoothScrollTimerTick(DispatcherQueueTimer sender, object args)
    {
        bool stillScrolling = false;

        if (_isSmoothScrollingVertical)
        {
            double currentV = OffsetSource.VerticalOffset;
            double diffV = _targetVerticalOffset - currentV;
            if (Math.Abs(diffV) <= 1.0)
            {
                OffsetSource.VerticalOffset = _targetVerticalOffset;
                _isSmoothScrollingVertical = false;
            }
            else
            {
                double stepV = diffV * 0.6;
                if (Math.Abs(stepV) < 1.0)
                    stepV = Math.Sign(diffV) * 1.0;

                OffsetSource.VerticalOffset = currentV + stepV;
                stillScrolling = true;
            }
        }

        if (_isSmoothScrollingHorizontal)
        {
            double currentH = OffsetSource.HorizontalOffset;
            double diffH = _targetHorizontalOffset - currentH;
            if (Math.Abs(diffH) <= 1.0)
            {
                OffsetSource.HorizontalOffset = _targetHorizontalOffset;
                _isSmoothScrollingHorizontal = false;
            }
            else
            {
                double stepH = diffH * 0.6;
                if (Math.Abs(stepH) < 1.0)
                    stepH = Math.Sign(diffH) * 1.0;

                OffsetSource.HorizontalOffset = currentH + stepH;
                stillScrolling = true;
            }
        }

        canvasHelper.UpdateAll();

        if (!stillScrolling)
        {
            _smoothScrollTimer?.Stop();
        }
    }

    public void CancelSmoothScroll()
    {
        _isSmoothScrollingVertical = false;
        _isSmoothScrollingHorizontal = false;
        if (_smoothScrollTimer != null && _smoothScrollTimer.IsRunning)
        {
            _smoothScrollTimer.Stop();
        }
    }

    public void SmoothScrollVerticalBy(double deltaPixels)
    {
        if (verticalScrollBar == null)
            return;

        double maxOffset = Math.Max(0, verticalScrollBar.Maximum * DefaultVerticalScrollSensitivity);

        if (!SmoothScrolling)
        {
            CancelSmoothScroll();
            OffsetSource.VerticalOffset = Math.Clamp(OffsetSource.VerticalOffset + deltaPixels, 0, maxOffset);
            canvasHelper.UpdateAll();
            return;
        }

        if (!_isSmoothScrollingVertical)
        {
            _targetVerticalOffset = OffsetSource.VerticalOffset;
            _isSmoothScrollingVertical = true;
        }

        _targetVerticalOffset = Math.Clamp(_targetVerticalOffset + deltaPixels, 0, maxOffset);
        EnsureSmoothScrollTimer();
    }

    public void SmoothScrollHorizontalBy(double deltaPixels)
    {
        if (horizontalScrollBar == null)
            return;

        double maxOffset = Math.Max(0, horizontalScrollBar.Maximum);

        if (!SmoothScrolling)
        {
            CancelSmoothScroll();
            OffsetSource.HorizontalOffset = Math.Clamp(OffsetSource.HorizontalOffset + deltaPixels, 0, maxOffset);
            canvasHelper.UpdateAll();
            return;
        }

        if (!_isSmoothScrollingHorizontal)
        {
            _targetHorizontalOffset = OffsetSource.HorizontalOffset;
            _isSmoothScrollingHorizontal = true;
        }

        _targetHorizontalOffset = Math.Clamp(_targetHorizontalOffset + deltaPixels, 0, maxOffset);
        EnsureSmoothScrollTimer();
    }
}
