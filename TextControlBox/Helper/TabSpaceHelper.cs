using System;
using TextControlBoxNS.Extensions;
using TextControlBoxNS.Text;

namespace TextControlBoxNS.Helper
{
    internal class TabSpaceHelper
    {
        private int _NumberOfSpaces = 4;
        private string OldSpaces = "    ";

        public int NumberOfSpaces
        {
            get => _NumberOfSpaces;
            set
            {
                if (value != _NumberOfSpaces)
                {
                    OldSpaces = Spaces;
                    _NumberOfSpaces = value;
                    Spaces = new string(' ', _NumberOfSpaces);
                }
            }
        }
        public bool UseSpacesInsteadTabs = false;
        public string TabCharacter { get => UseSpacesInsteadTabs ? Spaces : Tab; }
        private string Spaces = "    ";
        private string Tab = "\t";

        private TextManager textManager;
        private SelectionManager selectionManager;
        public void Init(TextManager textManager, SelectionManager selectionManager)
        {
            this.textManager = textManager;
            this.selectionManager = selectionManager;
        }

        public void UpdateNumberOfSpaces()
        {
            ReplaceSpacesToSpaces();
        }

        public void UpdateTabs()
        {
            if (UseSpacesInsteadTabs)
                ReplaceTabsToSpaces();
            else
                ReplaceSpacesToTabs();
        }
        public string UpdateTabs(string input)
        {
            if (UseSpacesInsteadTabs)
                return Replace(input, Tab, Spaces);
            return Replace(input, Spaces, Tab);
        }

        private void ReplaceSpacesToSpaces()
        {
            for (int i = 0; i < textManager.LinesCount; i++)
            {
                textManager.totalLines[i] = Replace(textManager.totalLines[i], OldSpaces, Spaces);
            }
        }
        private void ReplaceSpacesToTabs()
        {
            for (int i = 0; i < textManager.LinesCount; i++)
            {
                textManager.totalLines[i] = Replace(textManager.totalLines[i], Spaces, Tab);
            }
        }
        private void ReplaceTabsToSpaces()
        {
            for (int i = 0; i < textManager.LinesCount; i++)
            {
                textManager.totalLines[i] = Replace(textManager.totalLines[i], "\t", Spaces);
            }
        }
        public string Replace(string input, string find, string replace)
        {
            return input.Replace(find, replace, StringComparison.OrdinalIgnoreCase);
        }

        public TextSelection MoveTabBack(TextSelection textSelection, CursorPosition cursorPosition, string tabCharacter, UndoRedo undoRedo)
        {
            if (textSelection == null)
            {
                string line = textManager.GetLineText(cursorPosition.LineNumber);
                if (line.Contains(tabCharacter, System.StringComparison.Ordinal) && cursorPosition.CharacterPosition > 0)
                    cursorPosition.SubtractFromCharacterPos(tabCharacter.Length);

                undoRedo.RecordUndoAction(() =>
                {
                    textManager.SetLineText(cursorPosition.LineNumber, line.RemoveFirstOccurence(tabCharacter));
                }, cursorPosition.LineNumber, 1, 1);

                return new TextSelection(cursorPosition, null);
            }

            textSelection = selectionManager.OrderTextSelection(textSelection);
            int selectedLinesCount = textSelection.EndPosition.LineNumber - textSelection.StartPosition.LineNumber;

            TextSelection tempSel = new TextSelection(textSelection);
            tempSel.StartPosition.CharacterPosition = 0;
            tempSel.EndPosition.CharacterPosition = textManager.GetLineText(textSelection.EndPosition.LineNumber).Length + tabCharacter.Length;

            undoRedo.RecordUndoAction(() =>
            {
                for (int i = 0; i < selectedLinesCount + 1; i++)
                {
                    int lineIndex = i + textSelection.StartPosition.LineNumber;
                    string currentLine = textManager.GetLineText(lineIndex);

                    if (i == 0 && currentLine.Contains(tabCharacter, System.StringComparison.Ordinal) && cursorPosition.CharacterPosition > 0)
                        textSelection.StartPosition.CharacterPosition -= tabCharacter.Length;
                    else if (i == selectedLinesCount && currentLine.Contains(tabCharacter, System.StringComparison.Ordinal))
                    {
                        textSelection.EndPosition.CharacterPosition -= tabCharacter.Length;
                    }

                    textManager.SetLineText(lineIndex, currentLine.RemoveFirstOccurence(tabCharacter));
                }
            }, tempSel, selectedLinesCount);

            return new TextSelection(new CursorPosition(textSelection.StartPosition), new CursorPosition(textSelection.EndPosition));
        }

        public TextSelection MoveTab(TextSelection textSelection, CursorPosition cursorPosition, string tabCharacter, UndoRedo undoRedo)
        {
            if (textSelection == null)
            {
                string line = textManager.GetLineText(cursorPosition.LineNumber);

                undoRedo.RecordUndoAction(() =>
                {
                    textManager.SetLineText(cursorPosition.LineNumber, line.AddText(tabCharacter, cursorPosition.CharacterPosition));
                }, cursorPosition.LineNumber, 1, 1);

                cursorPosition.AddToCharacterPos(tabCharacter.Length);
                return new TextSelection(cursorPosition, null);
            }

            textSelection = selectionManager.OrderTextSelection(textSelection);
            int selectedLinesCount = textSelection.EndPosition.LineNumber - textSelection.StartPosition.LineNumber;

            if (textSelection.StartPosition.LineNumber == textSelection.EndPosition.LineNumber) //Singleline
                textSelection.StartPosition = selectionManager.Replace(textSelection, tabCharacter);
            else
            {
                TextSelection tempSel = new TextSelection(textSelection);
                tempSel.StartPosition.CharacterPosition = 0;
                tempSel.EndPosition.CharacterPosition = textManager.GetLineText(textSelection.EndPosition.LineNumber).Length + tabCharacter.Length;

                textSelection.EndPosition.CharacterPosition += tabCharacter.Length;
                textSelection.StartPosition.CharacterPosition += tabCharacter.Length;

                undoRedo.RecordUndoAction(() =>
                {
                    for (int i = textSelection.StartPosition.LineNumber; i < selectedLinesCount + textSelection.StartPosition.LineNumber + 1; i++)
                    {
                        textManager.SetLineText(i, textManager.GetLineText(i).AddToStart(tabCharacter));
                    }
                }, tempSel, selectedLinesCount + 1);
            }
            return new TextSelection(new CursorPosition(textSelection.StartPosition), new CursorPosition(textSelection.EndPosition));
        }
    }
}
