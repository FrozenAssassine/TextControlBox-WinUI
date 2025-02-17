using TextControlBoxNS.Core.Selection;
using TextControlBoxNS.Models.Enums;

namespace TextControlBoxNS.Core.Text;

internal class MoveLineManager
{
    private SelectionManager selectionManager;
    private CursorManager cursorManager;
    private TextManager textManager;
    private UndoRedo undoRedo;

    public void Init(SelectionManager selectionManager, CursorManager cursorManager, TextManager textManager, UndoRedo undoRedo)
    {
        this.selectionManager = selectionManager;
        this.cursorManager = cursorManager;
        this.textManager = textManager;
        this.undoRedo = undoRedo;
    }

    public bool Move(LineMoveDirection direction)
    {
        //Move single line
        if (selectionManager.HasSelection)
            return false;
        bool res = false;
        //move down:
        if (direction == LineMoveDirection.Down)
        {
            if (cursorManager.LineNumber >= textManager.LinesCount - 1)
                return false;

            undoRedo.RecordUndoAction(() =>
            {
                res = MoveLinesDown();
            }, cursorManager.LineNumber, 2, 2);
            return res;
        }

        //move up:
        if (cursorManager.LineNumber <= 0)
            return false;

        undoRedo.RecordUndoAction(() =>
        {
            res = MoveLinesUp();
        }, cursorManager.LineNumber - 1, 2, 2);

        return res;
    }

    private bool MoveLinesUp()
    {
        bool res = textManager.SwapLines(cursorManager.LineNumber, cursorManager.LineNumber - 1);
        if (res)
            cursorManager.LineNumber -= 1;
        return res;
    }
    private bool MoveLinesDown()
    {
        bool res = textManager.SwapLines(cursorManager.LineNumber, cursorManager.LineNumber + 1);
        if (res)
            cursorManager.LineNumber += 1;
        return res;
    }
}
