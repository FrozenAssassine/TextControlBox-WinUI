using System;
using TextControlBoxNS.Core.Selection;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Extensions;
using TextControlBoxNS.Models;

namespace TextControlBoxNS.Core;

internal class TabSpaceManager
{
    private int _NumberOfSpaces = 4;
    private string OldSpaces = "    ";

    public int NumberOfSpaces
    {
        get => _NumberOfSpaces;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException("Number of spaces must be greater than zero.");

            if (value != _NumberOfSpaces)
            {
                OldSpaces = Spaces;
                _NumberOfSpaces = value;
                Spaces = new string(' ', _NumberOfSpaces);

                eventsManager.CallTabsSpacesChanged(UseSpacesInsteadTabs, value);
            }
        }
    }

    private bool _UseSpacesInsteadTabs = false;
    public bool UseSpacesInsteadTabs
    {
        get => _UseSpacesInsteadTabs;
        set
        {
            _UseSpacesInsteadTabs = value;
            eventsManager.CallTabsSpacesChanged(value, _NumberOfSpaces);
        }
    }
    public string TabCharacter { get => UseSpacesInsteadTabs ? Spaces : Tab; }
    private string Spaces = "    ";
    public readonly string Tab = "\t";

    private UndoRedo undoRedo;
    private TextManager textManager;
    private SelectionManager selectionManager;
    private CursorManager cursorManager;
    private TextActionManager textActionsManager;
    private LongestLineManager longestLineManager;
    private EventsManager eventsManager;

    public void Init(TextManager textManager, SelectionManager selectionManager, CursorManager cursorManager, TextActionManager textActionsManager, UndoRedo undoRedo, LongestLineManager longestLineManager, EventsManager eventsManager)
    {
        this.textManager = textManager;
        this.selectionManager = selectionManager;
        this.cursorManager = cursorManager;
        this.textActionsManager = textActionsManager;
        this.undoRedo = undoRedo;
        this.longestLineManager = longestLineManager;
        this.eventsManager = eventsManager;
    }

    public void UpdateNumberOfSpaces()
    {
        ReplaceSpacesToSpaces();

        //longest line index must be the same, just recalculate its length
        longestLineManager.MeasureActualLineLength();
    }
    public void UpdateTabs()
    {
        if (UseSpacesInsteadTabs)
            ReplaceTabsToSpaces();
        else
            ReplaceSpacesToTabs();

        //longest line index must be the same, just recalculate its length
        longestLineManager.MeasureActualLineLength();
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

    private void MoveTabBackSingleLine(string tabCharacter, CursorPosition cursorPosition, TextSelection textSelection)
    {
        int lineIndex;
        var orderedSelection = selectionManager.OrderTextSelectionSeparated();
        if (textSelection.HasSelection)
            lineIndex = orderedSelection.startLine;
        else
            lineIndex = cursorManager.LineNumber;

        bool textChanged = false;
        undoRedo.RecordUndoAction(() =>
        {
            string currentLine = textManager.GetLineText(lineIndex);

            if (!currentLine.StartsWith(tabCharacter, StringComparison.Ordinal))
                return;

            string textRemovedTab = currentLine.Substring(tabCharacter.Length);

            if (!textChanged)
                textChanged = !textRemovedTab.Equals(currentLine);

            textManager.SetLineText(lineIndex, textRemovedTab);

            if (textChanged)
            {
                longestLineManager.needsRecalculation = true;

                if (selectionManager.HasSelection)
                {
                    selectionManager.SetSelectionStart(orderedSelection.startLine, orderedSelection.startChar >= tabCharacter.Length ? orderedSelection.startChar - tabCharacter.Length : 0);
                    selectionManager.SetSelectionEnd(orderedSelection.endLine, orderedSelection.endChar >= tabCharacter.Length ? orderedSelection.endChar - tabCharacter.Length : 0);
                }
                cursorManager.CharacterPosition = Math.Max(cursorManager.CharacterPosition - tabCharacter.Length, 0);
            }

        }, textSelection, 1);

        if (textChanged)
            eventsManager.CallTextChanged();
        longestLineManager.needsRecalculation = true;
    }
    private void MoveTabBackSelection(string tabCharacter, CursorPosition cursorPosition, TextSelection textSelection)
    {
        var selection = selectionManager.OrderTextSelectionSeparated();
        int selectedLinesCount = selection.endLine - selection.startLine + 1;

        bool textChanged = false;
        undoRedo.RecordUndoActionTab(() =>
        {
            for (int i = 0; i < selectedLinesCount; i++) 
            {
                int lineIndex = selection.startLine + i;
                string currentLine = textManager.GetLineText(lineIndex);

                if (!currentLine.StartsWith(tabCharacter, StringComparison.Ordinal))
                    continue;

                string textRemovedTab = currentLine.Substring(tabCharacter.Length);

                if (!textChanged)
                    textChanged = !textRemovedTab.Equals(currentLine);
                textManager.SetLineText(lineIndex, textRemovedTab);
            }

            if (textChanged)
            {
                longestLineManager.needsRecalculation = true;

                //move the orderedSelection
                selectionManager.SetSelectionStart(selection.startLine, selection.startChar >= tabCharacter.Length ? selection.startChar - tabCharacter.Length : 0);
                selectionManager.SetSelectionEnd(selection.endLine, selection.endChar >= tabCharacter.Length ? selection.endChar - tabCharacter.Length : 0);

                cursorManager.CharacterPosition = Math.Max(cursorManager.CharacterPosition - tabCharacter.Length, 0);
            }

        }, textSelection, selectedLinesCount);

        if (textChanged)
            eventsManager.CallTextChanged();
        longestLineManager.needsRecalculation = true;
    }
    public void MoveTabBack()
    {
        var cursorPosition = cursorManager.currentCursorPosition;
        var textSelection = selectionManager.currentTextSelection;
        string tabCharacter = TabCharacter;

        if (selectionManager.HasSelection && selectionManager.selectionStart.LineNumber != selectionManager.selectionEnd.LineNumber)
        {
            MoveTabBackSelection(tabCharacter, cursorPosition, textSelection);
            return;
        }

        MoveTabBackSingleLine(tabCharacter, cursorPosition, textSelection);
    }

    private void MoveTabNoSelection(string tabCharacter, CursorPosition cursorPosition, TextSelection textSelection)
    {
        textActionsManager.AddCharacter(tabCharacter);
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
        undoRedo.RecordUndoAction(() =>
        {
            selectionManager.SetSelectionStart(selection.startLine, selection.startChar + tabCharacter.Length);
            selectionManager.SetSelectionEnd(selection.endLine, selection.endChar + tabCharacter.Length);
            cursorManager.currentCursorPosition.CharacterPosition += tabCharacter.Length;

            for (int i = selection.startLine; i < selectedLinesCount + selection.startLine + 1; i++)
            {
                textManager.SetLineText(i, textManager.GetLineText(i).AddToStart(tabCharacter));
            }
        }, textSelection, selectedLinesCount + 1);

        eventsManager.CallTextChanged();
        longestLineManager.needsRecalculation = true;
    }
    public void MoveTab()
    {
        var cursorPosition = cursorManager.currentCursorPosition;
        var textSelection = selectionManager.currentTextSelection;
        string tabCharacter = TabCharacter;

        if (selectionManager.HasSelection)
        {
            MoveTabSelection(tabCharacter, cursorPosition, textSelection);
            return;
        }
        MoveTabNoSelection(tabCharacter, cursorPosition, textSelection);
    }
}