using Microsoft.UI.Input;
using System.Diagnostics;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Models;
using Windows.Foundation;

namespace TextControlBoxNS.Core.Selection;

internal class SelectionDragDropManager
{
    public bool isDragDropSelection = false;

    private CursorManager cursorManager;
    private SelectionManager selectionManager;
    private TextManager textManager;
    private CoreTextControlBox coreTextbox;
    private TextActionManager textActionManager;
    private CanvasUpdateManager canvasUpdateManager;
    private SelectionRenderer selectionRenderer;
    private TextRenderer textRenderer;
    private UndoRedo undoRedo;
    public void Init(
        CoreTextControlBox textbox,
        CursorManager cursorManager,
        SelectionManager selectionManager,
        TextManager textManager,
        TextActionManager textActionManager,
        CanvasUpdateManager canvasUpdateManager,
        SelectionRenderer selectionRenderer,
        TextRenderer textRenderer,
        UndoRedo undoRedo)
    {
        this.cursorManager = cursorManager;
        this.textManager = textManager;
        this.coreTextbox = textbox;
        this.textActionManager = textActionManager;
        this.canvasUpdateManager = canvasUpdateManager;
        this.selectionManager = selectionManager;
        this.textRenderer = textRenderer;
        this.selectionRenderer = selectionRenderer;
        this.undoRedo = undoRedo;
    }

    public void DoDragDropSelection()
    {
        if (!selectionManager.HasSelection || textManager._IsReadonly)
            return;

        //Position to insert is selection start or selection end -> no need to drag
        if (cursorManager.Equals(selectionManager.currentTextSelection.StartPosition, cursorManager.currentCursorPosition) || cursorManager.Equals(selectionManager.currentTextSelection.EndPosition, cursorManager.currentCursorPosition))
        {
            coreTextbox.ChangeCursor(InputSystemCursorShape.IBeam);
            isDragDropSelection = false;
            return;
        }

        string textToInsert = coreTextbox.SelectedText;
        var selection = new TextSelection(selectionManager.currentTextSelection);

        undoRedo.EnableCombineNextUndoItems = true;

        //Delete the selection
        selectionManager.ClearSelection();
        textActionManager.AddCharacter(textToInsert, false);

        selectionManager.SetSelection(selection);
        textActionManager.DeleteSelection();

        undoRedo.EnableCombineNextUndoItems = false;

        coreTextbox.ChangeCursor(InputSystemCursorShape.IBeam);
        isDragDropSelection = false;
        canvasUpdateManager.UpdateAll();
    }
    public void EndDragDropSelection(bool clearSelectedText = true)
    {
        isDragDropSelection = false;
        if (clearSelectedText)
            selectionManager.ClearSelection();

        coreTextbox.ChangeCursor(InputSystemCursorShape.IBeam);
        selectionManager.IsSelecting = false;
        canvasUpdateManager.UpdateCursor();
        canvasUpdateManager.UpdateSelection();
    }
    public bool DragDropOverSelection(Point curPos)
    {
        bool res = SelectionHelper.CursorIsInSelection(selectionManager, cursorManager.currentCursorPosition) ||
            SelectionHelper.PointerIsOverSelection(textRenderer, selectionManager, curPos);

        coreTextbox.ChangeCursor(res ? InputSystemCursorShape.UniversalNo : InputSystemCursorShape.IBeam);

        return res;
    }
}
