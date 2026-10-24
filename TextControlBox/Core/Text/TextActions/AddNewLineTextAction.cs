using TextControlBoxNS.Core.Selection;
using TextControlBoxNS.Extensions;

namespace TextControlBoxNS.Core.Text.TextActions;

internal class AddNewLineTextAction
{
    private TextManager textManager;
    private UndoRedo undoRedo;
    private CursorManager cursorManager;
    private SelectionManager selectionManager;
    private CanvasUpdateManager canvasUpdateManager;
    private EventsManager eventsManager;
    private AutoIndentionManager autoIndentionManager;
    private TextActionManager textActionManager;
    private LongestLineManager longestLineManager;

    public void Init(
        TextManager textManager,
        UndoRedo undoRedo,
        CurrentLineManager currentLineManager,
        CursorManager cursorManager,
        EventsManager eventsManager,
        CanvasUpdateManager canvasUpdateManager,
        SelectionManager selectionManager,
        AutoIndentionManager autoIndentionManager,
        TextActionManager textActionsManager,
        LongestLineManager longestLineManager
        )
    {
        this.textManager = textManager;
        this.undoRedo = undoRedo;
        this.cursorManager = cursorManager;
        this.eventsManager = eventsManager;
        this.canvasUpdateManager = canvasUpdateManager;
        this.selectionManager = selectionManager;
        this.autoIndentionManager = autoIndentionManager;
        this.textActionManager = textActionsManager;
        this.longestLineManager = longestLineManager;
    }


    public bool HandleEmptyDocument()
    {
        if (textManager.LinesCount == 0)
        {
            textManager.AddLine();
            return true;
        }
        return false;
    }

    public bool HandleFullTextSelection()
    {
        if (selectionManager.WholeTextSelected())
        {
            undoRedo.RecordUndoAction(() =>
            {
                textManager.ClearText(true);
                textManager.InsertOrAdd(-1, "");
                cursorManager.SetCursorPosition(1, 0);
            }, selectionManager.currentTextSelection, 2, textManager.LinesCount);

            selectionManager.ClearSelection();
            longestLineManager.needsRecalculation = true;
            canvasUpdateManager.UpdateAll();
            eventsManager.CallTextChanged();
            return true;
        }
        return false;
    }

    public void ApplyLineSplitWithIndentation()
    {
        int lineNumber = cursorManager.LineNumber;
        int charPosition = cursorManager.GetCurPosInLine();

        string currentLineText = textManager.GetLineText(lineNumber);
        string indentation;
        //inside the leading whitespace the rest of the line still holds its own indentation,
        //adding the calculated one on top doubled it. Only repeat what is in front of the cursor.
        if (IsInLeadingWhitespace(currentLineText, charPosition))
        {
            indentation = currentLineText.Substring(0, charPosition);
        }
        else
        {
            int indentationLevel = autoIndentionManager.OnEnterPressed(lineNumber);
            indentation = indentationLevel > 0
                ? autoIndentionManager.RepeatIndentionString(indentationLevel)
                : string.Empty;
        }

        undoRedo.RecordUndoAction(() =>
        {
            var splitLines = currentLineText.SplitAt(charPosition);
            textManager.SetLineText(lineNumber, indentation + splitLines[1]);
            textManager.InsertOrAdd(lineNumber, splitLines[0]);
            cursorManager.SetCursorPosition(lineNumber + 1, indentation.Length);
        }, lineNumber, 1, 2);

        longestLineManager.needsRecalculation = true;
    }

    private static bool IsInLeadingWhitespace(string line, int position)
    {
        for (int i = 0; i < position && i < line.Length; i++)
        {
            if (line[i] != ' ' && line[i] != '\t')
                return false;
        }
        return true;
    }

    public void ReplaceSelectionWithNewLine()
    {
        textActionManager.AddCharacter(textManager.NewLineCharacter);
    }
}
