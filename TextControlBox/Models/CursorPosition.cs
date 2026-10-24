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

    /// <summary>
    /// Gets the character position of the cursor within the current line.
    /// </summary>
    public int CharacterPosition { get; internal set; } = 0;
    /// <summary>
    /// Gets the line number in which the cursor is currently positioned.
    /// </summary>
    public int LineNumber { get; internal set; } = 0;
    /// <summary>
    /// Gets whether the cursor has trailing affinity (e.g. at the trailing edge of a character before a wrap).
    /// </summary>
    public bool IsTrailing { get; internal set; } = false;

    internal CursorPosition(CursorPosition curPos)
    {
        SetChangeValues(curPos.LineNumber, curPos.CharacterPosition, curPos.IsTrailing);
    }
    internal CursorPosition(int characterPosition = 0, int lineNumber = 0, bool isTrailing = false)
    {
        SetChangeValues(lineNumber, characterPosition, isTrailing);
    }

    internal void SetChangeValues(CursorPosition curPos)
    {
        if (curPos == null)
        {
            this.IsNull = true;
            return;
        }

        this.LineNumber = curPos.LineNumber;
        this.CharacterPosition = curPos.CharacterPosition;
        this.IsTrailing = curPos.IsTrailing;
        this.IsNull = curPos.IsNull;
    }

    internal void SetChangeValues(int line, int cursor, bool isTrailing = false)
    {
        this.IsNull = false;
        this.LineNumber = line;
        this.CharacterPosition = cursor;
        this.IsTrailing = isTrailing;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Line: {LineNumber}, Char: {CharacterPosition}" + (IsTrailing ? " (Trailing)" : "");
    }

    internal bool IsNull { get; set; } = false;
}
