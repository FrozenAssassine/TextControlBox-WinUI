using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using TextControlBox.Text;

namespace TextControlBox_WinUI.Helper
{
    internal class CanvasUpdateHelper
    {
        private CanvasControl Canvas_Text;
        private CanvasControl Canvas_Selection;
        private CanvasControl Canvas_Cursor;
        private CanvasControl Canvas_LineNumber;
        public CanvasUpdateHelper(CanvasControl canvas_text, CanvasControl canvas_selection, CanvasControl canvas_cursor, CanvasControl canvas_lineNumber) 
        {
            this.Canvas_Text = canvas_text;
            this.Canvas_Selection = canvas_selection;
            this.Canvas_Cursor = canvas_cursor;
            this.Canvas_LineNumber = canvas_lineNumber;
        }
        public void UpdateAll()
        {
            Canvas_Cursor.Invalidate();
            Canvas_Selection.Invalidate();
            Canvas_Text.Invalidate();
        }
        public CanvasUpdateHelper UpdateText()
        {
            Canvas_Text.Invalidate();
            return this;
        }
        public CanvasUpdateHelper UpdateSelection()
        {
            Canvas_Selection.Invalidate();
            return this;
        }
        public CanvasUpdateHelper UpdateCursor()
        {
            Canvas_Cursor.Invalidate();
            return this;
        }
    }
}
