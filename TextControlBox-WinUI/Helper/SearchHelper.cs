using Collections.Pooled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TextControlBox.Extensions;
using TextControlBox.Text;

namespace TextControlBox.Helper
{
    internal class SearchHelper
    {
        public int CurrentSearchLine = 0;
        public int CurrentSearchArrayIndex = 0;
        public int OldSearchArrayIndex = 0;
        public int[] MatchingSearchLines = null;
        public bool IsSearchOpen = false;
        public SearchParameter SearchParameter = null;
        public MatchCollection CurrentLineMatches = null;
        private int RegexIndexInLine = 0;

        private int CheckIndexValue(int i)
        {
            return i >= MatchingSearchLines.Length ? MatchingSearchLines.Length - 1 : i < 0 ? 0 : i;
        }

        public InternSearchResult FindNext(PooledList<string> TotalLines, CursorPosition CursorPosition)
        {
            if (IsSearchOpen && MatchingSearchLines != null && MatchingSearchLines.Length > 0)
            {
                CurrentSearchLine = CursorPosition.LineNumber;
                int indexInList = Array.IndexOf(MatchingSearchLines, CurrentSearchLine);
                //When in a line without a match search for the next line with a match
                if (indexInList == -1)
                {
                    for (int i = 0; i < MatchingSearchLines.Length; i++)
                    {
                        if (MatchingSearchLines[i] > CurrentSearchLine)
                        {
                            CurrentSearchArrayIndex = i - 1 < 0 ? 0 : i - 1;
                            CurrentSearchLine = MatchingSearchLines[CurrentSearchArrayIndex];
                            break;
                        }
                    }
                }
                else
                {
                    CurrentSearchArrayIndex = indexInList;
                    CurrentSearchLine = MatchingSearchLines[CurrentSearchArrayIndex];
                    if (OldSearchArrayIndex != CurrentSearchArrayIndex)
                    {
                        OldSearchArrayIndex = CurrentSearchArrayIndex;
                        CurrentLineMatches = null;
                    }
                }

                //Search went through all matches in the current line:
                if (CurrentLineMatches == null || RegexIndexInLine >= CurrentLineMatches.Count)
                {
                    RegexIndexInLine = 0;
                    //back at start
                    if (CurrentSearchLine < MatchingSearchLines[MatchingSearchLines.Length - 1])
                    {
                        CurrentSearchArrayIndex++;
                        CurrentSearchLine = CursorPosition.LineNumber = MatchingSearchLines[CurrentSearchArrayIndex];
                        CurrentLineMatches = Regex.Matches(TotalLines[CurrentSearchLine], SearchParameter.SearchExpression);
                    }
                    else
                        return new InternSearchResult(SearchResult.ReachedEnd, null);
                }

                if (RegexIndexInLine > CurrentLineMatches.Count)
                    RegexIndexInLine = CurrentLineMatches.Count - 1;
                if (RegexIndexInLine < 0)
                    RegexIndexInLine = 0;

                RegexIndexInLine++;
                if (RegexIndexInLine > CurrentLineMatches.Count || RegexIndexInLine < 0)
                    return new InternSearchResult(SearchResult.NotFound, null);

                return new InternSearchResult(SearchResult.Found, new TextSelection(
                    new CursorPosition(CurrentLineMatches[RegexIndexInLine - 1].Index, CursorPosition.LineNumber),
                    new CursorPosition(CurrentLineMatches[RegexIndexInLine - 1].Index + CurrentLineMatches[RegexIndexInLine - 1].Length, CursorPosition.LineNumber)));
            }
            return new InternSearchResult(SearchResult.NotFound, null);
        }
        public InternSearchResult FindPrevious(PooledList<string> Lines, CursorPosition CursorPosition)
        {
            if (IsSearchOpen && MatchingSearchLines != null)
            {
                //Find the next linnenumber with a match if the line is not in the array of matching lines
                CurrentSearchLine = CursorPosition.LineNumber;
                int indexInList = Array.IndexOf(MatchingSearchLines, CurrentSearchLine);
                if (indexInList == -1)
                {
                    //Find the first line with matches which is smaller than the current line:
                    var lines = MatchingSearchLines.Where(x => x < CurrentSearchLine);
                    if (lines.Count() < 1)
                    {
                        return new InternSearchResult(SearchResult.ReachedBegin, null);
                    }

                    CurrentSearchArrayIndex = Array.IndexOf(MatchingSearchLines, lines.Last());
                    CurrentSearchLine = MatchingSearchLines[CurrentSearchArrayIndex];
                    CurrentLineMatches = null;
                }
                else
                {
                    CurrentSearchArrayIndex = indexInList;
                    CurrentSearchLine = MatchingSearchLines[CurrentSearchArrayIndex];

                    if (OldSearchArrayIndex != CurrentSearchArrayIndex)
                    {
                        OldSearchArrayIndex = CurrentSearchArrayIndex;
                        CurrentLineMatches = null;
                    }
                }

                //Search went through all matches in the current line:
                if (CurrentLineMatches == null || RegexIndexInLine < 0)
                {
                    //back at start
                    if (CurrentSearchLine == MatchingSearchLines[0])
                    {
                        return new InternSearchResult(SearchResult.ReachedBegin, null);
                    }
                    if (CurrentSearchLine < MatchingSearchLines[MatchingSearchLines.Length - 1])
                    {
                        CurrentSearchLine = CursorPosition.LineNumber = MatchingSearchLines[CheckIndexValue(CurrentSearchArrayIndex - 1)];
                        CurrentLineMatches = Regex.Matches(Lines[CurrentSearchLine], SearchParameter.SearchExpression);
                        RegexIndexInLine = CurrentLineMatches.Count - 1;
                        CurrentSearchArrayIndex--;
                    }
                }

                if (CurrentLineMatches == null)
                    return new InternSearchResult(SearchResult.NotFound, null);

                if (RegexIndexInLine > CurrentLineMatches.Count)
                    RegexIndexInLine = CurrentLineMatches.Count - 1;
                if (RegexIndexInLine < 0)
                    RegexIndexInLine = 0;

                //RegexIndexInLine--;
                if (RegexIndexInLine >= CurrentLineMatches.Count || RegexIndexInLine < 0)
                    return new InternSearchResult(SearchResult.NotFound, null);

                return new InternSearchResult(SearchResult.Found, new TextSelection(
                    new CursorPosition(CurrentLineMatches[RegexIndexInLine].Index, CursorPosition.LineNumber),
                    new CursorPosition(CurrentLineMatches[RegexIndexInLine].Index + CurrentLineMatches[RegexIndexInLine--].Length, CursorPosition.LineNumber)));
            }
            return new InternSearchResult(SearchResult.NotFound, null);
        }

