using Newtonsoft.Json;
using System.Text.RegularExpressions;
using TextControlBoxNS.Models;
using Windows.Devices.Power;

namespace TextControlBoxNS;

/// <summary>
/// Represents a code language configuration used for syntax highlighting and auto-pairing in the text content.
/// </summary>
public class SyntaxHighlightLanguage
{
    /// <summary>
    /// Gets or sets the name of the code language.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the code language.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets an array of file filters for the code language.
    /// </summary>
    public string[] Filter { get; set; }

    /// <summary>
    /// Gets or sets the author of the code language definition.
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// Gets or sets an array of syntax highlights for the code language.
    /// </summary>
    public SyntaxHighlights[] Highlights { get; set; }

    /// <summary>
    /// Gets or sets an array of auto-pairing pairs for the code language.
    /// </summary>
    public AutoPairingPair[] AutoPairingPair { get; set; }

    /// <summary>
    /// Gets or sets an array of highlight rules for the code language.
    /// This is the new extensible system that supports dynamic highlighting.
    /// </summary>
    [JsonIgnore]
    public IHighlightRule[] HighlightRules { get; set; }

    internal void CompileAllRegex()
    {
        if (Highlights == null) return;

        foreach (var highlight in Highlights)
        {
            highlight.CompileRegex();
        }
    }

    /// <summary>
    /// Converts the legacy SyntaxHighlights array to the new IHighlightRule system.
    /// This allows gradual migration from the old to the new system.
    /// </summary>
    internal void ConvertToHighlightRules()
    {
        if (Highlights == null || Highlights.Length == 0)
        {
            HighlightRules = null;
            return;
        }

        var rules = new IHighlightRule[Highlights.Length];
        for (int i = 0; i < Highlights.Length; i++)
        {
            rules[i] = new RegexHighlightRule(Highlights[i]);
        }
        HighlightRules = rules;
    }
}
