using Newtonsoft.Json;
using System.Drawing;
using System.Text.RegularExpressions;
using TextControlBoxNS.Extensions;

namespace TextControlBoxNS;

/// <summary>
/// Represents a syntax highlight definition for a specific pattern in the text.
/// </summary>
public class SyntaxHighlights
{
    private readonly ColorConverter ColorConverter = new ColorConverter();

    /// <summary>
    /// Initializes a new instance of the SyntaxHighlights class with the specified pattern, colors, and font styles.
    /// </summary>
    /// <param name="pattern">The pattern to be highlighted in the text content.</param>
    /// <param name="colorLight">The color representation for the pattern in the light theme (e.g., "#RRGGBB" format).</param>
    /// <param name="colorDark">The color representation for the pattern in the dark theme (e.g., "#RRGGBB" format).</param>
    /// <param name="bold">true if the pattern should be displayed in bold font; otherwise, false.</param>
    /// <param name="italic">true if the pattern should be displayed in italic font; otherwise, false.</param>
    /// <param name="underlined">true if the pattern should be displayed with an underline; otherwise, false.</param>
    public SyntaxHighlights(string pattern, string colorLight, string colorDark, bool bold = false, bool italic = false, bool underlined = false)
    {
        this.Pattern = pattern;
        this.ColorDark = colorDark;
        this.ColorLight = colorLight;

        if (underlined || italic || bold)
            this.CodeStyle = new CodeFontStyle(underlined, italic, bold);
    }

    /// <summary>
    /// Gets or sets the font style for the pattern.
    /// </summary>
    public CodeFontStyle CodeStyle { get; set; } = null;

    /// <summary>
    /// Gets or sets the pattern to be highlighted in the text content.
    /// </summary>
    public string Pattern { get; set; }

    /// <summary>
    /// Gets the color representation for the pattern in the dark theme.
    /// </summary>
    public Windows.UI.Color ColorDark_Clr { get; private set; }

    /// <summary>
    /// Gets the color representation for the pattern in the light theme.
    /// </summary>
    public Windows.UI.Color ColorLight_Clr { get; private set; }

    /// <summary>
    /// Sets the color representation for the pattern in the dark theme.
    /// </summary>
    /// <param name="value">The color representation for the pattern in the dark theme (e.g., "#RRGGBB" format).</param>
    public string ColorDark
    {
        set => ColorDark_Clr = ((Color)ColorConverter.ConvertFromString(value)).ToMediaColor();
    }

    /// <summary>
    /// Sets the color representation for the pattern in the light theme.
    /// </summary>
    /// <param name="value">The color representation for the pattern in the light theme (e.g., "#RRGGBB" format).</param>
    public string ColorLight
    {
        set => ColorLight_Clr = ((Color)ColorConverter.ConvertFromString(value)).ToMediaColor();
    }

    [JsonIgnore]
    internal Regex PrecompiledRegex { get; private set; }

    // Initialize regex when setting the pattern
    internal void CompileRegex()
    {
        if (!string.IsNullOrEmpty(Pattern))
        {
            PrecompiledRegex = new Regex(Pattern, RegexOptions.Compiled);
        }
    }
}
