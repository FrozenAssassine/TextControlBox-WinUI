using Microsoft.Graphics.Canvas.Text;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Core.Selection;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Models;
using Windows.Foundation;

namespace TextControlBoxNS.Helper;

internal class SelectionHelper
{
    //returns whether the pointer is over a selection
    public static bool PointerIsOverSelection(TextRenderer textRenderer, SelectionManager selectionManager, Point pointerPosition)
    {
        if (textRenderer.DrawnTextLayout == null || !selectionManager.HasSelection)
            return false;

        CanvasTextLayoutRegion[] regions = textRenderer.DrawnTextLayout.GetCharacterRegions(selectionManager.currentTextSelection.renderedIndex, selectionManager.currentTextSelection.renderedLength);
        for (int i = 0; i < regions.Length; i++)
        {
            if (regions[i].LayoutBounds.Contains(pointerPosition))
                return true;
        }
        return false;
    }

    public static bool CursorIsInSelection(SelectionManager selectionManager, CursorPosition cursorPosition)
    {
        var textSel = selectionManager.OrderTextSelectionSeparated();
        if (textSel.startNull && textSel.endNull)
            return false;

        //Cursorposition is smaller than the start of selection
        if (textSel.startLine > cursorPosition.LineNumber)
            return false;

        //Selectionend is smaller than Cursorposition -> not in selection
        if (textSel.endLine < cursorPosition.LineNumber)
            return false;

        //Selection-start line equals Cursor line:
        if (cursorPosition.LineNumber == textSel.startLine)
            return cursorPosition.CharacterPosition > textSel.startChar;

        //Selection-end line equals Cursor line
        else if (cursorPosition.LineNumber == textSel.endLine)
            return cursorPosition.CharacterPosition < textSel.endChar;
        return true;
    }
    public static bool TextIsSelected(CursorPosition start, CursorPosition end)
    {
        if (start.IsNull || end.IsNull)
            return false;

        return start.LineNumber != end.LineNumber ||
            start.CharacterPosition != end.CharacterPosition;
    }

    public static (bool startNull, bool endNull, int startLine, int startChar, int endLine, int endChar) OrderTextSelectionSeparated(TextSelection selection, bool? hasSelection = null)
    {
        //allow for passing whether the selection is null or not. Required for undo/redo and selectionmanager checking. They are differentiated
        if (hasSelection.HasValue ? !hasSelection.Value : !selection.HasSelection)
            return (true, true, -1, -1, -1, -1);

        if (selection.EndPosition.IsNull && !selection.StartPosition.IsNull)
            return (false, true, selection.StartPosition.LineNumber, selection.StartPosition.CharacterPosition, -1, -1);

        if (!selection.EndPosition.IsNull && selection.StartPosition.IsNull)
            return (false, true, selection.EndPosition.LineNumber, selection.EndPosition.CharacterPosition, -1, -1);

        int startLine = selection.GetMinLine();
        int endLine = selection.GetMaxLine();
        int startPosition;
        int endPosition;

        if (startLine == endLine)
        {
            startPosition = selection.GetMinChar();
            endPosition = selection.GetMaxChar();
        }
        else
        {
            if (selection.StartPosition.LineNumber < selection.EndPosition.LineNumber)
            {
                endPosition = selection.EndPosition.CharacterPosition;
                startPosition = selection.StartPosition.CharacterPosition;
            }
            else
            {
                endPosition = selection.StartPosition.CharacterPosition;
                startPosition = selection.EndPosition.CharacterPosition;
            }
        }

        return (false, false, startLine, startPosition, endLine, endPosition);
    }

    //returns whether the selection starts at character zero and ends
    //needs to pass any selection object, since undo/redo uses different textselection
    public static bool WholeLinesAreSelected(TextSelection selection, TextManager textManager)
    {
        if (!selection.HasSelection)
            return false;

        var sel = OrderTextSelectionSeparated(selection);
        if (sel.startNull && sel.endNull)
            return false;

        return sel.startChar == 0 && sel.endChar == textManager.GetLineLength(sel.endLine);
    }
}
