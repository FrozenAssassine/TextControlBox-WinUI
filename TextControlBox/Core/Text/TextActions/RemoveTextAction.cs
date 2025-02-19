
namespace TextControlBoxNS.Core.Text.TextActions;

internal class RemoveTextAction
{
    private TextManager textManager;
    private UndoRedo undoRedo;
    private CurrentLineManager currentLineManager;
    private LongestLineManager longestLineManager;
    private CursorManager cursorManager;
    public void Init(
        TextManager textManager,
        UndoRedo undoRedo,
        CurrentLineManager currentLineManager,
        LongestLineManager longestLineManager,
        CursorManager cursorManager
        )
    {
        this.textManager = textManager;
        this.undoRedo = undoRedo;
        this.currentLineManager = currentLineManager;
        this.longestLineManager = longestLineManager;
        this.cursorManager = cursorManager;
    }

    public void HandleTextRemoval(bool controlIsPressed)
    {
        string curLine = currentLineManager.CurrentLine;
        var charPos = cursorManager.GetCurPosInLine();
        var stepsToMove = controlIsPressed ? cursorManager.CalculateStepsToMoveLeft(charPos, controlIsPressed) : 1;

        if (charPos - stepsToMove >= 0)
        {
            RemoveCharacterFromCurrentLine(charPos, stepsToMove);
        }
        else if (charPos - stepsToMove < 0)
        {
            RemoveLineAbove(curLine);
        }
    }

    public void RemoveCharacterFromCurrentLine(int charPos, int stepsToMove)
    {
        if (cursorManager.LineNumber == longestLineManager.longestIndex)
            longestLineManager.needsRecalculation = true;

        undoRedo.RecordUndoAction(() =>
        {
            currentLineManager.SafeRemove(charPos - stepsToMove, stepsToMove);
            cursorManager.CharacterPosition -= stepsToMove;

        }, cursorManager.LineNumber, 1, 1);
    }

    public void RemoveLineAbove(string curLine)
    {
        if (cursorManager.LineNumber <= 0)
            return;

        if (cursorManager.LineNumber == longestLineManager.longestIndex)
            longestLineManager.needsRecalculation = true;

        undoRedo.RecordUndoAction(() =>
        {
            int curpos = textManager.GetLineLength(cursorManager.LineNumber - 1);

            if (curLine.Length > 0)
                textManager.String_AddToEnd(cursorManager.LineNumber - 1, curLine);

            textManager.DeleteAt(cursorManager.LineNumber);

            cursorManager.LineNumber -= 1;
            cursorManager.CharacterPosition = curpos;

        }, cursorManager.LineNumber - 1, 3, 2);
    }
}
