using Microsoft.UI.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Renderer;

namespace TextControlBoxNS.Text
{
    internal class SelectionDragDropManager
    {
        public bool isDragDropSelection = false;

        private readonly CursorManager cursorManager;
        private readonly SelectionManager selectionManager;
        private readonly TextManager textManager;
        private readonly TextControlBox textbox;

        public SelectionDragDropManager()
        {

        }

        public void DoDragDropSelection()
        {
            if (selectionManager.currentTextSelection == null || textManager._IsReadonly)
                return;

            //Position to insert is selection start or selection end -> no need to drag
            if (cursorManager.Equals(selectionManager.currentTextSelection.StartPosition, cursorManager.currentCursorPosition) || cursorManager.Equals(selectionManager.currentTextSelection.EndPosition, cursorManager.currentCursorPosition))
            {
                //ChangeCursor(InputSystemCursorShape.IBeam);
                isDragDropSelection = false;
                return;
            }

            string textToInsert = textbox.SelectedText;
            CursorPosition curpos = new CursorPosition(CursorPosition);

            //Delete the selection
            RemoveText();

            CursorPosition = curpos;

            AddCharacter(textToInsert, false);

            ChangeCursor(InputSystemCursorShape.IBeam);
            isDragDropSelection = false;
            canvasHelper.UpdateAll();
        }
        public void EndDragDropSelection(bool clearSelectedText = true)
        {
            isDragDropSelection = false;
            if (clearSelectedText)
                ClearSelection();

            ChangeCursor(InputSystemCursorShape.IBeam);
            selectionrenderer.IsSelecting = false;
            canvasHelper.UpdateCursor();
        }
        public bool DragDropOverSelection(Point curPos)
        {
            bool res = SelectionHelper.CursorIsInSelection(selectionManager, CursorPosition, TextSelection) ||
                SelectionHelper.PointerIsOverSelection(textRenderer, curPos, selectionManager.currentTextSelection);

            ChangeCursor(res ? InputSystemCursorShape.UniversalNo : InputSystemCursorShape.IBeam);

            return res;
        }


    }
}
