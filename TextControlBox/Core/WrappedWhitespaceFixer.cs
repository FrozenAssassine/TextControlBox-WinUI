using Microsoft.Graphics.Canvas.Text;
using System;
using System.Numerics;
using Windows.Foundation;

namespace TextControlBoxNS.Core;

/// <summary>
/// DirectWrite never wraps whitespace: a run of spaces/tabs that reaches the wrap width "hangs" past the right edge
/// on the same row instead of continuing on the next one. For a run in the middle of a line (e.g. a long indent in
/// front of text) that means the text behind it wraps to the next row while every further tab only stretches the row
/// above into nothing. This fixer replaces each whitespace character that lies completely beyond the wrap width with an
/// invisible inline object of the same advance. Inline objects are not whitespace, so DirectWrite wraps them like any
/// other content, while caret positions, hit tests and character regions keep working per character.
/// Characters that start inside the wrap width are left alone, so the normal "space hangs at a word break" behavior is unchanged.
/// </summary>
internal static partial class WrappedWhitespaceFixer
{
    private const float Tolerance = 0.01f;
    /// <summary>Object replacement character: lets the inline object, not Unicode's "never break before whitespace"
    /// rule, decide the break opportunities. Same length as the replaced character, so every index stays valid.</summary>
    private const char ObjectReplacementChar = '\uFFFC';
    // Each pass converts at most one whitespace run per paragraph (converting a run reflows the rest of its paragraph,
    // so later positions in it are stale). A pass that converts nothing ends the loop.
    private const int MaxPasses = 64;

    /// <summary>
    /// Creates the layout for <paramref name="text"/> via <paramref name="createLayout"/> and, when whitespace hangs past
    /// the wrap width, recreates it with that whitespace turned into wrappable placeholders. The returned layout has the
    /// same character indices as <paramref name="text"/>.
    /// </summary>
    public static CanvasTextLayout CreateLayout(Func<string, CanvasTextLayout> createLayout, string text, CanvasTextFormat textFormat)
    {
        CanvasTextLayout layout = createLayout(text);
        if (layout == null || string.IsNullOrEmpty(text) || textFormat == null || textFormat.WordWrapping == CanvasWordWrapping.NoWrap)
            return layout;

        float wrapWidth = (float)layout.RequestedSize.Width;
        if (wrapWidth <= 1 || float.IsInfinity(wrapWidth) || float.IsNaN(wrapWidth))
            return layout;

        // Fast path: nothing hangs past the wrap width at all.
        if (!ContainsWhitespace(text) || !HasHangingOverflow(layout, wrapWidth))
            return layout;

        float fontSize = Math.Max(1, textFormat.FontSize);
        float[] advances = null; // advance per converted index, 0 = not converted
        char[] layoutText = null;

        for (int pass = 0; pass < MaxPasses; pass++)
        {
            bool anyConverted = false;
            int i = 0;
            while (i < text.Length)
            {
                if (!IsWrappableWhitespace(text[i]) || (advances != null && advances[i] > 0))
                {
                    i++;
                    continue;
                }

                int runStart = i;
                while (i < text.Length && IsWrappableWhitespace(text[i]) && (advances == null || advances[i] <= 0))
                    i++;

                if (!TryMeasureHangingRun(layout, text, textFormat, runStart, i, wrapWidth, fontSize, ref advances))
                    continue;

                anyConverted = true;
                // The rest of this paragraph will reflow; continue with the next paragraph in this pass.
                while (i < text.Length && text[i] != '\n' && text[i] != '\r')
                    i++;
            }

            if (!anyConverted)
                return layout;

            layoutText ??= text.ToCharArray();
            for (int k = 0; k < advances.Length; k++)
            {
                if (advances[k] > 0)
                    layoutText[k] = ObjectReplacementChar;
            }

            layout.Dispose();
            layout = createLayout(new string(layoutText));
            for (int k = 0; k < advances.Length; k++)
            {
                if (advances[k] > 0)
                    layout.SetInlineObject(k, 1, new WhitespaceInlineObject(advances[k], fontSize));
            }

            if (!HasHangingOverflow(layout, wrapWidth))
                return layout;
        }
        return layout;
    }

    /// <summary>Records the advances of the characters of a whitespace run that lie completely beyond the wrap width.
    /// Returns false when the run does not hang past the edge.</summary>
    private static bool TryMeasureHangingRun(CanvasTextLayout layout, string text, CanvasTextFormat textFormat, int runStart, int runEnd, float wrapWidth, float fontSize, ref float[] advances)
    {
        // Whitespace never wraps, so the whole run sits on one row; only runs whose end hangs past the edge matter.
        if (layout.GetCaretPosition(runEnd - 1, true).X <= wrapWidth + Tolerance)
            return false;

        int firstOutside = -1;
        for (int k = runStart; k < runEnd; k++)
        {
            if (layout.GetCaretPosition(k, false).X >= wrapWidth - Tolerance)
            {
                firstOutside = k;
                break;
            }
        }
        if (firstOutside < 0)
            return false;

        advances ??= new float[text.Length];
        for (int index = firstOutside; index < runEnd; index++)
        {
            float advance = layout.GetCaretPosition(index, true).X - layout.GetCaretPosition(index, false).X;
            if (advance <= 0 || float.IsNaN(advance))
                advance = text[index] == '\t' ? Math.Max(1, textFormat.IncrementalTabStop) : fontSize * 0.5f;
            advances[index] = advance;
        }
        return true;
    }

    private static bool HasHangingOverflow(CanvasTextLayout layout, float wrapWidth)
    {
        Rect bounds = layout.LayoutBoundsIncludingTrailingWhitespace;
        return bounds.X + bounds.Width > wrapWidth + Tolerance;
    }

    private static bool ContainsWhitespace(string text)
        => text.AsSpan().IndexOfAny(' ', '\t') >= 0;

    private static bool IsWrappableWhitespace(char c) => c == ' ' || c == '\t';

    /// <summary>Invisible placeholder with the advance of the whitespace character it replaces. Line breaks are
    /// allowed on both sides so a long whitespace run wraps across rows.</summary>
    private sealed partial class WhitespaceInlineObject : ICanvasTextInlineObject
    {
        private readonly float width;
        private readonly float height;

        public WhitespaceInlineObject(float width, float fontSize)
        {
            this.width = width;
            height = fontSize;
        }

        public void Draw(ICanvasTextRenderer textRenderer, Vector2 point, bool isSideways, bool isRightToLeft, object brush)
        {
            // Whitespace draws nothing; visible whitespace markers are drawn by WhitespaceCharactersRenderer.
        }

        public Size Size => new Size(width, height);
        public float Baseline => height * 0.8f;
        public bool SupportsSideways => false;
        public Rect DrawBounds => new Rect(0, 0, width, height);
        public CanvasLineBreakCondition BreakBefore => CanvasLineBreakCondition.CanBreak;
        public CanvasLineBreakCondition BreakAfter => CanvasLineBreakCondition.CanBreak;
    }
}
