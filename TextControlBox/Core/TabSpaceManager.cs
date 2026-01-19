using System;
using System.Text.RegularExpressions;
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
    private bool documentUsesSpaces;
    private int documentNumberOfSpaces;

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

    public void SetDocumentVariables(int spaces, bool spacesInsteadTabs)
    {
        documentNumberOfSpaces = spaces;
        documentUsesSpaces = spacesInsteadTabs;
    }

    private void RecordUndoStep(Action action, int spacesAfter, int documentSpacesAfter)
    {
        var data = new TabSpaceUndoData(
                this.UseSpacesInsteadTabs ? this.NumberOfSpaces : -1,
                this.documentUsesSpaces ? this.documentNumberOfSpaces : -1,
                spacesAfter,
                documentSpacesAfter
            );

        undoRedo.RecordUndoAction(() =>
        {
            action.Invoke();
        }, 0, textManager.LinesCount, textManager.LinesCount, data);
    }


    public void RewriteTabsSpaces(int targetSpacesTabs)
    {
        //when rewriting the tabs/spaces, we change the acutal text indentation,
        //but also we update the NumberOfSpaces and UseSpacesInsteadTabs properties,
        //which are only related to the with of the tab key. The event gets fired as well,
        //but always with the NumberOfSpaces and UseSpacesInsteadTabs values.
        //The document values stay the same and also stay private in this document.
        if (textManager == null) return;

        if (targetSpacesTabs == -1 && documentUsesSpaces)
        {
            RecordUndoStep(() =>
            {
                ConvertIndentationToTabs();
            }, -1, -1);

            documentUsesSpaces = UseSpacesInsteadTabs = false;
            NumberOfSpaces = 4;
            eventsManager.CallTextChanged();
        }
        else if (targetSpacesTabs > 0 && documentUsesSpaces && targetSpacesTabs != documentNumberOfSpaces)
        {
            RecordUndoStep(() =>
            {
                ConvertIndentationSpacesToSpaces(targetSpacesTabs);
            }, targetSpacesTabs, targetSpacesTabs);

            NumberOfSpaces = documentNumberOfSpaces = targetSpacesTabs;
            UseSpacesInsteadTabs = true;
            eventsManager.CallTextChanged();
        }
        else if (targetSpacesTabs > 0 && !documentUsesSpaces)
        {
            RecordUndoStep(() =>
            {
                ConvertIndentationToSpaces(targetSpacesTabs);
            }, targetSpacesTabs, targetSpacesTabs);

            documentUsesSpaces = UseSpacesInsteadTabs = true;
            documentNumberOfSpaces = NumberOfSpaces = targetSpacesTabs;
            eventsManager.CallTextChanged();
        }

        longestLineManager.MeasureActualLineLength();
    }

    private void ConvertIndentationToTabs()
    {
        for (int i = 0; i < textManager.LinesCount; i++)
        {
            string line = textManager.totalLines[i];
            if (string.IsNullOrEmpty(line)) continue;

            int indentEnd = GetIndentationEnd(line);
            if (indentEnd == 0) continue;

            // Process only the indentation part
            string indent = line.Substring(0, indentEnd);
            string rest = line.Substring(indentEnd);

            // Convert indentation to tabs
            string newIndent = ConvertIndentStringToTabs(indent, documentNumberOfSpaces);
            textManager.totalLines[i] = newIndent + rest;
        }
    }

    private void ConvertIndentationToSpaces(int spacesCount)
    {
        for (int i = 0; i < textManager.LinesCount; i++)
        {
            string line = textManager.totalLines[i];
            if (string.IsNullOrEmpty(line)) continue;

            int indentEnd = GetIndentationEnd(line);
            if (indentEnd == 0) continue;

            // Process only the indentation part
            string indent = line.Substring(0, indentEnd);
            string rest = line.Substring(indentEnd);

            // Convert indentation to spaces
            string newIndent = ConvertIndentStringToSpaces(indent, documentNumberOfSpaces, spacesCount);
            textManager.totalLines[i] = newIndent + rest;
        }
    }

    private void ConvertIndentationSpacesToSpaces(int newSpacesCount)
    {
        for (int i = 0; i < textManager.LinesCount; i++)
        {
            string line = textManager.totalLines[i];
            if (string.IsNullOrEmpty(line)) continue;

            int indentEnd = GetIndentationEnd(line);
            if (indentEnd == 0) continue;

            // Process only the indentation part
            string indent = line.Substring(0, indentEnd);
            string rest = line.Substring(indentEnd);

            // Convert spaces to different width spaces
            string newIndent = ConvertIndentStringToSpaces(indent, documentNumberOfSpaces, newSpacesCount);
            textManager.totalLines[i] = newIndent + rest;
        }
    }

    // Gets the index where indentation ends (first non-whitespace character)
    private int GetIndentationEnd(string line)
    {
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] != ' ' && line[i] != '\t')
                return i;
        }
        return line.Length; // Entire line is whitespace
    }

    // Converts mixed tabs/spaces indentation to tabs
    private string ConvertIndentStringToTabs(string indent, int spacesPerTab)
    {
        if (string.IsNullOrEmpty(indent)) return indent;

        // Calculate total indent level (in virtual columns)
        int totalColumns = 0;
        for (int i = 0; i < indent.Length; i++)
        {
            if (indent[i] == '\t')
                totalColumns += spacesPerTab;
            else
                totalColumns++;
        }

        // Convert to tabs + remaining spaces
        int tabs = totalColumns / spacesPerTab;
        int remainingSpaces = totalColumns % spacesPerTab;

        // Memory efficient: use stack allocation for small strings
        if (tabs + remainingSpaces < 256)
        {
            Span<char> result = stackalloc char[tabs + remainingSpaces];
            result.Slice(0, tabs).Fill('\t');
            result.Slice(tabs, remainingSpaces).Fill(' ');
            return new string(result);
        }
        else
        {
            return new string('\t', tabs) + new string(' ', remainingSpaces);
        }
    }

    // Converts mixed tabs/spaces indentation to spaces
    private string ConvertIndentStringToSpaces(string indent, int oldSpacesPerTab, int newSpacesPerTab)
    {
        if (string.IsNullOrEmpty(indent)) return indent;

        // Calculate total indent level in old space units
        int totalOldSpaces = 0;
        for (int i = 0; i < indent.Length; i++)
        {
            if (indent[i] == '\t')
                totalOldSpaces += oldSpacesPerTab;
            else
                totalOldSpaces++;
        }

        // Convert to new space units (preserving relative indentation)
        int newSpaceCount = (totalOldSpaces * newSpacesPerTab) / oldSpacesPerTab;

        // Memory efficient: use stack allocation for small strings
        if (newSpaceCount < 256)
        {
            Span<char> result = stackalloc char[newSpaceCount];
            result.Fill(' ');
            return new string(result);
        }
        else
        {
            return new string(' ', newSpaceCount);
        }
    }

    public string Replace(string input, string find, string replace)
    {
        return input.Replace(find, replace, StringComparison.OrdinalIgnoreCase);
    }
    public string UpdateTabs(string input)
    {
        if (UseSpacesInsteadTabs)
            return Replace(input, Tab, Spaces);
        return Replace(input, Spaces, Tab);
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
        Action action = () =>
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
        };

        if (textSelection.HasSelection)
            undoRedo.RecordUndoAction(action, textSelection, 1);
        else
            undoRedo.RecordUndoAction(action, lineIndex, 1, 1);

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