using System;

namespace TextControlBoxNS.Models;

internal class TextSelection
{
    public int Index { get; private set; }
    public int Length { get; private set;}

    public int ActualStartLine { get; set; }
    public int ActualStartCharacterPos { get; set; }
    public int ActualEndLine { get; set; }
    public int ActualEndCharacterPos { get; set; }

    public int OrderedStartLine { get => Math.Min(ActualEndLine, ActualStartLine); }
    public int OrderedStartCharacterPos { get => Math.Min(ActualEndCharacterPos, ActualStartCharacterPos); }
    public int OrderedEndLine { get => Math.Max(ActualEndLine, ActualStartLine); }
    public int OrderedEndCharacterPos { get => Math.Max(ActualEndCharacterPos, ActualStartCharacterPos); }

    public void SetStartPos(int startLine, int startCharacterPos, bool clearEndPos = false)
    {
        ActualStartLine = startLine;
        ActualStartCharacterPos = startCharacterPos;

        if (clearEndPos)
        {
            ActualEndLine = ActualEndCharacterPos = -1;
        }
    }
    public void SetEndPos(int endLine, int endCharacterPos)
    {
        ActualEndLine = endLine;
        ActualEndCharacterPos = endCharacterPos;
    }

    public bool HasStartPos { get; set; }
    public bool HasEndPos { get; set; }

    public bool HasSelection { get; set; }

    public TextSelection()
    {
        Index = 0;
        Length = 0;

        ActualStartLine = 0;
        ActualStartCharacterPos = 0;
        ActualEndLine = 0;
        ActualEndCharacterPos = 0;
    }

    public TextSelection(TextSelection textSelection)
    {
        this.Index = textSelection.Index;
        this.Length = textSelection.Length;
        this.ActualEndLine = textSelection.ActualEndLine;
        this.ActualStartLine = textSelection.ActualStartLine;
        this.ActualEndCharacterPos = textSelection.ActualEndCharacterPos;
        this.ActualStartCharacterPos = textSelection.ActualStartCharacterPos;
    }

    public bool IsLineInSelection(int line)
    {
        if (this.HasStartPos && this.HasEndPos)
        {
            if (this.OrderedStartLine > this.OrderedEndLine)
                return this.OrderedStartLine < line && this.OrderedEndLine > line;
            else if (this.OrderedStartLine == this.OrderedEndLine)
                return this.OrderedStartLine != line;
            else
                return this.OrderedStartLine > line && this.OrderedEndLine < line;
        }
        return false;
    }
}
