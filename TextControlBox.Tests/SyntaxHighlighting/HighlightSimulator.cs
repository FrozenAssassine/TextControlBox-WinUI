using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using TextControlBoxNS;
using TextControlBoxNS.Core.Renderer;

namespace TextControlBox.Tests.SyntaxHighlighting;

/// <summary>
/// Runs the renderer's highlight computation for a text and returns the dark theme color every character ends up
/// with, so tests can assert what the user actually sees.
/// </summary>
internal static class HighlightSimulator
{
    public static string?[] Colorize(SyntaxHighlightLanguage language, string text, string newLine = "\n", bool respectSpanBudget = false)
    {
        language.CompileAllRegex();
        var colors = new string?[text.Length];
        var runs = SyntaxHighlightingRenderer.ComputeHighlightRuns(language, text, text.Split(newLine), newLine, isLightTheme: false,
            maxRuns: respectSpanBudget ? SyntaxHighlightingRenderer.MaxHighlightsPerFrame : int.MaxValue,
            maxMilliseconds: int.MaxValue);

        foreach (var run in runs)
        {
            string hex = ToHex(run.Color);
            for (int i = run.Start; i < run.Start + run.Length; i++)
                colors[i] = hex;
        }
        return colors;
    }

    public static string ToHex(Windows.UI.Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    /// <summary>Asserts that every character of the n-th occurrence of <paramref name="token"/> has the expected color (null = not highlighted).</summary>
    public static void AssertColor(SyntaxHighlightLanguage language, string text, string token, string? expectedDarkColor, int occurrence = 0, string newLine = "\n")
    {
        var colors = Colorize(language, text, newLine);
        int index = IndexOf(text, token, occurrence);
        string? expected = expectedDarkColor?.ToUpperInvariant();

        var actual = colors.Skip(index).Take(token.Length).ToArray();
        if (actual.Any(c => c != expected))
        {
            Assert.Fail($"[{language.Name}] '{token}' in {Quote(text)} should be {expected ?? "not highlighted"}, but is {Describe(token, actual)}");
        }
    }

    /// <summary>Asserts that no character of <paramref name="token"/> has the given color.</summary>
    public static void AssertNotColor(SyntaxHighlightLanguage language, string text, string token, string unexpectedDarkColor, int occurrence = 0)
    {
        var colors = Colorize(language, text);
        int index = IndexOf(text, token, occurrence);
        string unexpected = unexpectedDarkColor.ToUpperInvariant();

        var actual = colors.Skip(index).Take(token.Length).ToArray();
        if (actual.Any(c => c == unexpected))
        {
            Assert.Fail($"[{language.Name}] '{token}' in {Quote(text)} must not be {unexpected}, but is {Describe(token, actual)}");
        }
    }

    public static string? ColorAt(SyntaxHighlightLanguage language, string text, string token, int occurrence = 0, string newLine = "\n")
    {
        var colors = Colorize(language, text, newLine);
        return colors[IndexOf(text, token, occurrence)];
    }

    private static int IndexOf(string text, string token, int occurrence)
    {
        int index = -1;
        for (int i = 0; i <= occurrence; i++)
        {
            index = text.IndexOf(token, index + 1, StringComparison.Ordinal);
            if (index == -1)
                throw new ArgumentException($"Token '{token}' (occurrence {occurrence}) not found in test text");
        }
        return index;
    }

    private static string Describe(string token, string?[] colors)
    {
        var groups = new List<string>();
        int start = 0;
        for (int i = 1; i <= colors.Length; i++)
        {
            if (i == colors.Length || colors[i] != colors[start])
            {
                groups.Add($"'{token.Substring(start, i - start)}'={colors[start] ?? "none"}");
                start = i;
            }
        }
        return string.Join(", ", groups);
    }

    private static string Quote(string text) => "\"" + text.Replace("\r", "\\r").Replace("\n", "\\n") + "\"";
}
