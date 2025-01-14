
using TextControlBoxNS.Models;
using TextControlBoxNS.Models.Enums;

namespace TextControlBoxNS.Core.Text;

internal class MoveLine
{
    public static void Move(SelectionManager selectionManager, TextManager textManager, TextSelection selection, CursorPosition cursorposition, UndoRedo undoredo, LineMoveDirection direction)
    {
        if (selection != null)
            return;

        //move down:
        if (direction == LineMoveDirection.Down)
        {
            if (cursorposition.LineNumber >= textManager.LinesCount - 1)
                return;

            undoredo.RecordUndoAction(() =>
            {
                selectionManager.MoveLinesDown(selection, cursorposition);
            }, cursorposition.LineNumber, 2, 2, cursorposition);
            return;
        }

        //move up:
        if (cursorposition.LineNumber <= 0)
            return;

        undoredo.RecordUndoAction(() =>
        {
            selectionManager.MoveLinesUp(selection, cursorposition);
        }, cursorposition.LineNumber - 1, 2, 2, cursorposition);
    }
}
