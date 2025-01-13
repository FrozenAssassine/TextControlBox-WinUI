using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextControlBoxNS.Extensions;
using TextControlBoxNS.Renderer;
using TextControlBoxNS.Text;
using Windows.Foundation;

namespace TextControlBoxNS.Helper
{
    internal class CursorHelper
    {
        public static int GetCursorLineFromPoint(Point point, float singleLineHeight, int numberOfRenderedLines, int numberOfStartLine)
        {
            //Calculate the relative linenumber, where the pointer was pressed at
            int linenumber = (int)(point.Y / singleLineHeight);
            linenumber += numberOfStartLine;
            return Math.Clamp(linenumber, 0, numberOfStartLine + numberOfRenderedLines - 1);
        }
        public static int GetCharacterPositionFromPoint(CurrentLineManager currentLineManager, CanvasTextLayout textLayout, Point cursorPosition, float marginLeft)
        {
            if (currentLineManager.GetCurrentLineText() == null || textLayout == null)
                return 0;

            textLayout.HitTest(
                (float)cursorPosition.X - marginLeft, 0,
                out var textLayoutRegion);
            return textLayoutRegion.CharacterIndex;
        }

        //Return the position in pixels of the cursor in the current line
        public static float GetCursorPositionInLine(CanvasTextLayout currentLineTextLayout, CursorPosition cursorPosition, float xOffset)
        {
            if (currentLineTextLayout == null)
                return 0;

            return currentLineTextLayout.GetCaretPosition(cursorPosition.CharacterPosition < 0 ? 0 : cursorPosition.CharacterPosition, false).X + xOffset;
        }

        private void UpdateCursorPosFromPoint(CanvasControl canvasText, CurrentLineManager currentLineManager, TextRenderer textRenderer, ScrollManager scrollManager, Point point, CursorPosition cursorPos)
        {
            //Apply an offset to the cursorposition
            point = point.Subtract(-(textRenderer.SingleLineHeight / 4), textRenderer.SingleLineHeight / 4);

            cursorPos.LineNumber = GetCursorLineFromPoint(point, textRenderer.SingleLineHeight, textRenderer.NumberOfRenderedLines, textRenderer.NumberOfStartLine);

            textRenderer.UpdateCurrentLineTextLayout(canvasText);
            cursorPos.CharacterPosition = GetCharacterPositionFromPoint(currentLineManager, textRenderer.CurrentLineTextLayout, point, (float)-scrollManager.HorizontalScroll);
        }
    }
}