        public void UpdateSearchLines(PooledList<string> TotalLines)
        {
            MatchingSearchLines = FindIndexes(TotalLines);
        }

        public SearchResult BeginSearch(PooledList<string> TotalLines, string Word, bool MatchCase, bool WholeWord)
        {
            SearchParameter = new SearchParameter(Word, WholeWord, MatchCase);
            UpdateSearchLines(TotalLines);

            if (Word == "" || Word == null)
                return SearchResult.InvalidInput;

            //A result was found
            if (MatchingSearchLines.Length > 0)
            {
                IsSearchOpen = true;
            }

            return MatchingSearchLines.Length > 0 ? SearchResult.Found : SearchResult.NotFound;
        }
        public void EndSearch()
        {
            IsSearchOpen = false;
            MatchingSearchLines = null;
        }

        private int[] FindIndexes(PooledList<string> Lines)
        {
            List<int> results = new List<int>();
            for (int i = 0; i < Lines.Count; i++)
            {
                if (Lines[i].Contains(SearchParameter))
                    results.Add(i);
            };
            return results.ToArray();
        }
    }
    internal class SearchParameter
    {
        public SearchParameter(string Word, bool wholeWord = false, bool matchCase = false)
        {
            this.Word = Word;
            this.WholeWord = wholeWord;
            this.MatchCase = matchCase;

            if (wholeWord)
                SearchExpression += @"\b" + (matchCase ? "" : "(?i)") + Word + @"\b";
            else
                SearchExpression += (matchCase ? "" : "(?i)") + Word;
        }
        public bool WholeWord { get; set; }
        public bool MatchCase { get; set; }
        public string Word { get; set; }
        public string SearchExpression { get; set; } = "";
    }
    /// <summary>
    /// A result returned from the search
    /// </summary>
    public enum SearchResult
    {
        Found, NotFound, InvalidInput, ReachedBegin, ReachedEnd, SearchNotOpened
    }

    internal struct InternSearchResult
    {
        public InternSearchResult(SearchResult result, TextSelection selection)
        {
            this.result = result;
            this.selection = selection;
        }
        public TextSelection selection;
        public SearchResult result;
    }
}