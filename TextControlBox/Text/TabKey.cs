using Collections.Pooled;
using TextControlBoxNS.Extensions;

namespace TextControlBoxNS.Text
{
    internal class TabKey
    {
        public static TextSelection MoveTabBack(PooledList<string> totalLines, TextSelection textSelection, CursorPosition cursorPosition, string tabCharacter, string newLineCharacter, UndoRedo undoRedo)
        {
            if (textSelection == null)
            {
                string line = totalLines.GetLineText(cursorPosition.LineNumber);
                if (line.Contains(tabCharacter, System.StringComparison.Ordinal) && cursorPosition.CharacterPosition > 0)
                    cursorPosition.SubtractFromCharacterPos(tabCharacter.Length);

                undoRedo.RecordUndoAction(() =>
                {
                    totalLines.SetLineText(cursorPosition.LineNumber, line.RemoveFirstOccurence(tabCharacter));
                }, totalLines, cursorPosition.LineNumber, 1, 1, newLineCharacter);

                return new TextSelection(cursorPosition, null);
            }

            textSelection = Selection.OrderTextSelection(textSelection);
            int selectedLinesCount = textSelection.EndPosition.LineNumber - textSelection.StartPosition.LineNumber;

            TextSelection tempSel = new TextSelection(textSelection);
            tempSel.StartPosition.CharacterPosition = 0;
            tempSel.EndPosition.CharacterPosition = totalLines.GetLineText(textSelection.EndPosition.LineNumber).Length + tabCharacter.Length;

            undoRedo.RecordUndoAction(() =>
            {
                for (int i = 0; i < selectedLinesCount + 1; i++)
                {
                    int lineIndex = i + textSelection.StartPosition.LineNumber;
                    string currentLine = totalLines.GetLineText(lineIndex);

                    if (i == 0 && currentLine.Contains(tabCharacter, System.StringComparison.Ordinal) && cursorPosition.CharacterPosition > 0)
                        textSelection.StartPosition.CharacterPosition -= tabCharacter.Length;
                    else if (i == selectedLinesCount && currentLine.Contains(tabCharacter, System.StringComparison.Ordinal))
                    {
                        textSelection.EndPosition.CharacterPosition -= tabCharacter.Length;
                    }

                    totalLines.SetLineText(lineIndex, currentLine.RemoveFirstOccurence(tabCharacter));
                }
            }, totalLines, tempSel, selectedLinesCount, newLineCharacter);

            return new TextSelection(new CursorPosition(textSelection.StartPosition), new CursorPosition(textSelection.EndPosition));
        }

        public static TextSelection MoveTab(PooledList<string> totalLines, TextSelection textSelection, CursorPosition cursorPosition, string tabCharacter, string newLineCharacter, UndoRedo undoRedo)
        {
            if (textSelection == null)
            {
                string line = totalLines.GetLineText(cursorPosition.LineNumber);

                undoRedo.RecordUndoAction(() =>
                {
                    totalLines.SetLineText(cursorPosition.LineNumber, line.AddText(tabCharacter, cursorPosition.CharacterPosition));
                }, totalLines, cursorPosition.LineNumber, 1, 1, newLineCharacter);

                cursorPosition.AddToCharacterPos(tabCharacter.Length);
                return new TextSelection(cursorPosition, null);
            }

            textSelection = Selection.OrderTextSelection(textSelection);
            int selectedLinesCount = textSelection.EndPosition.LineNumber - textSelection.StartPosition.LineNumber;

            if (textSelection.StartPosition.LineNumber == textSelection.EndPosition.LineNumber) //Singleline
                textSelection.StartPosition = Selection.Replace(textSelection, totalLines, tabCharacter, newLineCharacter);
            else
            {
                TextSelection tempSel = new TextSelection(textSelection);
                tempSel.StartPosition.CharacterPosition = 0;
                tempSel.EndPosition.CharacterPosition = totalLines.GetLineText(textSelection.EndPosition.LineNumber).Length + tabCharacter.Length;

                textSelection.EndPosition.CharacterPosition += tabCharacter.Length;
                textSelection.StartPosition.CharacterPosition += tabCharacter.Length;

                undoRedo.RecordUndoAction(() =>
                {
                    for (int i = textSelection.StartPosition.LineNumber; i < selectedLinesCount + textSelection.StartPosition.LineNumber + 1; i++)
                    {
                        totalLines.SetLineText(i, totalLines.GetLineText(i).AddToStart(tabCharacter));
                    }
                }, totalLines, tempSel, selectedLinesCount + 1, newLineCharacter);
            }
            return new TextSelection(new CursorPosition(textSelection.StartPosition), new CursorPosition(textSelection.EndPosition));
        }
    }
}