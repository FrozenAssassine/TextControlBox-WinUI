using TextControlBox.Text;

namespace TextControlBox_WinUI.Helper
{
    internal class SelectionManager
    {
        public bool HasSelection => CheckHasSelection();
        public bool IsSelecting { get; set; }
        public TextSelection Selection = new TextSelection(0,0, new CursorPosition(0,0), null);
        public CursorPosition Cursor { get => GetCursor(); set => Selection.StartPosition = value; }
        public bool IsSelectingOverLinenumbers { get; set; }

        private CursorPosition GetCursor()
        {
            if (Selection.StartPosition == null && Selection.EndPosition != null)
                return Selection.EndPosition;
            else if(Selection.EndPosition == null && Selection.StartPosition != null)
                return Selection.StartPosition;
            return Selection.StartPosition = new CursorPosition(0, 0); //FAllback 
        }

        public CursorPosition GetCursorPos()
        {
            return Cursor;
        }
        public void ClearSelection()
        {
            Selection.StartPosition = Selection.EndPosition = null;
        }

        public void SetSelection(TextSelection selection)
        {
            if (selection == null)
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
        public bool CheckHasSelection()
        {
            if (Selection.StartPosition == null || Selection.EndPosition == null)
                return false;

            if (Selection.StartPosition.LineNumber == Selection.EndPosition.LineNumber)
                return Selection.StartPosition.CharacterPosition != Selection.EndPosition.CharacterPosition;
            return true;
        }
    }
}
