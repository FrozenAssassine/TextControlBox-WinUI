using System.Text.Json.Serialization;
using System.Drawing;
using System.Text.RegularExpressions;
using TextControlBoxNS.Extensions;
using TextControlBoxNS.Models;

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

    // A pattern with heavy backtracking would otherwise block the UI thread on every redraw of a long line.
    // The limit has to leave room for a slow machine, it only exists to stop runaway patterns.
    internal static readonly System.TimeSpan MatchTimeout = System.TimeSpan.FromMilliseconds(100);
    private string compiledPattern;

    [JsonIgnore]
    internal Regex PrecompiledRegex { get; private set; }

    // Initialize regex when setting the pattern
    internal void CompileRegex()
    {
        if (string.IsNullOrEmpty(Pattern))
            return;

        //the languages are shared, do not compile the same pattern again for every textbox
        if (PrecompiledRegex != null && compiledPattern == Pattern)
            return;

        PrecompiledRegex = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.Multiline, MatchTimeout);
        compiledPattern = Pattern;

        // The first match of a compiled regex includes generating its code, which takes longer than the
        // match timeout for big patterns. Do that here, so the first drawn frame is not cut off by it.
        try
        {
            PrecompiledRegex.IsMatch("warm up");
        }
        catch (RegexMatchTimeoutException)
        {
        }
    }
}
