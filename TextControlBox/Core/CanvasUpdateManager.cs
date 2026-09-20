using Microsoft.UI.Input;
using TextControlBoxNS.Core.Renderer;

namespace TextControlBoxNS.Core;

internal class CanvasUpdateManager
{
    private CoreTextControlBox coreTextbox;
    private readonly CanvasBatchRedrawer _batchRedrawer;

    public CanvasUpdateManager()
    {
        _batchRedrawer = new CanvasBatchRedrawer(16); //16ms = 60FPS
    }

    public void Init(CoreTextControlBox coreTextbox)
    {
        this.coreTextbox = coreTextbox;
    }

    public void UpdateCursor()
    {
        if (!coreTextbox.canvasCursor.ReadyToDraw)
            return;

        coreTextbox.caretBlinkManager?.NotifyCaretActivity();

        //coreTextbox.canvasCursor.Invalidate();
        _batchRedrawer.RequestRedraw(coreTextbox.canvasCursor);
    }

    // Redraw the caret canvas for a blink-phase toggle WITHOUT resetting the blink
    // phase. UpdateCursor resets the phase via NotifyCaretActivity, so the blink
    // timer must use this path or the caret would never blink.
    public void RedrawCursorForBlink()
    {
        if (!coreTextbox.canvasCursor.ReadyToDraw)
            return;

        _batchRedrawer.RequestRedraw(coreTextbox.canvasCursor);
    }
    public void UpdateText()
    {
        if (!coreTextbox.canvasText.ReadyToDraw)
            return;

        coreTextbox.ChangeCursor(InputSystemCursorShape.IBeam);
        //coreTextbox.canvasText.Invalidate();
        _batchRedrawer.RequestRedraw(coreTextbox.canvasText);
    }
    public void UpdateSelection()
    {
        if (!coreTextbox.canvasSelection.ReadyToDraw)
            return;

        //coreTextbox.canvasSelection.Invalidate();
        _batchRedrawer.RequestRedraw(coreTextbox.canvasSelection);
    }

    public void UpdateLineNumbers()
    {
        if (!coreTextbox.canvasLineNumber.ReadyToDraw)
            return;

        //coreTextbox.canvasLineNumber.Invalidate();
        _batchRedrawer.RequestRedraw(coreTextbox.canvasLineNumber);
    }

    public void UpdateAll()
    {
        UpdateLineNumbers();
        UpdateText();
        UpdateSelection();
        UpdateCursor();
    }
}
