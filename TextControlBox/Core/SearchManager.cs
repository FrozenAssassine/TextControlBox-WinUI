using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Extensions;
using TextControlBoxNS.Models;

namespace TextControlBoxNS.Core;

internal class SearchManager
{
    public int[] MatchingSearchLines = null;
    public bool IsSearchOpen = false;
    public SearchParameter searchParameter = null;
    private TextManager textManager;

    public void Init(TextManager textManager)
    {
        this.textManager = textManager;
    }

    public InternSearchResult FindNext(CursorPosition cursorPosition)
    {
        if (!IsSearchOpen || MatchingSearchLines == null || MatchingSearchLines.Length == 0)
            return new InternSearchResult(SearchResult.NotFound, null);

        int startLine = cursorPosition.LineNumber;
        int startIndex = cursorPosition.CharacterPosition;

        for (int i = 0; i < MatchingSearchLines.Length; i++)
        {
            int lineNumber = MatchingSearchLines[i];
            if (lineNumber < startLine) continue;
            string line = textManager.totalLines[lineNumber];
            MatchCollection matches = Regex.Matches(line, searchParameter.SearchExpression);

            foreach (Match match in matches)
            {
                if (lineNumber > startLine || match.Index >= startIndex)
                {
                    cursorPosition.SetChangeValues(lineNumber, match.Index + match.Length);
                    return new InternSearchResult(SearchResult.Found, new TextSelection(0, 0, lineNumber, match.Index, lineNumber, match.Index + match.Length));
                }
            }
        }
        return new InternSearchResult(SearchResult.ReachedEnd, null);
    }

    public InternSearchResult FindPrevious(CursorPosition cursorPosition)
    {
        if (!IsSearchOpen || MatchingSearchLines == null || MatchingSearchLines.Length == 0)
            return new InternSearchResult(SearchResult.NotFound, null);

        int startLine = cursorPosition.LineNumber;
        int startIndex = cursorPosition.CharacterPosition;

        for (int i = MatchingSearchLines.Length - 1; i >= 0; i--)
        {
            int lineNumber = MatchingSearchLines[i];
            if (lineNumber > startLine) continue;
            string line = textManager.totalLines[lineNumber];
            MatchCollection matches = Regex.Matches(line, searchParameter.SearchExpression);

            for (int j = matches.Count - 1; j >= 0; j--)
            {
                Match match = matches[j];
                if (lineNumber < startLine || (lineNumber == startLine && match.Index < startIndex))
                {
                    cursorPosition.SetChangeValues(lineNumber, match.Index);
                    return new InternSearchResult(SearchResult.Found, new TextSelection(0, 0, lineNumber, match.Index, lineNumber, match.Index + match.Length));
                }
            }
        }
        return new InternSearchResult(SearchResult.ReachedBegin, null);
    }

    public void UpdateSearchLines()
    {
        MatchingSearchLines = FindIndexes();
    }

    public SearchResult BeginSearch(string word, bool matchCase, bool wholeWord)
    {
        searchParameter = new SearchParameter(word, wholeWord, matchCase);
        UpdateSearchLines();

        if (word == null || word.Length == 0)
        {
            IsSearchOpen = false;
            return SearchResult.InvalidInput;
        }

        if (MatchingSearchLines.Length > 0)
            IsSearchOpen = true;

        return MatchingSearchLines.Length > 0 ? SearchResult.Found : SearchResult.NotFound;
    }
    public void EndSearch()
    {
        IsSearchOpen = false;
        MatchingSearchLines = null;
    }

    private int[] FindIndexes()
    {
        List<int> results = new List<int>();
        for (int i = 0; i < textManager.totalLines.Count; i++)
        {
            if (textManager.totalLines[i].Contains(searchParameter))
                results.Add(i);
        };
        return results.ToArray();
    }
}