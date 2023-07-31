using Collections.Pooled;

namespace TextControlBoxNS.Text
{
    internal class MoveLine
    {
        public static void Move(PooledList<string> totalLines, TextSelection selection, CursorPosition cursorposition, UndoRedo undoredo, string newLineCharacter, LineMoveDirection direction)
        {
            if (selection != null)
                return;

            //move down:
            if (direction == LineMoveDirection.Down)
            {
                if (cursorposition.LineNumber >= totalLines.Count - 1)
                    return;

                undoredo.RecordUndoAction(() =>
                {
                    Selection.MoveLinesDown(totalLines, selection, cursorposition);
                }, totalLines, cursorposition.LineNumber, 2, 2, newLineCharacter, cursorposition);
                return;
            }

            //move up:
            if (cursorposition.LineNumber <= 0)
                return;

            undoredo.RecordUndoAction(() =>
            {
                Selection.MoveLinesUp(totalLines, selection, cursorposition);
            }, totalLines, cursorposition.LineNumber - 1, 2, 2, newLineCharacter, cursorposition);
        }
    }
}
