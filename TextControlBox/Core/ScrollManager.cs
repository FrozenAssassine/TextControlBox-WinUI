
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

    public double VerticalScroll { get => verticalScrollBar.Value; set { verticalScrollBar.Value = value < 0 ? 0 : value; canvasHelper.UpdateAll(); } }
    public double HorizontalScroll { get => horizontalScrollBar.Value; set { horizontalScrollBar.Value = value < 0 ? 0 : value; canvasHelper.UpdateAll(); } }

    public ScrollBar verticalScrollBar;
    public ScrollBar horizontalScrollBar;
    private CanvasUpdateManager canvasHelper;
    private TextRenderer textRenderer;
    private CursorManager cursorManager;
    private TextManager textManager;
    private CoreTextControlBox coreTextbox;
    private Grid scrollGrid;

    public void Init(CoreTextControlBox coreTextbox, CanvasUpdateManager canvasHelper, TextManager textManager, TextRenderer textRenderer, CursorManager cursorManager, ScrollBar verticalScrollBar, ScrollBar horizontalScrollBar)
    {
        this.verticalScrollBar = coreTextbox.verticalScrollBar;
        this.horizontalScrollBar = coreTextbox.horizontalScrollBar;
        scrollGrid = coreTextbox.scrollGrid;
        this.canvasHelper = canvasHelper;
        this.textRenderer = textRenderer;
        this.cursorManager = cursorManager;
        this.textManager = textManager;
        this.coreTextbox = coreTextbox;
        verticalScrollBar.Loaded += VerticalScrollbar_Loaded;
        verticalScrollBar.Scroll += VerticalScrollBar_Scroll;
        horizontalScrollBar.Scroll += HorizontalScrollBar_Scroll;
    }

    private void VerticalScrollbar_Loaded(object sender, RoutedEventArgs e)
    {
        verticalScrollBar.Maximum = ((textManager.LinesCount + 1) * textRenderer.SingleLineHeight - scrollGrid.ActualHeight) / DefaultVerticalScrollSensitivity;
        verticalScrollBar.ViewportSize = coreTextbox.ActualHeight;
    }
    private void HorizontalScrollBar_Scroll(object sender, ScrollEventArgs e)
    {
        canvasHelper.UpdateAll();
    }
    private void VerticalScrollBar_Scroll(object sender, ScrollEventArgs e)
    {
        //only update when a line was scrolled
        if ((int)(verticalScrollBar.Value / textRenderer.SingleLineHeight * DefaultVerticalScrollSensitivity) != textRenderer.NumberOfStartLine)
        {
            canvasHelper.UpdateAll();
        }
    }

    public void UpdateWhenScrolled()
    {
        //only update when a line was scrolled
        if ((int)(verticalScrollBar.Value / textRenderer.SingleLineHeight) != textRenderer.NumberOfStartLine)
        {
            canvasHelper.UpdateAll();
        }
    }

    public void ScrollLineIntoViewIfOutside(int line, bool update = true)
    {
        if (textRenderer.OutOfRenderedArea(line))
            ScrollLineIntoView(line, update);
    }

    public void ScrollOneLineUp(bool update = true)
    {
        verticalScrollBar.Value -= textRenderer.SingleLineHeight / DefaultVerticalScrollSensitivity;
        if(update)
            canvasHelper.UpdateAll();
    }
    public void ScrollOneLineDown(bool update = true)
    {
        verticalScrollBar.Value += textRenderer.SingleLineHeight / DefaultVerticalScrollSensitivity;
        if(update)
            canvasHelper.UpdateAll();
    }

    public void ScrollLineIntoView(int line, bool update = true)
    {
        verticalScrollBar.Value = (line - textRenderer.NumberOfRenderedLines / 2) * textRenderer.SingleLineHeight / DefaultVerticalScrollSensitivity;
        
        if(update)
            canvasHelper.UpdateAll();
    }

    public void ScrollTopIntoView(bool update = true)
    {
        verticalScrollBar.Value = (cursorManager.LineNumber - 1) * textRenderer.SingleLineHeight / DefaultVerticalScrollSensitivity;
        if(update)
            canvasHelper.UpdateAll();
    }
    public void ScrollBottomIntoView(bool update = true)
    {
        verticalScrollBar.Value = (cursorManager.LineNumber - textRenderer.NumberOfRenderedLines + 1) * textRenderer.SingleLineHeight / DefaultVerticalScrollSensitivity;
        if(update)
            canvasHelper.UpdateAll();
    }

    public void ScrollPageUp()
    {
        cursorManager.LineNumber -= textRenderer.NumberOfRenderedLines;
        if (cursorManager.LineNumber < 0)
            cursorManager.LineNumber = 0;

        verticalScrollBar.Value -= textRenderer.NumberOfRenderedLines * textRenderer.SingleLineHeight / DefaultVerticalScrollSensitivity;
        canvasHelper.UpdateAll();
    }


    public void ScrollPageDown()
    {
        cursorManager.LineNumber += textRenderer.NumberOfRenderedLines;
        if (cursorManager.LineNumber > textManager.LinesCount - 1)
            cursorManager.LineNumber = textManager.LinesCount - 1;
        verticalScrollBar.Value += textRenderer.NumberOfRenderedLines * textRenderer.SingleLineHeight / DefaultVerticalScrollSensitivity;
        canvasHelper.UpdateAll();
    }
    public void UpdateScrollToShowCursor(bool update = true)
    {
        if (textRenderer.NumberOfStartLine + textRenderer.NumberOfRenderedLines <= cursorManager.LineNumber)
            verticalScrollBar.Value = (cursorManager.LineNumber - textRenderer.NumberOfRenderedLines + 1) * textRenderer.SingleLineHeight / DefaultVerticalScrollSensitivity;
        else if (textRenderer.NumberOfStartLine > cursorManager.LineNumber)
            verticalScrollBar.Value = (cursorManager.LineNumber - 1) * textRenderer.SingleLineHeight / DefaultVerticalScrollSensitivity;

        if (update)
            canvasHelper.UpdateAll();
    }

    public void ScrollIntoViewHorizontal(CanvasControl canvasText, bool update = true)
    {
        float curPosInLine = CursorHelper.GetCursorPositionInLine(
            textRenderer.CurrentLineTextLayout,
            cursorManager.currentCursorPosition,
            0
        );

        if (curPosInLine == OldHorizontalScrollValue)
            return;

        double visibleStart = horizontalScrollBar.Value;
        double visibleEnd = visibleStart + canvasText.ActualWidth;

        if (curPosInLine < visibleStart + 3)
        {
            horizontalScrollBar.Value = Math.Max(curPosInLine - 3, horizontalScrollBar.Minimum);
        }
        else if (curPosInLine > visibleEnd - 3)
        {
            horizontalScrollBar.Value = Math.Min(curPosInLine - canvasText.ActualWidth + 3, horizontalScrollBar.Maximum);
        }
        OldHorizontalScrollValue = curPosInLine;

        if (update)
            canvasHelper.UpdateAll();
    }
}
