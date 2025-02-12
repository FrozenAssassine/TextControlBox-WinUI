using System;
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

    public void Init(
        CanvasUpdateManager canvasUpdateManager,
        UndoRedo undoRedo,
        TextManager textManager,
        SearchManager searchManager,
        CursorManager cursorManager,
    TextActionManager textActionManager,
    SelectionRenderer selectionRenderer,
    SelectionManager selectionManager,
    EventsManager eventsManager)
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
    }

    public SearchResult ReplaceAll(string word, string replaceWord, bool matchCase, bool wholeWord)
    {
        if (!searchManager.IsSearchOpen)
            return SearchResult.SearchNotOpened;

        if (word.Length == 0 || replaceWord.Length == 0)
            return SearchResult.InvalidInput;

        SearchParameter searchParameter = new SearchParameter(word, wholeWord, matchCase);

        bool isFound = false;
        undoRedo.RecordUndoAction(() =>
        {
            for (int i = 0; i < textManager.LinesCount; i++)
            {
                if (textManager.totalLines[i].Contains(searchParameter))
                {
                    isFound = true;
                    textManager.SetLineText(i, Regex.Replace(textManager.totalLines[i], searchParameter.SearchExpression, replaceWord));
                }
            }
        }, 0, textManager.LinesCount, textManager.LinesCount);

        eventsManager.CallTextChanged();

        canvasUpdateManager.UpdateText();
        return isFound ? SearchResult.Found : SearchResult.NotFound;
    }

    public InternSearchResult ReplaceNext(string replaceWord)
    {
        if (!searchManager.IsSearchOpen)
            return new InternSearchResult(SearchResult.SearchNotOpened, null);

        var res = searchManager.FindNext(cursorManager.currentCursorPosition);
        if (res.Selection != null)
        {
            selectionManager.currentTextSelection.SetChangedValues(res.Selection);
            selectionRenderer.SetSelection(res.Selection);

            undoRedo.RecordUndoAction(() =>
            {
                selectionManager.Replace(replaceWord);
            }, selectionManager.currentTextSelection, 1);

            eventsManager.CallTextChanged();

            var start = res.Selection.StartPosition;
            selectionRenderer.SetSelection(start.LineNumber, start.CharacterPosition, start.LineNumber, start.CharacterPosition + replaceWord.Length);
        }
        return res;
    }
}
