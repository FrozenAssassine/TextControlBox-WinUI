using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Core;
using Windows.Foundation;
using TextControlBoxNS.Models;

namespace TextControlBoxNS.Helper
{
    internal class SelectionHelper
    {

        //returns whether the pointer is over a selection
        public static bool PointerIsOverSelection(TextRenderer textRenderer, Point pointerPosition, TextSelection selection)
        {
            if (textRenderer.DrawnTextLayout == null || selection == null)
                return false;

            CanvasTextLayoutRegion[] regions = textRenderer.DrawnTextLayout.GetCharacterRegions(selection.Index, selection.Length);
            for (int i = 0; i < regions.Length; i++)
            {
                if (regions[i].LayoutBounds.Contains(pointerPosition))
                    return true;
            }
            return false;
        }

        public static bool CursorIsInSelection(SelectionManager selectionManager, CursorPosition cursorPosition, TextSelection textSelection)
        {
            if (textSelection == null)
                return false;

            //Cursorposition is smaller than the start of selection
            if (textSelection.OrderedStartLine > cursorPosition.LineNumber)
                return false;

            //Selectionend is smaller than Cursorposition -> not in selection
            if (textSelection.OrderedEndLine < cursorPosition.LineNumber)
                return false;

            //Selection-start line equals Cursor line:
            if (cursorPosition.LineNumber == textSelection.OrderedStartLine)
                return cursorPosition.CharacterPosition > textSelection.OrderedStartCharacterPos;

            //Selection-end line equals Cursor line
            else if (cursorPosition.LineNumber == textSelection.OrderedEndLine)
                return cursorPosition.CharacterPosition < textSelection.OrderedEndCharacterPos;
            return true;
        }
    }
}
