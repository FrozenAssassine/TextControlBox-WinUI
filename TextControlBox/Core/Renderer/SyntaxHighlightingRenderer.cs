using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Xaml;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Extensions;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Registry;
using TextMateSharp.Themes;
using Windows.UI.Text;

namespace TextControlBoxNS.Core.Renderer;

internal class SyntaxHighlightingRenderer
{
    public readonly static FontWeight BoldFont = new FontWeight(600);
    private static ColorConverter colorConverter = new ColorConverter();
    public static void UpdateSyntaxHighlighting2(CanvasTextLayout drawnTextLayout, ApplicationTheme theme, SyntaxHighlightLanguage syntaxHighlightingLanguage, bool syntaxHighlighting, string renderedText)
    {
        if (syntaxHighlightingLanguage?.Highlights == null || !syntaxHighlighting)
            return;

        bool isLightTheme = theme == ApplicationTheme.Light;

        foreach (var highlight in syntaxHighlightingLanguage.Highlights)
        {
            if (highlight.PrecompiledRegex == null) return;

            var color = isLightTheme ? highlight.ColorLight_Clr : highlight.ColorDark_Clr;

            foreach (var match in highlight.PrecompiledRegex.EnumerateMatches(renderedText)) 
            {
                int index = match.Index;
                int length = match.Length;

                drawnTextLayout.SetColor(index, length, color);

                if (highlight.CodeStyle != null)
                {
                    if (highlight.CodeStyle.Italic)
                        drawnTextLayout.SetFontStyle(index, length, Windows.UI.Text.FontStyle.Italic);
                    if (highlight.CodeStyle.Bold)
                        drawnTextLayout.SetFontWeight(index, length, BoldFont);
                    if (highlight.CodeStyle.Underlined)
                        drawnTextLayout.SetUnderline(index, length, true);
                }
            }
        };
    }

    private static bool IsLoaded = false;

    private static IGrammar grammar;
    private static Theme textMateTheme;
    private static int globalIndex = 0;
    public static void Load()
    {
        RegistryOptions options = new RegistryOptions(ThemeName.Monokai);
        Registry registry = new Registry(options);

        textMateTheme = registry.GetTheme();
        grammar = registry.LoadGrammar(options.GetScopeByExtension(".cs"));
    }

    public static void UpdateSyntaxHighlighting(TextRenderer textRenderer, TextManager textManager, CanvasTextLayout drawnTextLayout, ApplicationTheme appTheme)
    {
        if (!IsLoaded)
        {
            Load();
            IsLoaded = true;
        }
        globalIndex = 0;

        for (int i = textRenderer.NumberOfStartLine; i < textRenderer.NumberOfStartLine + textRenderer.NumberOfRenderedLines; i++)
        {
            var line = textManager.totalLines[i];
            ITokenizeLineResult result = grammar.TokenizeLine(line);

            foreach (IToken token in result.Tokens)
            {
                int startIndex = Math.Min(token.StartIndex, line.Length);
                int endIndex = Math.Min(token.EndIndex, line.Length);

                int length = endIndex - startIndex;
                int absoluteIndex = globalIndex + startIndex;

                foreach (ThemeTrieElementRule themeRule in textMateTheme.Match(token.Scopes))
                {
                    var clr = ((Color)colorConverter.ConvertFromString(textMateTheme.GetColor(themeRule.foreground))).ToMediaColor();

                    drawnTextLayout.SetColor(absoluteIndex, length, clr);
                    //if ((themeRule.fontStyle & TextMateSharp.Themes.FontStyle.Italic) != 0)
                    //    drawnTextLayout.SetFontStyle(absoluteIndex, length, Windows.UI.Text.FontStyle.Italic);

                    //if ((themeRule.fontStyle & TextMateSharp.Themes.FontStyle.Underline) != 0)
                    //    drawnTextLayout.SetUnderline(absoluteIndex, length, true);

                    //if ((themeRule.fontStyle & TextMateSharp.Themes.FontStyle.Bold) != 0)
                    //    drawnTextLayout.SetFontWeight(absoluteIndex, length, BoldFont);

                }
            }
            globalIndex += line.Length + 2;
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
