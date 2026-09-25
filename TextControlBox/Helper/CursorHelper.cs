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
        // In wrap mode a pointer Y maps to a visual row, which maps to its owning document line.
        if (textRenderer.IsWordWrapEnabled)
            return textRenderer.GetDocumentLineFromVisualRow(textRenderer.GetVisualRowFromPointY(point.Y));

        //Calculate the relative linenumber, where the pointer was pressed at
        double adjustedY = Math.Max(0, point.Y - textRenderer.TopInset);
        int relativeLine = (int)Math.Floor(adjustedY / textRenderer.SingleLineHeight);

        return Math.Max(0, relativeLine + textRenderer.NumberOfStartLine);
    }
    public static (int CharacterIndex, bool IsTrailing) GetCharacterPositionFromPoint(CurrentLineManager currentLineManager, CanvasTextLayout textLayout, Point cursorPosition, float marginLeft, float y = 0, bool isSelecting = false)
    {
        if (currentLineManager.GetCurrentLineText() == null || textLayout == null)
            return (0, false);

        textLayout.HitTest(
            (float)cursorPosition.X - marginLeft, y,
            out var textLayoutRegion,
            out bool isTrailingHit);

        if (!isTrailingHit)
            return (textLayoutRegion.CharacterIndex, false);

        int nextIndex = textLayoutRegion.CharacterIndex + textLayoutRegion.CharacterCount;

        // If the trailing position wraps to the next visual row, keep the cursor/selection
        // on the current visual row at the trailing edge of the last visible word.
        var hitPos = textLayout.GetCaretPosition(textLayoutRegion.CharacterIndex, false);
        string currentLine = currentLineManager.GetCurrentLineText();
        int lineLength = currentLine?.Length ?? nextIndex;
        var nextPos = textLayout.GetCaretPosition(Math.Min(nextIndex, lineLength), false);
        if (nextPos.Y > hitPos.Y + 1)
        {
            int charIndex = textLayoutRegion.CharacterIndex;
            if (currentLine != null && charIndex < currentLine.Length && char.IsWhiteSpace(currentLine[charIndex]) && charIndex > 0)
            {
                charIndex--;
            }

            if (isSelecting)
                return (charIndex + textLayoutRegion.CharacterCount, false);

            return (charIndex, true);
        }

        if (isSelecting)
        {
            if (currentLine != null && nextIndex >= currentLine.Length)
            {
                float relativeX = (float)cursorPosition.X - marginLeft;
                if (relativeX > textLayoutRegion.LayoutBounds.Right + 6)
                {
                    return (currentLine.Length + 1, false);
                }
            }
            return (nextIndex, false);
        }

        return (nextIndex, false);
    }

    //Return the position in pixels of the cursor in the current line
    public static float GetCursorPositionInLine(CanvasTextLayout currentLineTextLayout, CursorPosition cursorPosition, float xOffset)
    {
        if (currentLineTextLayout == null)
            return 0;

        try
        {
            return currentLineTextLayout.GetCaretPosition(cursorPosition.CharacterPosition < 0 ? 0 : cursorPosition.CharacterPosition, cursorPosition.IsTrailing).X + xOffset;
        }
        catch
        {
            return 0;
        }
    }

    public static void UpdateCursorPosFromPoint(CanvasControl canvasText, CurrentLineManager currentLineManager, TextRenderer textRenderer, ScrollManager scrollManager, Point point, CursorPosition cursorPos, bool isSelecting = false)
    {
        

        cursorPos.LineNumber = GetCursorLineFromPoint(textRenderer, point);
        cursorPos.LineNumber = Math.Clamp(cursorPos.LineNumber, 0, Math.Max(0, textRenderer.LinesCount - 1));

        //GetCursorLineFromPoint returns absolute line index.    
        textRenderer.UpdateCurrentLineTextLayout(canvasText);

        // In wrap mode the current-line layout spans multiple rows, so hit-test at the within-line Y and use a
        // zero left margin (no horizontal scroll). Otherwise the current-line layout is sliced to the
        // horizontal window when virtualized, so hit-test against the slice-shifted margin, then map the
        // rendered index back to a document column. All helpers are no-ops when wrap/virtualization are off.
        float hitTestY = textRenderer.IsWordWrapEnabled
            ? textRenderer.GetWrappedLineHitTestYFromPointY(cursorPos.LineNumber, point.Y)
            : 0;
        float marginLeft = textRenderer.IsWordWrapEnabled ? 0 : textRenderer.HorizontalOffset;
        var (renderedCharacterPosition, isTrailing) = GetCharacterPositionFromPoint(currentLineManager, textRenderer.CurrentLineTextLayout, point, marginLeft, hitTestY, isSelecting);
        int docIndex = textRenderer.GetDocumentCharacterIndexFromRenderedIndex(cursorPos.LineNumber, renderedCharacterPosition, isSelecting);
        string currentLine = currentLineManager.GetCurrentLineText();
        if (!string.IsNullOrEmpty(currentLine))
        {
            docIndex = TextElementHelper.SnapToTextElementStart(currentLine, Math.Clamp(docIndex, 0, currentLine.Length));
        }
        cursorPos.CharacterPosition = docIndex;
        cursorPos.IsTrailing = isTrailing;
    }
}

