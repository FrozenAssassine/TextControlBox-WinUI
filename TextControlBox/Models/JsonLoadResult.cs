namespace TextControlBoxNS;

/// <summary>
/// Represents the result of a JSON load operation for a code language in the textbox.
/// </summary>
public class JsonLoadResult
{
    /// <summary>
    /// Initializes a new instance of the JsonLoadResult class with the specified loading status and Syntaxhighlighting.
    /// </summary>
    /// <param name="succeed">true if the loading operation succeeded; otherwise, false.</param>
    /// <param name="syntaxHighlighting">The Syntaxhighlighting loaded from the JSON data.</param>
    public JsonLoadResult(bool succeed, SyntaxHighlightLanguage syntaxHighlighting)
    {
        this.Succeed = succeed;
        this.SyntaxHighlighting = syntaxHighlighting;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the loading operation succeeded.
    /// </summary>
    public bool Succeed { get; set; }

    /// <summary>
    /// Gets or sets the Syntaxhighlighting that was loaded from the JSON data.
    /// </summary>
    public SyntaxHighlightLanguage SyntaxHighlighting { get; set; }
}
