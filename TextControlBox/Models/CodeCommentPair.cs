namespace TextControlBoxNS.Models;

public class CodeCommentPair
{
    public CodeCommentPair(string characters)
    {
        this.StartCharacter = this.EndCharacter = characters;
    }
    public CodeCommentPair(string startCharacter, string endCharacter)
    {
        this.StartCharacter = startCharacter;
        this.EndCharacter = endCharacter;
    }

    public string StartCharacter { get; set; }
    public string EndCharacter { get; set; }
}
