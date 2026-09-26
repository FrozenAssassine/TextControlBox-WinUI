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

    public const int MaxHighlightTextLength = 20_000;
    public const int MaxHighlightsPerFrame = 1_500;

    public static void UpdateSyntaxHighlighting(LineSliceResult lineSliceResult, string newLineCharacter, CanvasTextLayout drawnTextLayout, ApplicationTheme theme, SyntaxHighlightLanguage syntaxHighlightingLanguage, bool syntaxHighlighting)
    {
        if (!syntaxHighlighting || drawnTextLayout == null || string.IsNullOrEmpty(lineSliceResult.Text))
            return;

        // Skip syntax highlighting when the rendered text exceeds the safety threshold.
        // DirectWrite and Direct2D device-lock contention during thousands of range allocations
        // can stall the GPU queue or deadlock with swapchain resizing.
        if (lineSliceResult.Text.Length > MaxHighlightTextLength)
            return;

        bool isLightTheme = theme == ApplicationTheme.Light;
        int appliedSpansCount = 0;
        int textLength = lineSliceResult.Text.Length;

        if (syntaxHighlightingLanguage?.HighlightRules != null && syntaxHighlightingLanguage.HighlightRules.Length > 0)
        {
            foreach (var rule in syntaxHighlightingLanguage.HighlightRules)
            {
                if (appliedSpansCount >= MaxHighlightsPerFrame)
                    break;

                var highlights = rule.GetHighlights(lineSliceResult.Lines, lineSliceResult.Text, newLineCharacter);
                if (highlights == null)
                    continue;

                foreach (var span in highlights)
                {
                    if (appliedSpansCount >= MaxHighlightsPerFrame)
                        break;

                    if (span.Start >= 0 && span.Length > 0 && span.Start + span.Length <= textLength)
                    {
                        ApplyHighlightSpan(drawnTextLayout, span, isLightTheme);
                        appliedSpansCount++;
                    }
                }
            }
        }
        else if (syntaxHighlightingLanguage?.Highlights != null)
        {
            foreach (var highlight in syntaxHighlightingLanguage.Highlights)
            {
                if (appliedSpansCount >= MaxHighlightsPerFrame)
                    break;

                if (highlight.PrecompiledRegex == null) return;

                var color = isLightTheme ? highlight.ColorLight_Clr : highlight.ColorDark_Clr;

                foreach (var match in highlight.PrecompiledRegex.EnumerateMatches(lineSliceResult.Text))
                {
                    if (appliedSpansCount >= MaxHighlightsPerFrame)
                        break;

                    int index = match.Index;
                    int length = match.Length;

                    if (index < 0 || length <= 0 || index + length > textLength)
                        continue;

                    try
                    {
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
                        appliedSpansCount++;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"UpdateSyntaxHighlighting: DirectWrite formatting failed: {ex.Message}");
                        break;
                    }
                }
            }
        }
    }

    private static void ApplyHighlightSpan(CanvasTextLayout drawnTextLayout, HighlightSpan span, bool isLightTheme)
    {
        try
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ApplyHighlightSpan: DirectWrite formatting failed: {ex.Message}");
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
