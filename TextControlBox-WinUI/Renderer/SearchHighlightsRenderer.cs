using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.UI;

namespace TextControlBox.Renderer
{
    internal class SearchHighlightsRenderer
    {
        public static Rect CreateRect(Rect r, float MarginLeft = 0, float MarginTop = 0)
        {
            return new Rect(
                new Point(
                    Math.Floor(r.Left + MarginLeft),//X
                    Math.Floor(r.Top + MarginTop)), //Y
                new Point(
                    Math.Ceiling(r.Right + MarginLeft), //Width
                    Math.Ceiling(r.Bottom + MarginTop))); //Height
        }

        public static void RenderHighlights(
            CanvasDrawEventArgs Event,
            CanvasTextLayout DrawnTextLayout,
            string RenderedText,
            int[] PossibleLines,
            string SearchRegex,
            float ScrollOffsetX,
            float OffsetTop,
            Color SearchHighlightColor)
        {
            if (SearchRegex == null || PossibleLines == null || PossibleLines.Length == 0)
                return;
            MatchCollection matches = null;
            try
            {
                matches = Regex.Matches(RenderedText, SearchRegex);
            }
            catch (ArgumentException)
            {
                return;
            }
            for (int j = 0; j < matches.Count; j++)
            {
                var match = matches[j];

                var layoutRegion = DrawnTextLayout.GetCharacterRegions(match.Index, match.Length);
                if (layoutRegion.Length > 0)
                {
                    Event.DrawingSession.FillRectangle(CreateRect(layoutRegion[0].LayoutBounds, ScrollOffsetX, OffsetTop), SearchHighlightColor);
                }
            }
            return;
        }
    }
}
