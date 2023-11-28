using TextControlBox_WinUI.Helper;

namespace TextControlBox.Text
{
    internal class MoveLine
    {
        public static TextSelection Move(TextManager textManager, TextSelection selection, CursorPosition cursorposition, UndoRedoManager undoredo, MoveDirection direction)
        {
            TextSelection result = null;
            if (direction == MoveDirection.Down)
            {
                if (selection == null)
                {
                    if (cursorposition.LineNumber >= textManager.Lines.Count - 1)
                        return null;

                    undoredo.RecordUndoAction(() =>
                    {
                        Selection.MoveLinesDown(textManager, selection, cursorposition);

                    }, cursorposition.LineNumber, 2, 2, cursorposition);
                    return result;
                }
            }
            else
            {
                if (selection == null)
                {
                    if (cursorposition.LineNumber <= 0)
                        return null;

                    undoredo.RecordUndoAction(() =>
                    {
                        Selection.MoveLinesUp(textManager, selection, cursorposition);

                    }, cursorposition.LineNumber - 1, 2, 2, cursorposition);
                    return result;
                }
            }
            return null;
        }
    }
    internal enum MoveDirection
    {
        Up, Down
    }
}
