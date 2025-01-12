using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Numerics;
using TextControlBoxNS.Extensions;
using TextControlBoxNS.Text;
using Windows.Foundation;

namespace TextControlBoxNS.Renderer
{
    internal class CursorRenderer
    {
        public CursorSize _CursorSize = null;
        private readonly TextManager textManager;
        public CursorRenderer(TextManager textManager)
        {
            this.textManager = textManager;
        }

        public int GetCursorLineFromPoint(Point point, float singleLineHeight, int numberOfRenderedLines, int numberOfStartLine)
        {
            //Calculate the relative linenumber, where the pointer was pressed at
            int linenumber = (int)(point.Y / singleLineHeight);
            linenumber += numberOfStartLine;
            return Math.Clamp(linenumber, 0, numberOfStartLine + numberOfRenderedLines - 1);
        }
        public int GetCharacterPositionFromPoint(CanvasTextLayout textLayout, Point cursorPosition, float marginLeft)
        {
            if (textManager.totalLines.GetCurrentLineText() == null || textLayout == null)
                return 0;

            textLayout.HitTest(
                (float)cursorPosition.X - marginLeft, 0,
                out var textLayoutRegion);
            return textLayoutRegion.CharacterIndex;
        }

        //Return the position in pixels of the cursor in the current line
        public float GetCursorPositionInLine(CanvasTextLayout currentLineTextLayout, CursorPosition cursorPosition, float xOffset)
        {
            if (currentLineTextLayout == null)
                return 0;

            return currentLineTextLayout.GetCaretPosition(cursorPosition.CharacterPosition < 0 ? 0 : cursorPosition.CharacterPosition, false).X + xOffset;
        }

        //Return the cursor Width
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
}
