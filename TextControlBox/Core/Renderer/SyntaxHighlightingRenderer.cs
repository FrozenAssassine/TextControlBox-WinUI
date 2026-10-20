using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Xaml;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections.Generic;
using TextControlBoxNS.Models;
using Windows.UI.Text;
using static TextControlBoxNS.Core.Text.TextManager;

namespace TextControlBoxNS.Core.Renderer;

internal class SyntaxHighlightingRenderer
{
    public readonly static FontWeight BoldFont = new FontWeight(600);
    public const FontStyle ItalicFont = FontStyle.Italic;

    public const int MaxHighlightTextLength = 20_000;
    // Limits the formatting ranges handed to DirectWrite. The rules are merged per character first, so this counts
    // the runs of equal formatting that are actually visible, not the single rule matches. 150 lines of dense code
    // need about 2000 runs. SetColor gets slower than linear above that (~10ms for 2500, ~25ms for 4000 ranges).
    public const int MaxHighlightsPerFrame = 2_500;
    // Highlighting runs on the UI thread on every rebuild of the visible text. When the rules together need
    // longer than this, the remaining rules are skipped for the frame instead of freezing the editor.
    public const int MaxHighlightMillisecondsPerFrame = 150;

    private const byte StyleBold = 1, StyleItalic = 2, StyleUnderline = 4;

    internal readonly record struct HighlightRun(int Start, int Length, Windows.UI.Color Color, bool Bold, bool Italic, bool Underlined);

    public static void UpdateSyntaxHighlighting(LineSliceResult lineSliceResult, string newLineCharacter, CanvasTextLayout drawnTextLayout, ApplicationTheme theme, SyntaxHighlightLanguage syntaxHighlightingLanguage, bool syntaxHighlighting)
    {
        if (!syntaxHighlighting || drawnTextLayout == null || string.IsNullOrEmpty(lineSliceResult.Text))
            return;

        // Skip syntax highlighting when the rendered text exceeds the safety threshold.
        // DirectWrite and Direct2D device-lock contention during thousands of range allocations
        // can stall the GPU queue or deadlock with swapchain resizing.
        if (lineSliceResult.Text.Length > MaxHighlightTextLength)
            return;

        var runs = ComputeHighlightRuns(syntaxHighlightingLanguage, lineSliceResult.Text, lineSliceResult.Lines, newLineCharacter, theme == ApplicationTheme.Light);
        foreach (var run in runs)
        {
            try
            {
                drawnTextLayout.SetColor(run.Start, run.Length, run.Color);
                if (run.Italic)
                    drawnTextLayout.SetFontStyle(run.Start, run.Length, ItalicFont);
                if (run.Bold)
                    drawnTextLayout.SetFontWeight(run.Start, run.Length, BoldFont);
                if (run.Underlined)
                    drawnTextLayout.SetUnderline(run.Start, run.Length, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateSyntaxHighlighting: DirectWrite formatting failed: {ex.Message}");
                break;
            }
        }
    }

    // The rules are applied in order and a later rule overwrites an earlier one. That is done on a per character
    // buffer first, so only the merged result reaches DirectWrite and a limit never drops the last rules (which are
    // usually the strings and comments) but only cuts off the end of the text.
    internal static List<HighlightRun> ComputeHighlightRuns(SyntaxHighlightLanguage language, string text, ReadOnlySpan<string> lines, string newLineCharacter, bool isLightTheme, int maxRuns = MaxHighlightsPerFrame, int maxMilliseconds = MaxHighlightMillisecondsPerFrame)
    {
        var runs = new List<HighlightRun>();
        if (language == null || string.IsNullOrEmpty(text))
            return runs;

        int textLength = text.Length;
        //0 means not highlighted, every highlight color is opaque
        uint[] colors = ArrayPool<uint>.Shared.Rent(textLength);
        byte[] styles = ArrayPool<byte>.Shared.Rent(textLength);
        try
        {
            Array.Clear(colors, 0, textLength);
            Array.Clear(styles, 0, textLength);
            long startTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();

            bool TimeIsUp() => System.Diagnostics.Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds > maxMilliseconds;

            if (language.HighlightRules != null && language.HighlightRules.Length > 0)
            {
                foreach (var rule in language.HighlightRules)
                {
                    if (TimeIsUp())
                        break;

                    var highlights = rule.GetHighlights(lines, text, newLineCharacter);
                    if (highlights == null)
                        continue;

                    foreach (var span in highlights)
                        Paint(colors, styles, textLength, span.Start, span.Length, isLightTheme ? span.ColorLight : span.ColorDark, span.Style);
                }
            }
            else if (language.Highlights != null)
            {
                foreach (var highlight in language.Highlights)
                {
                    if (TimeIsUp())
                        break;
                    if (highlight.PrecompiledRegex == null)
                        continue;

                    var color = isLightTheme ? highlight.ColorLight_Clr : highlight.ColorDark_Clr;
                    try
                    {
                        foreach (var match in highlight.PrecompiledRegex.EnumerateMatches(text))
                            Paint(colors, styles, textLength, match.Index, match.Length, color, highlight.CodeStyle);
                    }
                    catch (System.Text.RegularExpressions.RegexMatchTimeoutException)
                    {
                        //the rest of this rule is skipped for this frame
                    }
                }
            }

            int runStart = 0;
            for (int i = 1; i <= textLength; i++)
            {
                if (i < textLength && colors[i] == colors[runStart] && styles[i] == styles[runStart])
                    continue;

                if (colors[runStart] != 0)
                {
                    if (runs.Count >= maxRuns)
                        break;

                    uint c = colors[runStart];
                    byte style = styles[runStart];
                    runs.Add(new HighlightRun(runStart, i - runStart,
                        Windows.UI.Color.FromArgb((byte)(c >> 24), (byte)(c >> 16), (byte)(c >> 8), (byte)c),
                        (style & StyleBold) != 0, (style & StyleItalic) != 0, (style & StyleUnderline) != 0));
                }
                runStart = i;
            }
        }
        finally
        {
            ArrayPool<uint>.Shared.Return(colors);
            ArrayPool<byte>.Shared.Return(styles);
        }
        return runs;
    }

    private static void Paint(uint[] colors, byte[] styles, int textLength, int start, int length, Windows.UI.Color color, CodeFontStyle style)
    {
        if (start < 0 || length <= 0 || start + length > textLength)
            return;

        uint packed = ((uint)Math.Max((byte)1, color.A) << 24) | ((uint)color.R << 16) | ((uint)color.G << 8) | color.B;
        byte styleFlags = 0;
        if (style != null)
            styleFlags = (byte)((style.Bold ? StyleBold : 0) | (style.Italic ? StyleItalic : 0) | (style.Underlined ? StyleUnderline : 0));

        colors.AsSpan(start, length).Fill(packed);
        //like DirectWrite before: a style is only ever added, a later rule does not remove it
        if (styleFlags != 0)
        {
            for (int i = start; i < start + length; i++)
                styles[i] |= styleFlags;
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
