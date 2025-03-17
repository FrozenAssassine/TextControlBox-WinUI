namespace TextControlBoxNS.Core.Text.TextActions;

internal class DeleteTextAction
{
    private TextManager textManager;
    private CoreTextControlBox coreTextbox;
    private UndoRedo undoRedo;
    private CurrentLineManager currentLineManager;
    private LongestLineManager longestLineManager;
    private CursorManager cursorManager;
    public void Init(
        TextManager textManager,
        CoreTextControlBox coreTextbox,
        UndoRedo undoRedo,
        CurrentLineManager currentLineManager,
        LongestLineManager longestLineManager,
        CursorManager cursorManager
        )
    {
        this.textManager = textManager;
        this.coreTextbox = coreTextbox;
        this.undoRedo = undoRedo;
        this.currentLineManager = currentLineManager;
        this.longestLineManager = longestLineManager;
        this.cursorManager = cursorManager;
    }

    public void DeleteCurrentLine()
    {
        //Do not delete empty line
        if (cursorManager.LineNumber == 0 && textManager.GetLineLength(cursorManager.LineNumber) == 0 && textManager.LinesCount == 1)
            return;

        coreTextbox.DeleteLine(cursorManager.LineNumber);
    }

    public void DeleteTextInLine(bool controlIsPressed)
    {
        int characterPos = cursorManager.GetCurPosInLine();

        if (characterPos == currentLineManager.Length)
        {
            MergeNextLine();
        }
        else if (textManager.LinesCount > cursorManager.LineNumber)
        {
            RemoveTextInLine(controlIsPressed);
        }
    }

    public void MergeNextLine()
    {
        string lineToAdd = cursorManager.LineNumber + 1 < textManager.LinesCount ? textManager.GetLineText(cursorManager.LineNumber + 1) : null;

        if (lineToAdd != null)
        {
            if (cursorManager.LineNumber == longestLineManager.longestIndex)
                longestLineManager.needsRecalculation = true;

            undoRedo.RecordUndoAction(() =>
            {
                int curPos = textManager.GetLineLength(cursorManager.LineNumber);
                currentLineManager.AddToEnd(lineToAdd);
                textManager.DeleteAt(cursorManager.LineNumber + 1);

                cursorManager.CharacterPosition = curPos;
            }, cursorManager.LineNumber, 2, 1);
        }
    }

    public void RemoveTextInLine(bool controlIsPressed)
    {
        int characterPos = cursorManager.GetCurPosInLine();
        int stepsToMove = controlIsPressed ? cursorManager.CalculateStepsToMoveRight(characterPos) : 1;

        if (cursorManager.LineNumber == longestLineManager.longestIndex)
            longestLineManager.needsRecalculation = true;

        undoRedo.RecordUndoAction(() =>
        {
            currentLineManager.SafeRemove(characterPos, stepsToMove);
        }, cursorManager.LineNumber, 1, 1);
    }
}
