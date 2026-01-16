using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Xaml;
using Newtonsoft.Json;
using System;
using TextControlBoxNS.Models;
using Windows.UI.Text;
using static TextControlBoxNS.Core.Text.TextManager;

namespace TextControlBoxNS.Core.Renderer;

internal class SyntaxHighlightingRenderer
{
    public readonly static FontWeight BoldFont = new FontWeight(600);
    public const FontStyle ItalicFont = FontStyle.Italic;

    public static void UpdateSyntaxHighlighting(LineSliceResult lineSliceResult, string newLineCharacter, CanvasTextLayout drawnTextLayout, ApplicationTheme theme, SyntaxHighlightLanguage syntaxHighlightingLanguage, bool syntaxHighlighting)
    {
        if (!syntaxHighlighting)
            return;

        bool isLightTheme = theme == ApplicationTheme.Light;

        if (syntaxHighlightingLanguage?.HighlightRules != null && syntaxHighlightingLanguage.HighlightRules.Length > 0)
        {
            foreach (var rule in syntaxHighlightingLanguage.HighlightRules)
            {
                foreach (var span in rule.GetHighlights(lineSliceResult.Lines, lineSliceResult.Text, newLineCharacter))
                {
                    ApplyHighlightSpan(drawnTextLayout, span, isLightTheme);
                }
            }
        }
        else if (syntaxHighlightingLanguage?.Highlights != null)
        {
            foreach (var highlight in syntaxHighlightingLanguage.Highlights)
            {
                if (highlight.PrecompiledRegex == null) return;

                var color = isLightTheme ? highlight.ColorLight_Clr : highlight.ColorDark_Clr;

                foreach (var match in highlight.PrecompiledRegex.EnumerateMatches(lineSliceResult.Text))
                {
                    int index = match.Index;
                    int length = match.Length;

                    drawnTextLayout.SetColor(index, length, color);

                    if (highlight.CodeStyle != null)
                    {
                        if (highlight.CodeStyle.Italic)
                            drawnTextLayout.SetFontStyle(index, length, ItalicFont);
                        if (highlight.CodeStyle.Bold)
                            drawnTextLayout.SetFontWeight(index, length, BoldFont);
                        if (highlight.CodeStyle.Underlined)
                            drawnTextLayout.SetUnderline(index, length, true);
                    }
                }
            }
        }
    }

    private static void ApplyHighlightSpan(CanvasTextLayout drawnTextLayout, HighlightSpan span, bool isLightTheme)
    {
        var color = isLightTheme ? span.ColorLight : span.ColorDark;
        drawnTextLayout.SetColor(span.Start, span.Length, color);

        if (span.Style != null)
        {
            if (span.Style.Italic)
                drawnTextLayout.SetFontStyle(span.Start, span.Length, ItalicFont);
            if (span.Style.Bold)
                drawnTextLayout.SetFontWeight(span.Start, span.Length, BoldFont);
            if (span.Style.Underlined)
                drawnTextLayout.SetUnderline(span.Start, span.Length, true);
        }
    }

    public static JsonLoadResult GetSyntaxHighlightingFromJson(string json)
    {
        try
        {
            var jsonHighlight = JsonConvert.DeserializeObject<JsonSyntaxHighlighting>(json);
            //Apply the filter as an array
            var highlightLanguage = new SyntaxHighlightLanguage
            {
                Author = jsonHighlight.Author,
                Description = jsonHighlight.Description,
                Highlights = jsonHighlight.Highlights,
                Name = jsonHighlight.Name,
                Filter = jsonHighlight.Filter.Split("|", StringSplitOptions.RemoveEmptyEntries),
            };
            return new JsonLoadResult(true, highlightLanguage);
        }
        catch (JsonReaderException)
        {
            return new JsonLoadResult(false, null);
        }
        catch (JsonSerializationException)
        {
            return new JsonLoadResult(false, null);
        }
    }
}
