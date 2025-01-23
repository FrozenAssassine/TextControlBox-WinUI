namespace TextControlBoxNS.Models;

internal class TextSelection
{
    public TextSelection()
    {
        Index = 0;
        Length = 0;
        StartPosition.IsNull = true;
        EndPosition.IsNull = true;
    }
    public TextSelection(int index = 0, int length = 0, CursorPosition startPosition = null, CursorPosition endPosition = null)
    {
        Index = index;
        Length = length;

        if(StartPosition != null)
            StartPosition.SetChangeValues(startPosition);
        
        if(endPosition != null)
            EndPosition.SetChangeValues(endPosition);
    }
    public TextSelection(CursorPosition startPosition = null, CursorPosition endPosition = null)
    {
        if(startPosition != null)
            StartPosition.SetChangeValues(startPosition);
        
        if(endPosition != null)
            EndPosition.SetChangeValues(endPosition);
    }
    public TextSelection(TextSelection textSelection)
    {
        if(textSelection.StartPosition != null)
            StartPosition.SetChangeValues(textSelection.StartPosition);
        if(textSelection.EndPosition != null)
            EndPosition.SetChangeValues(textSelection.EndPosition);

        Index = textSelection.Index;
        Length = textSelection.Length;
    }

    public int Index { get; set; }
    public int Length { get; set; }

    public CursorPosition StartPosition { get; private set; } = new CursorPosition();
    public CursorPosition EndPosition { get; private set; } = new CursorPosition();

    public bool IsLineInSelection(int line)
    {
        if (!this.StartPosition.IsNull && !this.EndPosition.IsNull)
        {
            if (this.StartPosition.LineNumber > this.EndPosition.LineNumber)
                return this.StartPosition.LineNumber < line && this.EndPosition.LineNumber > line;
            else if (this.StartPosition.LineNumber == this.EndPosition.LineNumber)
                return this.StartPosition.LineNumber != line;
            else
                return this.StartPosition.LineNumber > line && this.EndPosition.LineNumber < line;
        }
        return false;
    }

}
