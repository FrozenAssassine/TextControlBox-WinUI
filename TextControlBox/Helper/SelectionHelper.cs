﻿using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextControlBoxNS.Renderer;
using TextControlBoxNS.Text;
using Windows.Foundation;

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

            textSelection = selectionManager.OrderTextSelection(textSelection);

            //Cursorposition is smaller than the start of selection
            if (textSelection.StartPosition.LineNumber > cursorPosition.LineNumber)
                return false;

            //Selectionend is smaller than Cursorposition -> not in selection
            if (textSelection.EndPosition.LineNumber < cursorPosition.LineNumber)
                return false;

            //Selection-start line equals Cursor line:
            if (cursorPosition.LineNumber == textSelection.StartPosition.LineNumber)
                return cursorPosition.CharacterPosition > textSelection.StartPosition.CharacterPosition;

            //Selection-end line equals Cursor line
            else if (cursorPosition.LineNumber == textSelection.EndPosition.LineNumber)
                return cursorPosition.CharacterPosition < textSelection.EndPosition.CharacterPosition;
            return true;
        }


    }
}
