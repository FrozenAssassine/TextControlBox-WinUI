using System.Text.RegularExpressions;

namespace TextControlBoxNS.Models;
internal class SearchParameter
{
    public SearchParameter(string word, bool wholeWord = false, bool matchCase = false)
    {
        Word = word;
        WholeWord = wholeWord;
        MatchCase = matchCase;

        if (wholeWord)
            SearchExpression += @"\b" + (matchCase ? "" : "(?i)") + Regex.Escape(word) + @"\b";
        else
            SearchExpression += (matchCase ? "" : "(?i)") + Regex.Escape(word);
    }

    public bool WholeWord { get; set; }
    public bool MatchCase { get; set; }
    public string Word { get; set; }
    public string SearchExpression { get; set; } = "";
}
