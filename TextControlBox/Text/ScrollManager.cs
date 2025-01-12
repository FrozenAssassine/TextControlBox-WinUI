
using Microsoft.UI.Xaml.Controls.Primitives;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Renderer;

namespace TextControlBoxNS.Text;

internal class ScrollManager
{
    private readonly ScrollBar verticalScrollBar;
    private readonly ScrollBar horizontalScrollBar;
    private readonly CanvasHelper canvasHelper;
    private readonly TextRenderer textRenderer;
    private readonly CursorManager cursorManager;
    private readonly TextManager textManager;

    public ScrollManager(CanvasHelper canvasHelper, TextManager textManager, TextRenderer textRenderer, CursorManager cursorManager, ScrollBar verticalScrollBar, ScrollBar horizontalScrollBar)
    {
        this.verticalScrollBar = verticalScrollBar;
        this.horizontalScrollBar = horizontalScrollBar;
        this.canvasHelper = canvasHelper;
        this.textRenderer = textRenderer;
        this.cursorManager = cursorManager;
        this.textManager = textManager;
    }

    public double _HorizontalScrollSensitivity = 1;
    public double _VerticalScrollSensitivity = 1;
    public int DefaultVerticalScrollSensitivity = 4;
    public float OldHorizontalScrollValue = 0;
    public void ScrollLineToCenter(int line)
    {
        if (textRenderer.OutOfRenderedArea(line))
            ScrollLineIntoView(line);
    }


    public void ScrollOneLineUp()
    {
        verticalScrollBar.Value -= textRenderer.SingleLineHeight / DefaultVerticalScrollSensitivity;
        canvasHelper.UpdateAll();
    }
    public void ScrollOneLineDown()
    {
        verticalScrollBar.Value += textRenderer.SingleLineHeight / DefaultVerticalScrollSensitivity;
        canvasHelper.UpdateAll();
    }

    public void ScrollLineIntoView(int line)
    {
        verticalScrollBar.Value = (line - textRenderer.NumberOfRenderedLines / 2) * textRenderer.SingleLineHeight / DefaultVerticalScrollSensitivity;
        canvasHelper.UpdateAll();
    }

    public void ScrollTopIntoView()
    {
        verticalScrollBar.Value = (cursorManager.LineNumber - 1) * textRenderer.SingleLineHeight / DefaultVerticalScrollSensitivity;
        canvasHelper.UpdateAll();
    }
    public void ScrollBottomIntoView()
    {
        verticalScrollBar.Value = (cursorManager.LineNumber - textRenderer.NumberOfRenderedLines + 1) * textRenderer.SingleLineHeight / DefaultVerticalScrollSensitivity;
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
        if (cursorManager.LineNumber > textManager.LinesCount- 1)
            cursorManager.LineNumber = textManager.LinesCount - 1;
        verticalScrollBar.Value += textRenderer.NumberOfRenderedLines * textRenderer.SingleLineHeight / DefaultVerticalScrollSensitivity;
        canvasHelper.UpdateAll();
    }


}
