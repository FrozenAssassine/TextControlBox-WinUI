using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TextControlBoxNS.Models;

internal sealed class RegexHighlightRule : IHighlightRule
{
    private readonly SyntaxHighlights _highlight;

    public RegexHighlightRule(SyntaxHighlights highlight)
    {
        _highlight = highlight;
        _highlight.CompileRegex();
    }

    public List<HighlightSpan> GetHighlights(ReadOnlySpan<string> lines, string text, string newLineCharacter)
    {
        var highlights = new List<HighlightSpan>();

        if (_highlight.PrecompiledRegex == null)
            return highlights;

        try
        {
            foreach (Match match in _highlight.PrecompiledRegex.Matches(text))
            {
                highlights.Add(new HighlightSpan
                {
                    Start = match.Index,
                    Length = match.Length,
                    ColorLight = _highlight.ColorLight_Clr,
                    ColorDark = _highlight.ColorDark_Clr,
                    Style = _highlight.CodeStyle
                });
            }
        }
        catch (RegexMatchTimeoutException)
        {
            //keep the highlights found so far, the rest of this rule is skipped for this frame
        }
        return highlights;
    }
}
