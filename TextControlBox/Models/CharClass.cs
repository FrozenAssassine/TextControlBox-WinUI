namespace TextControlBoxNS.Models;

internal enum CharClass
{
    Whitespace,
    Word,
    Symbol
}

internal static class CharClassHelper
{
    public static CharClass GetCharClass(char c)
    {
        if (char.IsWhiteSpace(c))
            return CharClass.Whitespace;

        if (char.IsLetterOrDigit(c) || c == '_')
            return CharClass.Word;

        return CharClass.Symbol;
    }
}
