using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI.Core;

namespace TextControlBoxNS.Helper
{
    internal class CanvasHelper
    {
        public CanvasControl Canvas_Selection = null;
        public CanvasControl Canvas_Linenumber = null;
        public CanvasControl Canvas_Text = null;
        public CanvasControl Canvas_Cursor = null;

        public CanvasHelper(CanvasControl canvas_selection, CanvasControl canvas_linenumber, CanvasControl canvas_text, CanvasControl canvas_cursor)
        {
            this.Canvas_Text = canvas_text;
            this.Canvas_Linenumber = canvas_linenumber;
            this.Canvas_Cursor = canvas_cursor;
            this.Canvas_Selection = canvas_selection;
        }

        public void UpdateCursor()
        {
            Canvas_Cursor.Invalidate();
        }
        public void UpdateText()
        {
            //TODO:  Utils.ChangeCursor(CoreCursorType.IBeam);
            Canvas_Text.Invalidate();
        }
        public void UpdateSelection()
        {
            Canvas_Selection.Invalidate();
        }
        public void UpdateAll()
        {
            UpdateText();
            UpdateSelection();
            UpdateCursor();
        }
    }
}
