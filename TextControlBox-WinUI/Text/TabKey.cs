using TextControlBox.Extensions;
using TextControlBox.Helper;
using TextControlBox_WinUI.Helper;
using TextControlBox_WinUI.Models;

namespace TextControlBox.Text
{
    internal class TabKey
    {
        public static TextSelection MoveTabBack(TextManager textManager, SelectionManager selectionManager, TabSpaceHelper tabSpaceHelper, UndoRedoManager UndoRedo)
        {
            if (selectionManager.HasSelection)
            {
                string line = textManager.Lines.GetLineText(selectionManager.Cursor.LineNumber);
                if (line.Contains(tabSpaceHelper.TabCharacter, System.StringComparison.Ordinal) && selectionManager.Cursor.CharacterPosition > 0)
                    selectionManager.Cursor.SubtractFromCharacterPos(tabSpaceHelper.TabCharacter.Length);

                UndoRedo.RecordUndoAction(() =>
                {
                    textManager.Lines.SetLineText(selectionManager.Cursor.LineNumber, line.RemoveFirstOccurence(tabSpaceHelper.TabCharacter));
                }, selectionManager.Cursor.LineNumber, 1, 1);

                return new TextSelection(selectionManager.Cursor, null);
            }
            else
            {
                selectionManager.Selection = Selection.OrderTextSelection(selectionManager.Selection);
                int SelectedLinesCount = selectionManager.Selection.EndPosition.LineNumber - selectionManager.Selection.StartPosition.LineNumber;

                TextSelection tempSel = new TextSelection(selectionManager.Selection);
                tempSel.StartPosition.CharacterPosition = 0;
                tempSel.EndPosition.CharacterPosition = textManager.Lines.GetLineText(selectionManager.Selection.EndPosition.LineNumber).Length + tabSpaceHelper.TabCharacter.Length;

                UndoRedo.RecordUndoAction(() =>
                {
                    for (int i = 0; i < SelectedLinesCount + 1; i++)
                    {
                        int lineIndex = i + selectionManager.Selection.StartPosition.LineNumber;
                        string currentLine = textManager.Lines.GetLineText(lineIndex);

                        if (i == 0 && currentLine.Contains(tabSpaceHelper.TabCharacter, System.StringComparison.Ordinal) && selectionManager.Cursor.CharacterPosition > 0)
                            selectionManager.Selection.StartPosition.CharacterPosition -= tabSpaceHelper.TabCharacter.Length;
                        else if (i == SelectedLinesCount && currentLine.Contains(tabSpaceHelper.TabCharacter, System.StringComparison.Ordinal))
                        {
                            selectionManager.Selection.EndPosition.CharacterPosition -= tabSpaceHelper.TabCharacter.Length;
                        }

                        textManager.Lines.SetLineText(lineIndex, currentLine.RemoveFirstOccurence(tabSpaceHelper.TabCharacter));
                    }
                }, tempSel, SelectedLinesCount);

                return new TextSelection(new CursorPosition(selectionManager.Selection.StartPosition), new CursorPosition(selectionManager.Selection.EndPosition));
            }
        }

        public static TextSelection MoveTab(TextManager textManager, TextControlBoxProperties textBoxProperties, SelectionManager selectionManager, TabSpaceHelper tabSpaceHelper, UndoRedoManager undoRedo)
        {
            if (selectionManager.HasSelection)
            {
                string line = textManager.Lines.GetLineText(selectionManager.Cursor.LineNumber);

                undoRedo.RecordUndoAction(() =>
                {
                    textManager.Lines.SetLineText(selectionManager.Cursor.LineNumber, line.AddText(tabSpaceHelper.TabCharacter, selectionManager.Cursor.CharacterPosition));
                }, selectionManager.Cursor.LineNumber, 1, 1);

                selectionManager.Cursor.AddToCharacterPos(tabSpaceHelper.TabCharacter.Length);
                return new TextSelection(selectionManager.Cursor, null);
            }
            else
            {
                selectionManager.Selection = Selection.OrderTextSelection(selectionManager.Selection);
                int SelectedLinesCount = selectionManager.Selection.EndPosition.LineNumber - selectionManager.Selection.StartPosition.LineNumber;

                if (selectionManager.Selection.StartPosition.LineNumber == selectionManager.Selection.EndPosition.LineNumber) //Singleline
                {
                    selectionManager.Selection.StartPosition = Selection.Replace(textManager, textBoxProperties, selectionManager.Selection, tabSpaceHelper.TabCharacter);
                }
                else
                {
                    TextSelection tempSel = new TextSelection(selectionManager.Selection);
                    tempSel.StartPosition.CharacterPosition = 0;
                    tempSel.EndPosition.CharacterPosition = textManager.Lines.GetLineText(selectionManager.Selection.EndPosition.LineNumber).Length + tabSpaceHelper.TabCharacter.Length;

                    selectionManager.Selection.EndPosition.CharacterPosition += tabSpaceHelper.TabCharacter.Length;
                    selectionManager.Selection.StartPosition.CharacterPosition += tabSpaceHelper.TabCharacter.Length;

                    undoRedo.RecordUndoAction(() =>
                    {
                        for (int i = selectionManager.Selection.StartPosition.LineNumber; i < SelectedLinesCount + selectionManager.Selection.StartPosition.LineNumber + 1; i++)
                        {
                            textManager.Lines.SetLineText(i, textManager.Lines.GetLineText(i).AddToStart(tabSpaceHelper.TabCharacter));
                        }
                    }, tempSel, SelectedLinesCount + 1);
                }
                return new TextSelection(new CursorPosition(selectionManager.Selection.StartPosition), new CursorPosition(selectionManager.Selection.EndPosition));
            }
        }
    }
}