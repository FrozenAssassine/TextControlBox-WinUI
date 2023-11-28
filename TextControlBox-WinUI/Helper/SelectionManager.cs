using TextControlBox.Text;

namespace TextControlBox_WinUI.Helper
{
    internal class SelectionManager
    {
        public bool HasSelection => Selection.HasSelection();
        public bool IsSelecting { get; set; }
        public TextSelection Selection = new TextSelection(0,0, new CursorPosition(0,0), new CursorPosition(0,0));
        public CursorPosition Cursor { get; set; } = new CursorPosition(0,0);
        public bool IsSelectingOverLinenumbers { get; set; }

        public CursorPosition GetCursorPos()
        {
            return Cursor;
        }
        public void ClearSelection()
        {
            Selection.StartPosition = new CursorPosition(Cursor);
            Selection.EndPosition = new CursorPosition(Cursor);
        }

        public void SetSelection(TextSelection selection)
        {
            if (selection.HasSelection())
                return;

            SetSelection(selection.StartPosition, selection.EndPosition);
        }
        public void SetSelection(CursorPosition startPosition, CursorPosition endPosition)
        {
            Selection.StartPosition = startPosition;
            Selection.EndPosition = endPosition;
        }
        public void SetSelectionStart(CursorPosition startPosition)
        {
            Selection.StartPosition = startPosition;
        }
        public void SetSelectionEnd(CursorPosition endPosition)
        {
            Selection.EndPosition = endPosition;
        }
    }
}
