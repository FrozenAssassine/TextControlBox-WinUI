using Microsoft.UI.Input;

namespace TextControlBoxNS.Text
{
    internal class CanvasUpdateManager
    {
        private CoreTextControlBox coreTextbox;

        public void Init(CoreTextControlBox coreTextbox)
        {
            this.coreTextbox = coreTextbox;
        }

        public void UpdateCursor()
        {
            coreTextbox.canvasCursor.Invalidate();
        }
        public void UpdateText()
        {
            coreTextbox.ChangeCursor(InputSystemCursorShape.IBeam);
            coreTextbox.canvasText.Invalidate();
        }
        public void UpdateSelection()
        {
            coreTextbox.canvasSelection.Invalidate();
        }

        public void UpdateLineNumbers()
        {
            coreTextbox.canvasLineNumber.Invalidate();
        }

        public void UpdateAll()
        {
            UpdateText();
            UpdateSelection();
            UpdateCursor();
        }
    }
}
