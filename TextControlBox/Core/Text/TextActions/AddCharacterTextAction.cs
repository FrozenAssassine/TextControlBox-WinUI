using System;
using System.Diagnostics;
using TextControlBoxNS.Core.Selection;
using TextControlBoxNS.Extensions;

namespace TextControlBoxNS.Core.Text.TextActions;

internal class AddCharacterTextAction
{
    private TextManager textManager;
    private CoreTextControlBox coreTextbox;
    private UndoRedo undoRedo;
    private CurrentLineManager currentLineManager;
    private LongestLineManager longestLineManager;
    private CursorManager cursorManager;
    private SelectionManager selectionManager;
    private CanvasUpdateManager canvasUpdateManager;

    public void Init(
        TextManager textManager,
        CoreTextControlBox coreTextbox,
        UndoRedo undoRedo,
        CurrentLineManager currentLineManager,
        LongestLineManager longestLineManager,
        CursorManager cursorManager,
        SelectionManager selectionManager,
        CanvasUpdateManager canvasUpdateHelper)
    {
        this.textManager = textManager;
        this.coreTextbox = coreTextbox;
        this.undoRedo = undoRedo;
        this.currentLineManager = currentLineManager;
        this.longestLineManager = longestLineManager;
        this.cursorManager = cursorManager;
        this.selectionManager = selectionManager;
        this.canvasUpdateManager = canvasUpdateHelper;
    }

    public int CalculateSplitTextLength(string text)
    {
        return text.Length > 1 && text.Contains(textManager.NewLineCharacter, StringComparison.Ordinal)
            ? text.CountLines(textManager.NewLineCharacter)
            : 1;
    }

    public void HandleSingleCharacterWithoutSelection(string text)
    {
        var res = AutoPairing.AutoPair(coreTextbox, text);
        text = res.text;

        undoRedo.RecordUndoAction(() =>
        {
            var characterPos = cursorManager.GetCurPosInLine();

            if (characterPos > currentLineManager.Length - 1)
                currentLineManager.AddToEnd(text);
            else
                currentLineManager.AddText(text, characterPos);

            cursorManager.CharacterPosition = res.length + characterPos;
        }, cursorManager.LineNumber, 1, 1);

        if (currentLineManager.Length > longestLineManager.longestLineLength)
        {
            longestLineManager.longestIndex = cursorManager.LineNumber;
            longestLineManager.MeasureActualLineLength();
        }
    }

    public void HandleMultiLineTextWithoutSelection(string text, int splittedTextLength)
    {
        undoRedo.RecordUndoAction(() =>
        {
            selectionManager.InsertText(text);
        }, cursorManager.LineNumber, 1, splittedTextLength);
        
        longestLineManager.CheckRecalculateLongestLine(text);
    }

    public void HandleTextWithSelection(string text, int splittedTextLength)
    {
        text = AutoPairing.AutoPairSelection(coreTextbox, text);
        if (text == null)
            return;

        undoRedo.RecordUndoAction(() =>
        {
            selectionManager.Replace(text);
            selectionManager.ClearSelection();
        }, selectionManager.currentTextSelection, splittedTextLength);
        
        longestLineManager.Recalculate();

        canvasUpdateManager.UpdateAll();
    }
}
