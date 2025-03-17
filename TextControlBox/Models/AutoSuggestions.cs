namespace TextControlBoxNS.Models;

public interface AutoSuggestions
{
    public string[] Suggestions { get; }
}

public class TestSuggestions : AutoSuggestions
{
    public string[] Suggestions => [
        "while",
        "for",
        "int",
        "float",
        "double",
        "string",
        "void",
        "do",
        "case",
        "switch"
    ];
}
