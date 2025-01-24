using System;
using System.Runtime.CompilerServices;

namespace TextControlBoxNS.Models;

internal class TextSelection
{
    public bool IsNull = true;
    public TextSelection()
    {
        renderedIndex = 0;
        renderedLength = 0;
        StartPosition.IsNull = true;
        EndPosition.IsNull = true;
        this.IsNull = false;
    }
    public TextSelection(int index, int length, int startLine, int startChar, int endLine, int endChar)
    {
        renderedIndex = index;
        renderedLength = length;
        this.IsNull = false;

        StartPosition.SetChangeValues(startLine, startChar);
        EndPosition.SetChangeValues(endLine, endChar);
    }
    public TextSelection(int index = 0, int length = 0, CursorPosition startPosition = null, CursorPosition endPosition = null)
    {
        renderedIndex = index;
        renderedLength = length;
        this.IsNull = false;

        if(StartPosition != null)
            StartPosition.SetChangeValues(startPosition);
        
        if(endPosition != null)
            EndPosition.SetChangeValues(endPosition);
    }
    public TextSelection(CursorPosition startPosition = null, CursorPosition endPosition = null)
    {
        this.IsNull = false;

        if(startPosition != null)
            StartPosition.SetChangeValues(startPosition);
        
        if(endPosition != null)
            EndPosition.SetChangeValues(endPosition);
    }
    public TextSelection(TextSelection textSelection)
    {
        this.IsNull = false;

        if (textSelection.StartPosition != null)
            StartPosition.SetChangeValues(textSelection.StartPosition);
        if(textSelection.EndPosition != null)
            EndPosition.SetChangeValues(textSelection.EndPosition);

        renderedIndex = textSelection.renderedIndex;
        renderedLength = textSelection.renderedLength;
    }

    internal int renderedIndex { get; set; }
    internal int renderedLength { get; set; }

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

    internal void SetChangedValues(CursorPosition start, CursorPosition end)
    {
        this.StartPosition.SetChangeValues(start);
        this.EndPosition.SetChangeValues(end);
        this.IsNull = false;
    }


    internal int GetMinLine()
    {
        return Math.Min(StartPosition.LineNumber, EndPosition.LineNumber);
    }
    internal int GetMaxLine()
    {
        return Math.Max(StartPosition.LineNumber, EndPosition.LineNumber);
    }
    internal int GetMinChar()
    {
        return Math.Min(StartPosition.CharacterPosition, EndPosition.CharacterPosition);
    }
    internal int GetMaxChar()
    {
        return Math.Max(StartPosition.CharacterPosition, EndPosition.CharacterPosition);
    }
}
