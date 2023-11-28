using System;
using System.Text.RegularExpressions;

namespace TextControlBox.Text
{
    internal class LineEndings
    {
        public static string LineEndingToString(LineEnding LineEnding)
        {
            return LineEnding == LineEnding.LF ? "\n" :
                LineEnding == LineEnding.CRLF ? "\r\n" : "\r";
        }
        public static LineEnding FindLineEnding(string Text)
        {
            if (Text.IndexOf("\r\n", StringComparison.Ordinal) > -1)
            {
                return LineEnding.CRLF;
            }
            else if (Text.IndexOf("\n", StringComparison.Ordinal) > -1)
            {
                return LineEnding.LF;
            }
            else if (Text.IndexOf("\r", StringComparison.Ordinal) > -1)
            {
                return LineEnding.CR;
            }
            else
            {
                return LineEnding.CRLF;
            }
        }
        public static string CleanLineEndings(string Text, LineEnding lineEnding)
        {
            return Regex.Replace(Text, "(\r\n|\r|\n)", LineEndingToString(lineEnding));
        }
    }
    /// <summary>
    /// Represents the three default lineendings LF, CRLF, CR
    /// </summary>
    public enum LineEnding
    {
        LF, CRLF, CR
    }
}
