using Microsoft.Graphics.Canvas.Text;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Core;
using Windows.Foundation;
using TextControlBoxNS.Models;

namespace TextControlBoxNS.Helper;

internal class SelectionHelper
{
    //returns whether the pointer is over a selection
    public static bool PointerIsOverSelection(TextRenderer textRenderer, Point pointerPosition, TextSelection selection)
    {
        if (textRenderer.DrawnTextLayout == null || selection.IsNull)
            return false;

        CanvasTextLayoutRegion[] regions = textRenderer.DrawnTextLayout.GetCharacterRegions(selection.renderedIndex, selection.renderedLength);
        for (int i = 0; i < regions.Length; i++)
        {
            if (regions[i].LayoutBounds.Contains(pointerPosition))
                return true;
        }
        return false;
    }

    public static bool CursorIsInSelection(SelectionManager selectionManager, CursorPosition cursorPosition, TextSelection textSelection)
    {
        var textSel = selectionManager.OrderTextSelectionSeparated(textSelection);
        if (textSel.startNull && textSel.endNull)
            return false;

        //Cursorposition is smaller than the start of selection
        if (textSel.startLine> cursorPosition.LineNumber)
            return false;

        //Selectionend is smaller than Cursorposition -> not in selection
        if (textSel.endLine< cursorPosition.LineNumber)
            return false;

        //Selection-start line equals Cursor line:
        if (cursorPosition.LineNumber == textSel.startLine)
            return cursorPosition.CharacterPosition > textSel.startChar;

        //Selection-end line equals Cursor line
        else if (cursorPosition.LineNumber == textSel.endLine)
            return cursorPosition.CharacterPosition < textSel.endChar;
        return true;
    }


}
