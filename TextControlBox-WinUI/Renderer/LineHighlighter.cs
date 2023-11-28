using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace TextControlBox.Renderer
{
    internal class LineHighlighter
    {
        public static void Render(float CanvasWidth, CanvasTextLayout TextLayout, float XOffset, float Y, float FontSize, CanvasDrawEventArgs args, CanvasSolidColorBrush BackgroundBrush)
        {
            if (TextLayout == null)
                return;

            args.DrawingSession.FillRectangle(0, Y, CanvasWidth, FontSize, BackgroundBrush);
        }
    }
}
