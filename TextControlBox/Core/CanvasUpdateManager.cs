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

        // Register the canvases
        _batchRedrawer.RegisterCanvas(coreTextbox.canvasCursor);
        _batchRedrawer.RegisterCanvas(coreTextbox.canvasText);
        _batchRedrawer.RegisterCanvas(coreTextbox.canvasSelection);
        _batchRedrawer.RegisterCanvas(coreTextbox.canvasLineNumber);
    }

    public void UpdateCursor()
    {
        if (!coreTextbox.canvasCursor.ReadyToDraw)
            return;

        //coreTextbox.canvasCursor.Invalidate();
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
        UpdateText();
        UpdateSelection();
        UpdateCursor();
    }
}
