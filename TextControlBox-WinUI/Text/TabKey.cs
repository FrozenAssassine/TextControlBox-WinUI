using Collections.Pooled;
using TextControlBox.Extensions;
using TextControlBox_WinUI.Helper;
using TextControlBox_WinUI.Models;

namespace TextControlBox.Text
{
    internal class TabKey
    {
        public static TextSelection MoveTabBack(TextManager textManager, TextSelection TextSelection, CursorPosition CursorPosition, string TabCharacter, UndoRedoManager UndoRedo)
        {
            if (TextSelection == null)
            {
                string line = textManager.Lines.GetLineText(CursorPosition.LineNumber);
                if (line.Contains(TabCharacter, System.StringComparison.Ordinal) && CursorPosition.CharacterPosition > 0)
                    CursorPosition.SubtractFromCharacterPos(TabCharacter.Length);

                UndoRedo.RecordUndoAction(() =>
                {
                    textManager.Lines.SetLineText(CursorPosition.LineNumber, line.RemoveFirstOccurence(TabCharacter));
                }, CursorPosition.LineNumber, 1, 1);

                return new TextSelection(CursorPosition, null);
            }
            else
            {
                TextSelection = Selection.OrderTextSelection(TextSelection);
                int SelectedLinesCount = TextSelection.EndPosition.LineNumber - TextSelection.StartPosition.LineNumber;

                TextSelection tempSel = new TextSelection(TextSelection);
                tempSel.StartPosition.CharacterPosition = 0;
                tempSel.EndPosition.CharacterPosition = textManager.Lines.GetLineText(TextSelection.EndPosition.LineNumber).Length + TabCharacter.Length;

                UndoRedo.RecordUndoAction(() =>
                {
                    for (int i = 0; i < SelectedLinesCount + 1; i++)
                    {
                        int lineIndex = i + TextSelection.StartPosition.LineNumber;
                        string currentLine = textManager.Lines.GetLineText(lineIndex);

                        if (i == 0 && currentLine.Contains(TabCharacter, System.StringComparison.Ordinal) && CursorPosition.CharacterPosition > 0)
                            TextSelection.StartPosition.CharacterPosition -= TabCharacter.Length;
                        else if (i == SelectedLinesCount && currentLine.Contains(TabCharacter, System.StringComparison.Ordinal))
                        {
                            TextSelection.EndPosition.CharacterPosition -= TabCharacter.Length;
                        }

                        textManager.Lines.SetLineText(lineIndex, currentLine.RemoveFirstOccurence(TabCharacter));
                    }
                }, tempSel, SelectedLinesCount);

                return new TextSelection(new CursorPosition(TextSelection.StartPosition), new CursorPosition(TextSelection.EndPosition));
            }
        }

        public static TextSelection MoveTab(TextManager textManager, TextControlBoxProperties textBoxProperties, TextSelection textSelection, CursorPosition cursorPosition, string tabCharacter, UndoRedoManager undoRedo)
        {
            if (textSelection == null)
            {
                string line = textManager.Lines.GetLineText(cursorPosition.LineNumber);

                undoRedo.RecordUndoAction(() =>
                {
                    textManager.Lines.SetLineText(cursorPosition.LineNumber, line.AddText(tabCharacter, cursorPosition.CharacterPosition));
                }, cursorPosition.LineNumber, 1, 1);

                cursorPosition.AddToCharacterPos(tabCharacter.Length);
                return new TextSelection(cursorPosition, null);
            }
            else
            {
                textSelection = Selection.OrderTextSelection(textSelection);
                int SelectedLinesCount = textSelection.EndPosition.LineNumber - textSelection.StartPosition.LineNumber;

                if (textSelection.StartPosition.LineNumber == textSelection.EndPosition.LineNumber) //Singleline
                {
                    textSelection.StartPosition = Selection.Replace(textManager, textBoxProperties, textSelection, tabCharacter);
                }
                else
                {
                    TextSelection tempSel = new TextSelection(textSelection);
                    tempSel.StartPosition.CharacterPosition = 0;
                    tempSel.EndPosition.CharacterPosition = textManager.Lines.GetLineText(textSelection.EndPosition.LineNumber).Length + tabCharacter.Length;

                    textSelection.EndPosition.CharacterPosition += tabCharacter.Length;
                    textSelection.StartPosition.CharacterPosition += tabCharacter.Length;

                    undoRedo.RecordUndoAction(() =>
                    {
                        for (int i = textSelection.StartPosition.LineNumber; i < SelectedLinesCount + textSelection.StartPosition.LineNumber + 1; i++)
                        {
                            textManager.Lines.SetLineText(i, textManager.Lines.GetLineText(i).AddToStart(tabCharacter));
                        }
                    }, tempSel, SelectedLinesCount + 1);
                }
                return new TextSelection(new CursorPosition(textSelection.StartPosition), new CursorPosition(textSelection.EndPosition));
            }
        }
    }
}