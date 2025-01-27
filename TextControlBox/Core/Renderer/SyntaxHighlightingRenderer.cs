using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Xaml;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using Windows.UI.Text;

namespace TextControlBoxNS.Core.Renderer;

internal class SyntaxHighlightingRenderer
{
    public readonly static FontWeight BoldFont = new FontWeight(600);
    public const FontStyle ItalicFont = FontStyle.Italic;

    public static void UpdateSyntaxHighlighting(CanvasTextLayout drawnTextLayout, ApplicationTheme theme, SyntaxHighlightLanguage syntaxHighlightingLanguage, bool syntaxHighlighting, string renderedText)
    {
        if (syntaxHighlightingLanguage == null || !syntaxHighlighting)
            return;

        var highlights = syntaxHighlightingLanguage.Highlights;
        for (int i = 0; i < highlights.Length; i++)
        {
            var matches = Regex.Matches(renderedText, highlights[i].Pattern, RegexOptions.Compiled);
            var highlight = highlights[i];
            var color = theme == ApplicationTheme.Light ? highlight.ColorLight_Clr : highlight.ColorDark_Clr;

            for (int j = 0; j < matches.Count; j++)
            {
                var match = matches[j];
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
