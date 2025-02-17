using Microsoft.UI.Xaml;
using System;
using TextControlBoxNS.Core;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Core.Selection;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Extensions;
using TextControlBoxNS.Models;

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
        public readonly string Tab = "\t";

        private UndoRedo undoRedo;
        private TextManager textManager;
        private SelectionManager selectionManager;
        private CursorManager cursorManager;
        private TextActionManager textActionsManager;
        public void Init(TextManager textManager, SelectionManager selectionManager, CursorManager cursorManager, TextActionManager textActionsManager, UndoRedo undoRedo)
        {
            this.textManager = textManager;
            this.selectionManager = selectionManager;
            this.cursorManager = cursorManager;
            this.textActionsManager = textActionsManager;
            this.undoRedo = undoRedo;
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
                textManager.totalLines[i] = textManager.totalLines[i].Replace(OldSpaces, Spaces);
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
                textManager.totalLines[i] = Replace(textManager.totalLines[i], Tab.ToString(), Spaces);
            }
        }
        public string Replace(string input, string find, string replace)
        {
            return input.Replace(find, replace, StringComparison.OrdinalIgnoreCase);
        }

        private void MoveTabBackNoSelection(string tabCharacter, CursorPosition cursorPosition, TextSelection textSelection)
        {
            string line = textManager.GetLineText(cursorPosition.LineNumber);
            if (line.Contains(tabCharacter, StringComparison.Ordinal) && cursorPosition.CharacterPosition > 0)
                cursorPosition.CharacterPosition -= tabCharacter.Length;

            undoRedo.RecordUndoAction(() =>
            {
                textManager.SetLineText(cursorPosition.LineNumber, line.RemoveFirstOccurence(tabCharacter));
            }, cursorPosition.LineNumber, 1, 1);

            textSelection.EndPosition.IsNull = true;
            textSelection.StartPosition.SetChangeValues(cursorPosition);
        }
        private void MoveTabBackSelection(string tabCharacter, CursorPosition cursorPosition, TextSelection textSelection)
        {
            var selection = selectionManager.OrderTextSelectionSeparated();
            int selectedLinesCount = selection.endLine - selection.startLine;

            undoRedo.RecordUndoAction(() =>
            {
                for (int i = 0; i < selectedLinesCount + 1; i++)
                {
                    int lineIndex = i + selection.startLine;
                    string currentLine = textManager.GetLineText(lineIndex);

                    //move the selection
                    if (i == 0 && currentLine.Contains(tabCharacter, StringComparison.Ordinal) && cursorPosition.CharacterPosition > 0)
                        selectionManager.SetSelectionStart(textSelection.StartPosition.LineNumber, textSelection.StartPosition.CharacterPosition - tabCharacter.Length);
                    else if (i == selectedLinesCount && currentLine.Contains(tabCharacter, StringComparison.Ordinal))
                        selectionManager.SetSelectionEnd(textSelection.EndPosition.LineNumber, textSelection.EndPosition.CharacterPosition - tabCharacter.Length);

                    textManager.SetLineText(lineIndex, currentLine.RemoveFirstOccurence(tabCharacter));
                }
            }, textSelection, selectedLinesCount);
        }
        public void MoveTabBack()
        {
            var cursorPosition = cursorManager.currentCursorPosition;
            var textSelection = selectionManager.currentTextSelection;
            string tabCharacter = this.TabCharacter;

            if (selectionManager.HasSelection)
            {
                MoveTabBackSelection(tabCharacter, cursorPosition, textSelection);
                return;
            }

            MoveTabBackNoSelection(tabCharacter, cursorPosition, textSelection);
        }


        private void MoveTabNoSelection(string tabCharacter, CursorPosition cursorPosition, TextSelection textSelection)
        {
            string line = textManager.GetLineText(cursorPosition.LineNumber);

            undoRedo.RecordUndoAction(() =>
            {
                textManager.SetLineText(cursorPosition.LineNumber, line.AddText(tabCharacter, cursorPosition.CharacterPosition));
            }, cursorPosition.LineNumber, 1, 1);

            cursorPosition.CharacterPosition += tabCharacter.Length;
            textSelection.EndPosition.IsNull = true;
            textSelection.StartPosition.SetChangeValues(cursorPosition);

        }
        private void MoveTabSelection(string tabCharacter, CursorPosition cursorPosition, TextSelection textSelection)
        {
            var selection = selectionManager.OrderTextSelectionSeparated();
            int selectedLinesCount = selection.endLine - selection.startLine;
            
            //Singleline just replace the selected with a tab character
            if (selection.startLine == selection.endLine) 
            {
                textActionsManager.AddCharacter(tabCharacter);
                return;
            }
            
            //multiline
            selectionManager.SetSelectionStart(textSelection.StartPosition.LineNumber, textSelection.StartPosition.CharacterPosition + tabCharacter.Length);
            selectionManager.SetSelectionEnd(textSelection.EndPosition.LineNumber, textSelection.EndPosition.CharacterPosition + tabCharacter.Length);
            cursorManager.currentCursorPosition.CharacterPosition += tabCharacter.Length;

            undoRedo.RecordUndoAction(() =>
            {
                for (int i = selection.startLine; i < selectedLinesCount + selection.startLine + 1; i++)
                {
                    textManager.SetLineText(i, textManager.GetLineText(i).AddToStart(tabCharacter));
                }
            }, textSelection, selectedLinesCount + 1);
        }

        public void MoveTab()
        {
            var cursorPosition = cursorManager.currentCursorPosition;
            var textSelection = selectionManager.currentTextSelection;
            string tabCharacter = this.TabCharacter;

            if (selectionManager.HasSelection)
            {
                MoveTabSelection(tabCharacter, cursorPosition, textSelection);
                return;
            }

            MoveTabNoSelection(tabCharacter, cursorPosition, textSelection);
        }
    }
}
