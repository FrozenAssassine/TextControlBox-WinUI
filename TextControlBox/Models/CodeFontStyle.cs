namespace TextControlBoxNS;

/// <summary>
/// Represents the font style settings for a code element in the text.
/// </summary>
public class CodeFontStyle
{
    /// <summary>
    /// Initializes a new instance of the CodeFontStyle class with the specified font style settings.
    /// </summary>
    /// <param name="underlined">true if the code element should be displayed with an underline; otherwise, false.</param>
    /// <param name="italic">true if the code element should be displayed in italic font; otherwise, false.</param>
    /// <param name="bold">true if the code element should be displayed in bold font; otherwise, false.</param>
    public CodeFontStyle(bool underlined, bool italic, bool bold)
    {
        this.Italic = italic;
        this.Bold = bold;
        this.Underlined = underlined;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the code element should be displayed with an underline.
    /// </summary>
    public bool Underlined { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the code element should be displayed in bold font.
    /// </summary>
    public bool Bold { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the code element should be displayed in italic font.
    /// </summary>
    public bool Italic { get; set; }
}
