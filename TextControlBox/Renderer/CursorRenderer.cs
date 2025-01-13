using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Numerics;

namespace TextControlBoxNS.Renderer;

internal class CursorRenderer
{
    public CursorSize _CursorSize = null;

    public void RenderCursor(CanvasTextLayout textLayout, int characterPosition, float xOffset, float y, float fontSize, CursorSize customSize, CanvasDrawEventArgs args, CanvasSolidColorBrush cursorColorBrush)
    {
        if (textLayout == null)
            return;

        Vector2 vector = textLayout.GetCaretPosition(characterPosition < 0 ? 0 : characterPosition, false);
        if (customSize == null)
            args.DrawingSession.FillRectangle(vector.X + xOffset, y, 1, fontSize, cursorColorBrush);
        else
            args.DrawingSession.FillRectangle(vector.X + xOffset + customSize.OffsetX, y + customSize.OffsetY, (float)customSize.Width, (float)customSize.Height, cursorColorBrush);
    }
}
