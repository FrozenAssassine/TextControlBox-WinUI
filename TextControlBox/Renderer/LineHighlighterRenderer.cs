using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace TextControlBoxNS.Renderer
{
    internal class LineHighlighterRenderer
    {
        public static void Render(float canvasWidth, CanvasTextLayout textLayout, float y, float fontSize, CanvasDrawEventArgs args, CanvasSolidColorBrush backgroundBrush)
        {
            if (textLayout == null)
                return;

            args.DrawingSession.FillRectangle(0, y, canvasWidth, fontSize, backgroundBrush);
        }
    }
}
