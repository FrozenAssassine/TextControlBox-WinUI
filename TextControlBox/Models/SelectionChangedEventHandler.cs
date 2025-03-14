﻿namespace TextControlBoxNS;

/// <summary>
/// Represents the class that will be returned by the SelectionChanged event of the TextControlBox.
/// </summary>
public class SelectionChangedEventHandler
{
    /// <summary>
    /// Represents the position of the cursor within the current line.
    /// </summary>
    public int CharacterPositionInLine { get; set; }

    /// <summary>
    /// Represents the line number where the cursor is currently located.
    /// </summary>
    public int LineNumber { get; set; }
}
