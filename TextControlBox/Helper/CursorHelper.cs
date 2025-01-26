using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using TextControlBoxNS.Core;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Extensions;
using Windows.Foundation;

namespace TextControlBoxNS.Helper;

internal class CursorHelper
{
    public static int GetCursorLineFromPoint(TextRenderer textRenderer, Point point)
    {
        //Calculate the relative linenumber, where the pointer was pressed at
        int linenumber = (int)(point.Y / textRenderer.SingleLineHeight);
        linenumber += textRenderer.NumberOfStartLine;
        return Math.Clamp(linenumber, 0, textRenderer.NumberOfStartLine + textRenderer.NumberOfRenderedLines - 1);
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

    public static void UpdateCursorPosFromPoint(CanvasControl canvasText, CurrentLineManager currentLineManager, TextRenderer textRenderer, ScrollManager scrollManager, Point point, CursorPosition cursorPos)
    {
        //Apply an offset to the cursorposition
        point = point.Subtract(-(textRenderer.SingleLineHeight / 4), textRenderer.SingleLineHeight / 4);

        cursorPos.LineNumber = GetCursorLineFromPoint(textRenderer, point);

        textRenderer.UpdateCurrentLineTextLayout(canvasText);
        cursorPos.CharacterPosition = GetCharacterPositionFromPoint(currentLineManager, textRenderer.CurrentLineTextLayout, point, (float)-scrollManager.HorizontalScroll);
    }
}
