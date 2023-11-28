using Microsoft.Graphics.Canvas.Text;
using TextControlBox.Extensions;
using TextControlBox.Renderer;
using TextControlBox.Text;
using TextControlBox_WinUI.Models;
using Windows.Foundation;

namespace TextControlBox_WinUI.Helper
{
    internal class CursorHelper
    {
        public static int GetCursorLineFromPoint(Point point, TextRenderer textRenderer, TextControlBoxProperties props)
        {
            //Calculate the relative linenumber, where the pointer was pressed at
            int Linenumber = (int)(point.Y / props.SingleLineHeight);
            if (Linenumber < 0)
                Linenumber = 0;

            Linenumber += textRenderer.RenderStartLine;

            if (Linenumber >= textRenderer.RenderStartLine + textRenderer.RenderLineCount)
                Linenumber = textRenderer.RenderStartLine + textRenderer.RenderLineCount - 1;

            return Linenumber;
        }
        public static int GetCharacterPositionFromPoint(CurrentWorkingLine workingLine, Point cursorPosition, float marginLeft)
        {
            if (workingLine.Text == null || workingLine.TextLayout == null)
                return 0;

            workingLine.TextLayout.HitTest(
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

        public static int GetCurrentPosInLine(CurrentWorkingLine workingLine, CursorPosition cursorPos)
        {
            int curLineLength = workingLine.Text.Length;

            if (cursorPos.CharacterPosition > curLineLength)
                return curLineLength;
            return cursorPos.CharacterPosition;
        }

        public static void UpdateCursorVariable(TextControlBox.TextControlBox textbox, Point point, TextControlBoxProperties props, SelectionManager cursorManager, TextRenderer textRenderer, TextManager textManager, CurrentWorkingLine workingLine)
        {
            //Apply an offset to the cursorposition
            point = point.Subtract(-(props.SingleLineHeight / 4), props.SingleLineHeight / 4);

            cursorManager.Cursor.LineNumber = GetCursorLineFromPoint(point, textRenderer, props);
            workingLine.UpdateTextLayout(textManager, cursorManager.Cursor);
            cursorManager.Cursor.CharacterPosition = GetCharacterPositionFromPoint(workingLine, point, (float)-textbox.HorizontalScroll);
        }

    }
}
