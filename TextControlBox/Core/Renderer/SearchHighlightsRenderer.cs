using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Text.RegularExpressions;
using TextControlBoxNS.Helper;
using Windows.UI;

namespace TextControlBoxNS.Core.Renderer
{
    internal class SearchHighlightsRenderer
    {
        public static void RenderHighlights(
            CanvasDrawEventArgs args,
            CanvasDrawingSession drawingSession,
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

            //draw the characters only to the drawingSession, which gets passed as a 
            //CanvasCommandList instance for efficient batched rendering 

            MatchCollection matches = Regex.Matches(renderedText, searchRegex);
            for (int j = 0; j < matches.Count; j++)
            {
                var match = matches[j];

                var layoutRegion = drawnTextLayout.GetCharacterRegions(match.Index, match.Length);
                if (layoutRegion.Length > 0)
                {
                    drawingSession.FillRectangle(Utils.CreateRect(layoutRegion[0].LayoutBounds, scrollOffsetX, offsetTop), searchHighlightColor);
                }
            }
            return;
        }
    }
}
