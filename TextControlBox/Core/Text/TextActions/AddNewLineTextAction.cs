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

    public void Init(
        TextManager textManager,
        UndoRedo undoRedo,
        CurrentLineManager currentLineManager,
        CursorManager cursorManager,
        EventsManager eventsManager,
        CanvasUpdateManager canvasUpdateManager,
        SelectionManager selectionManager,
        AutoIndentionManager autoIndentionManager,
        TextActionManager textActionsManager
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
                cursorManager.SetCursorPosition(1, 1);
            }, 0, textManager.LinesCount, 2);

            selectionManager.ClearSelection();
            canvasUpdateManager.UpdateAll();
            eventsManager.CallTextChanged();
            return true;
        }
        return false;
    }

    public void ApplyLineSplitWithIndentation()
    {
        int lineNumber = cursorManager.LineNumber;
        int charPosition = cursorManager.CharacterPosition;

        string currentLineText = textManager.GetLineText(lineNumber);
        int indentationLevel = autoIndentionManager.OnEnterPressed(lineNumber);
        string indentation = indentationLevel > 0
            ? autoIndentionManager.RepeatIndentionString(indentationLevel)
            : string.Empty;

        undoRedo.RecordUndoAction(() =>
        {
            var splitLines = currentLineText.SplitAt(charPosition);
            textManager.SetLineText(lineNumber, indentation + splitLines[1]);
            textManager.InsertOrAdd(lineNumber, splitLines[0]);
            cursorManager.SetCursorPosition(lineNumber + 1, indentation.Length);
        }, lineNumber, 1, 2);

    }

    public void ReplaceSelectionWithNewLine()
    {
        textActionManager.AddCharacter(textManager.NewLineCharacter);
    }
}
