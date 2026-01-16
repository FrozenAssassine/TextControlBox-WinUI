using System;
using System.Collections.Generic;
using TextControlBoxNS;
using TextControlBoxNS.Models;

internal sealed class CsvColumnHighlightRule : IHighlightRule
{
    private readonly CodeFontStyle _style;
    private readonly int _cycleLength;

    public CsvColumnHighlightRule(int cycleLength = 8, CodeFontStyle style = null)
    {
        _style = style;
        _cycleLength = Math.Max(1, cycleLength);
    }

    private static Windows.UI.Color HsvToColor(
        double h,
        double s,
        double v)
    {
        double c = v * s;
        double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        double m = v - c;

        (double r, double g, double b) = h switch
        {
            < 60 => (c, x, 0d),
            < 120 => (x, c, 0d),
            < 180 => (0d, c, x),
            < 240 => (0d, x, c),
            < 300 => (x, 0d, c),
            _ => (c, 0d, x)
        };

        return Windows.UI.Color.FromArgb(
            255,
            (byte)((r + m) * 255),
            (byte)((g + m) * 255),
            (byte)((b + m) * 255)
        );
    }

    private static Windows.UI.Color GenerateColumnColor(int columnIndex, int cycleLength, bool darkTheme)
    {
        int i = columnIndex % cycleLength;

        double hue = (360.0 / cycleLength) * i;

        double saturation = darkTheme ? 0.45 : 0.55;
        double value = darkTheme ? 0.95 : 0.75;

        return HsvToColor(hue, saturation, value);
    }

    public List<HighlightSpan> GetHighlights(ReadOnlySpan<string> lines, string text, string newLineCharacter)
    {
        var highlights = new List<HighlightSpan>();
        GetHighlights(lines, text, newLineCharacter, highlights);
        return highlights;
    }

    private void GetHighlights(ReadOnlySpan<string> lines, string text, string newLineCharacter, List<HighlightSpan> output)
    {
        int globalIndex = 0;

        foreach (string line in lines)
        {
            int lineStart = globalIndex;
            int columnIndex = 0;
            int tokenStart = lineStart;

            for (int i = 0; i <= line.Length; i++)
            {
                if (i != line.Length && line[i] != ',')
                    continue;

                int tokenLength = (lineStart + i) - tokenStart;

                if (tokenLength > 0)
                {
                    output.Add(new HighlightSpan
                    {
                        Start = tokenStart,
                        Length = tokenLength,
                        ColorLight = GenerateColumnColor(columnIndex, _cycleLength, false),
                        ColorDark = GenerateColumnColor(columnIndex, _cycleLength, true),
                        Style = _style
                    });
                }

                columnIndex++;
                tokenStart = lineStart + i + 1;
            }

            globalIndex += line.Length + newLineCharacter.Length;
        }
    }
}