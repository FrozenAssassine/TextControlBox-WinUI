using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Core.Selection;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Extensions;
using TextControlBoxNS.Models;

namespace TextControlBoxNS.Core;

internal class ReplaceManager
{
    private CanvasUpdateManager canvasUpdateManager;
    private UndoRedo undoRedo;
    private TextManager textManager;
    private SearchManager searchManager;
    private CursorManager cursorManager;
    private TextActionManager textActionManager;
    private SelectionRenderer selectionRenderer;
    private SelectionManager selectionManager;
    private EventsManager eventsManager;
    private LongestLineManager longestLineManager;

    public void Init(
        CanvasUpdateManager canvasUpdateManager,
        UndoRedo undoRedo,
        TextManager textManager,
        SearchManager searchManager,
        CursorManager cursorManager,
        TextActionManager textActionManager,
        SelectionRenderer selectionRenderer,
        SelectionManager selectionManager,
        EventsManager eventsManager,
        LongestLineManager longestLineManager)
    {
        this.canvasUpdateManager = canvasUpdateManager;
        this.undoRedo = undoRedo;
        this.textManager = textManager;
        this.searchManager = searchManager;
        this.cursorManager = cursorManager;
        this.textActionManager = textActionManager;
        this.selectionRenderer = selectionRenderer;
        this.selectionManager = selectionManager;
        this.eventsManager = eventsManager;
        this.longestLineManager = longestLineManager;
    }

    public SearchResult ReplaceAll(string word, string replaceWord, bool matchCase, bool wholeWord)
    {
        if (string.IsNullOrEmpty(word))
            return SearchResult.InvalidInput;

        replaceWord ??= "";
        selectionManager.ClearSelection();

        SearchParameter searchParameter = new SearchParameter(word, wholeWord, matchCase);

        //use the replacement literally, "$1" or "$&" must not be treated as regex substitutions
        replaceWord = LineEndings.CleanLineEndings(replaceWord, textManager.LineEnding);
        MatchEvaluator replacement = _ => replaceWord;
        bool multilineReplacement = replaceWord.Contains(textManager.NewLineCharacter, StringComparison.Ordinal);

        bool isFound = false;
        undoRedo.RecordUndoAction(() =>
        {
            if (!multilineReplacement)
            {
                for (int i = 0; i < textManager.LinesCount; i++)
                {
                    if (textManager.totalLines[i].Contains(searchParameter))
                    {
                        isFound = true;
                        textManager.SetLineText(i, Regex.Replace(textManager.totalLines[i], searchParameter.SearchExpression, replacement));
                    }
                }
                //the cursor line can be shorter now
                selectionManager.ClampToDocument();
                return;
            }

            //the replacement adds lines, so every replaced line has to be split again
            var newLines = new List<string>(textManager.LinesCount);
            for (int i = 0; i < textManager.LinesCount; i++)
            {
                string line = textManager.totalLines[i];
                if (line.Contains(searchParameter))
                {
                    isFound = true;
                    newLines.AddRange(Regex.Replace(line, searchParameter.SearchExpression, replacement).Split(textManager.NewLineCharacter));
                }
                else
                    newLines.Add(line);
            }

            if (isFound)
            {
                textManager.totalLines.Clear();
                textManager.InsertOrAddRange(newLines, 0);
            }
            selectionManager.ClampToDocument();
            //the redo count gets clamped to the actual number of lines after the action
        }, 0, textManager.LinesCount, multilineReplacement ? int.MaxValue : textManager.LinesCount);

        eventsManager.CallTextChanged();

        if (isFound)
            longestLineManager.needsRecalculation = true;

        canvasUpdateManager.UpdateText();
        return isFound ? SearchResult.Found : SearchResult.NotFound;
    }

    public InternSearchResult ReplaceNext(string replaceWord)
    {
        if (!searchManager.IsSearchOpen)
            return new InternSearchResult(SearchResult.SearchNotOpened, null);

        replaceWord ??= "";
        var res = searchManager.FindNext(cursorManager.currentCursorPosition);
        if (res.Selection != null)
        {
            selectionManager.SetSelection(res.Selection);

            int splittedTextLength = replaceWord.Contains(textManager.NewLineCharacter, StringComparison.Ordinal)
                ? replaceWord.CountLines(textManager.NewLineCharacter)
                : 1;

            undoRedo.RecordUndoAction(() =>
            {
                selectionManager.Replace(replaceWord);
            }, selectionManager.currentTextSelection, splittedTextLength);

            eventsManager.CallTextChanged();
            longestLineManager.needsRecalculation = true;

            selectionManager.ClearSelection();
        }
        return res;
    }
}
