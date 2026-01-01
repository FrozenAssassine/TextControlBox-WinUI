using System;
using System.Text.RegularExpressions;

namespace TextControlBoxNS.Core.Text;

internal class LineEndings
{
    public static string LineEndingToString(LineEnding lineEnding)
    {
        return
            lineEnding == LineEnding.LF ? "\n" :
            lineEnding == LineEnding.CRLF ? "\r\n" :
            "\r";
    }
    public static LineEnding FindLineEnding(string text)
    {
        if (text.IndexOf("\r\n", StringComparison.Ordinal) > -1)
            return LineEnding.CRLF;
        else if (text.IndexOf("\n", StringComparison.Ordinal) > -1)
            return LineEnding.LF;
        else if (text.IndexOf("\r", StringComparison.Ordinal) > -1)
            return LineEnding.CR;
        return LineEnding.CRLF;
    }
    public static string CleanLineEndings(string text, LineEnding lineEnding)
    {
        return Regex.Replace(text, "(\r\n|\r|\n)", LineEndingToString(lineEnding));
    }

    public static string RemoveLineEndings(string text)
    {
        return Regex.Replace(text, "(\r\n|\r|\n)", "");
    }


    public static bool ContainsLineEndings(string text)
    {
        return Regex.Match(text, "(\r\n|\r|\n)").Success;
    }
}
