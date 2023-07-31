using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Text.RegularExpressions;
using TextControlBoxNS.Helper;
using Windows.UI;

namespace TextControlBoxNS.Renderer
{
    internal class SearchHighlightsRenderer
    {
        public static void RenderHighlights(
            CanvasDrawEventArgs args,
            CanvasTextLayout drawnTextLayout,
            string renderedText,
            int[] possibleLines,
            string searchRegex,
            float scrollOffsetX,
            float offsetTop,
            Color searchHighlightColor)
        {
            if (searchRegex == null || possibleLines == null || possibleLines.Length == 0)
                return;

            MatchCollection matches;
            try
            {
                matches = Regex.Matches(renderedText, searchRegex);
            }
            catch (ArgumentException)
            {
                return;
            }
            for (int j = 0; j < matches.Count; j++)
            {
                var match = matches[j];

                var layoutRegion = drawnTextLayout.GetCharacterRegions(match.Index, match.Length);
                if (layoutRegion.Length > 0)
                {
                    args.DrawingSession.FillRectangle(Utils.CreateRect(layoutRegion[0].LayoutBounds, scrollOffsetX, offsetTop), searchHighlightColor);
                }
            }
            return;
        }
    }
}
