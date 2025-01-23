namespace TextControlBoxNS;

/// <summary>
/// Represents the position of the cursor in the textbox.
/// </summary>
/// <remarks>
/// The CursorPosition class stores the position of the cursor within the textbox.
/// It consists of two properties: CharacterPosition and LineNumber.
/// The CharacterPosition property indicates the index of the cursor within the current line (zero-based index).
/// The LineNumber property represents the line number on which the cursor is currently positioned (zero-based index).
/// </remarks>
public class CursorPosition
{
    internal CursorPosition(int characterPosition = 0, int lineNumber = 0)
    {
        this.CharacterPosition = characterPosition;
        this.LineNumber = lineNumber;
    }
    internal CursorPosition(CursorPosition currentCursorPosition)
    {
        if (currentCursorPosition == null)
            return;

        this.CharacterPosition = currentCursorPosition.CharacterPosition;
        this.LineNumber = currentCursorPosition.LineNumber;
    }
    /// <summary>
    /// Gets the character position of the cursor within the current line.
    /// </summary>
    public int CharacterPosition { get; internal set; } = 0;
    /// <summary>
    /// Gets the line number in which the cursor is currently positioned.
    /// </summary>
    public int LineNumber { get; internal set; } = 0;

    internal void AddToCharacterPos(int add)
    {
        CharacterPosition += add;
    }
    internal void SubtractFromCharacterPos(int subtract)
    {
        CharacterPosition -= subtract;
        if (CharacterPosition < 0)
            CharacterPosition = 0;
    }
    internal void SetChangeValues(CursorPosition curPos)
    {
        this.LineNumber = curPos.LineNumber;
        this.CharacterPosition = curPos.CharacterPosition;
    }
    internal static CursorPosition ChangeLineNumber(CursorPosition currentCursorPosition, int lineNumber)
    {
        return new CursorPosition(currentCursorPosition.CharacterPosition, lineNumber);
    }

    internal void SetChangeValues(int line, int cursor)
    {
        this.LineNumber = line;
        this.CharacterPosition = cursor;
    }

    internal bool IsNull = false;
}
