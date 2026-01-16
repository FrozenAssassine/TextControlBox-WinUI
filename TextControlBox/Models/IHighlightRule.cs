using Collections.Pooled;
using System;
using System.Collections.Generic;
using TextControlBoxNS;

/// <summary>
/// Defines a syntax highlighting rule.
/// Implementations analyze text and return highlight spans.
/// </summary>
public interface IHighlightRule
{
    /// <summary>
    /// Analyzes the given text and returns all highlight spans.
    /// </summary>
    /// <param name="lines">
    /// The text split into individual lines (read-only).
    /// </param>
    /// <param name="text">
    /// The full text content as a single string.
    /// </param>
    /// <param name="newLineCharacter">
    /// The newline character used in the text (e.g. "\n" or "\r\n" or "\r").
    /// </param>
    /// <returns>
    /// A list of highlight spans describing where and how text should be highlighted.
    /// </returns>
    public List<HighlightSpan> GetHighlights(
        ReadOnlySpan<string> lines,
        string text,
        string newLineCharacter
    );
}

/// <summary>
/// Represents a single highlighted section of text.
/// </summary>
public struct HighlightSpan
{
    /// <summary>
    /// The start index of the highlighted text (absolute index in the full text).
    /// </summary>
    public int Start { get; init; }

    /// <summary>
    /// The length of the highlighted text.
    /// </summary>
    public int Length { get; init; }

    /// <summary>
    /// Text color used in light theme.
    /// </summary>
    public Windows.UI.Color ColorLight { get; init; }

    /// <summary>
    /// Text color used in dark theme.
    /// </summary>
    public Windows.UI.Color ColorDark { get; init; }

    /// <summary>
    /// Font style applied to the highlighted text (e.g. bold, italic).
    /// </summary>
    public CodeFontStyle Style { get; init; }
}