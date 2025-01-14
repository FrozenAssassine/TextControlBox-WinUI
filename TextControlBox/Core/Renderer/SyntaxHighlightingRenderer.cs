using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Xaml;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using Windows.UI.Text;

namespace TextControlBoxNS.Core.Renderer;

internal class SyntaxHighlightingRenderer
{
    public static FontWeight BoldFont = new FontWeight { Weight = 600 };
    public static FontStyle ItalicFont = FontStyle.Italic;

    public static void UpdateSyntaxHighlighting(CanvasTextLayout drawnTextLayout, ApplicationTheme theme, SyntaxHighlightLanguage codeLanguage, bool syntaxHighlighting, string renderedText)
    {
        if (codeLanguage == null || !syntaxHighlighting)
            return;

        var highlights = codeLanguage.Highlights;
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

    public static JsonLoadResult GetCodeLanguageFromJson(string json)
    {
        try
        {
            var jsonCodeLanguage = JsonConvert.DeserializeObject<JsonCodeLanguage>(json);
            //Apply the filter as an array
            var codelanguage = new SyntaxHighlightLanguage
            {
                Author = jsonCodeLanguage.Author,
                Description = jsonCodeLanguage.Description,
                Highlights = jsonCodeLanguage.Highlights,
                Name = jsonCodeLanguage.Name,
                Filter = jsonCodeLanguage.Filter.Split("|", StringSplitOptions.RemoveEmptyEntries),
            };
            return new JsonLoadResult(true, codelanguage);
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
