using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace TextControlBoxNS;

/// <summary>
/// Represents the design settings for the textbox.
/// </summary>
public class TextControlBoxDesign
{
    /// <summary>
    /// Create an instance of the TextControlBoxDesign from a given design
    /// </summary>
    /// <param name="design">The design to create a new instance from</param>
    public TextControlBoxDesign(TextControlBoxDesign design)
    {
        this.Background = design.Background;
        this.TextColor = design.TextColor;
        this.SelectionColor = design.SelectionColor;
        this.CursorColor = design.CursorColor;
        this.LineHighlighterColor = design.LineHighlighterColor;
        this.LineNumberColor = design.LineNumberColor;
        this.LineNumberBackground = design.LineNumberBackground;
        this.SearchHighlightColor = design.SearchHighlightColor;
    }

    /// <summary>
    /// Create a new instance of the TextControlBoxDesign class
    /// </summary>
    /// <param name="background">The background color of the textbox</param>
    /// <param name="textColor">The color of the text</param>
    /// <param name="selectionColor">The color of the selection</param>
    /// <param name="cursorColor">The color of the cursor</param>
    /// <param name="lineHighlighterColor">The color of the linehighlighter</param>
    /// <param name="lineNumberColor">The color of the linenumber</param>
    /// <param name="lineNumberBackground">The background color of the linenumbers</param>
    /// <param name="searchHighlightColor">The color of the search highlights</param>
    public TextControlBoxDesign(Brush background, Color textColor, Color selectionColor, Color cursorColor, Color lineHighlighterColor, Color lineNumberColor, Color lineNumberBackground, Color searchHighlightColor, Color InvisibleCharacterColor)
    {
        this.Background = background;
        this.TextColor = textColor;
        this.SelectionColor = selectionColor;
        this.CursorColor = cursorColor;
        this.LineHighlighterColor = lineHighlighterColor;
        this.LineNumberColor = lineNumberColor;
        this.LineNumberBackground = lineNumberBackground;
        this.SearchHighlightColor = searchHighlightColor;
        this.InvisibleCharacterColor = InvisibleCharacterColor;
    }

    /// <summary>
    /// Gets or sets the background color of the textbox.
    /// </summary>
    public Brush Background { get; set; }

    /// <summary>
    /// Gets or sets the text color of the textbox.
    /// </summary>
    public Color TextColor { get; set; }

    /// <summary>
    /// Gets or sets the color of the selected text in the textbox.
    /// </summary>
    public Color SelectionColor { get; set; }

    /// <summary>
    /// Gets or sets the color of the cursor in the textbox.
    /// </summary>
    public Color CursorColor { get; set; }

    /// <summary>
    /// Gets or sets the color of the line highlighter in the textbox.
    /// </summary>
    public Color LineHighlighterColor { get; set; }

    /// <summary>
    /// Gets or sets the color of the line numbers in the textbox.
    /// </summary>
    public Color LineNumberColor { get; set; }

    /// <summary>
    /// Gets or sets the background color of the line numbers in the textbox.
    /// </summary>
    public Color LineNumberBackground { get; set; }

    /// <summary>
    /// Gets or sets the color used to highlight search results in the textbox.
    /// </summary>
    public Color SearchHighlightColor { get; set; }

    /// <summary>
    /// Gets or sets the color used to highlight invisible characters like tabs and spaces.
    /// </summary>
    public Color InvisibleCharacterColor { get; set; }
}
