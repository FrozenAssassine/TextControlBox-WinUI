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
        double adjustedY = point.Y - textRenderer.VerticalRenderingOffset;
        int relativeLine = (int)Math.Floor(adjustedY / textRenderer.SingleLineHeight);

        return Math.Max(0, relativeLine + textRenderer.NumberOfStartLine);
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
        //Apply an offset to the cursorposition to make selection easier
        point.X += textRenderer.SingleLineHeight / scrollManager.DefaultVerticalScrollSensitivity;
        

        cursorPos.LineNumber = GetCursorLineFromPoint(textRenderer, point);
        cursorPos.LineNumber = Math.Clamp(cursorPos.LineNumber, 0, textRenderer.NumberOfStartLine + textRenderer.NumberOfRenderedLines - 1); //Clamp to visible? or total? GetCursorLineFromPoint handles relative logic, but we need to clamp to document bounds.

        //GetCursorLineFromPoint returns absolute line index.    
        textRenderer.UpdateCurrentLineTextLayout(canvasText);
        cursorPos.CharacterPosition = GetCharacterPositionFromPoint(currentLineManager, textRenderer.CurrentLineTextLayout, point, (float)-scrollManager.HorizontalScroll);
    }
}

