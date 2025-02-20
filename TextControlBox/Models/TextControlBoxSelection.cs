using TextControlBoxNS.Core.Selection;

namespace TextControlBoxNS.Models;

/// <summary>
/// Represents a text selection within a TextControlBox, storing start and end positions.
/// </summary>
public struct TextControlBoxSelection
{
    internal TextControlBoxSelection(TextSelection selection)
    {
        this.StartLinePos = selection.StartPosition.LineNumber;
        this.EndLinePos = selection.EndPosition.LineNumber;
        this.StartCharacterPos = selection.StartPosition.CharacterPosition;
        this.EndCharacterPos = selection.EndPosition.CharacterPosition;
    }

    internal TextControlBoxSelection(SelectionManager selectionManger)
    {
        var ordered = selectionManger.OrderTextSelectionSeparated();
        this.StartLinePos = ordered.startLine;
        this.EndLinePos = ordered.endLine;
        this.StartCharacterPos = ordered.startChar;
        this.EndCharacterPos = ordered.endChar;
    }

    /// <summary>
    /// Gets or sets the starting line position of the selection.
    /// </summary>
    public int StartLinePos { get; set; }

    /// <summary>
    /// Gets or sets the ending line position of the selection.
    /// </summary>
    public int EndLinePos { get; set; }

    /// <summary>
    /// Gets or sets the starting character position within the line.
    /// </summary>
    public int StartCharacterPos { get; set; }

    /// <summary>
    /// Gets or sets the ending character position within the line.
    /// </summary>
    public int EndCharacterPos { get; set; }
}
