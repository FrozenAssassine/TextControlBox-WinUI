namespace TextControlBoxNS.Models;

internal struct InternSearchResult
{
    public InternSearchResult(SearchResult result, TextSelection selection)
    {
        Result = result;
        Selection = selection;
    }

    public TextSelection Selection;
    public SearchResult Result;
}
