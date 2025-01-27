using Microsoft.UI.Input;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Helper;
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

    public void Init(
        CoreTextControlBox textbox,
        CursorManager cursorManager,
        SelectionManager selectionManager,
        TextManager textManager,
        TextActionManager textActionManager,
        CanvasUpdateManager canvasUpdateManager,
        SelectionRenderer selectionRenderer,
        TextRenderer textRenderer)
    {
        this.cursorManager = cursorManager;
        this.textManager = textManager;
        coreTextbox = textbox;
        this.textActionManager = textActionManager;
        this.canvasUpdateManager = canvasUpdateManager;
        this.selectionManager = selectionManager;
        this.textRenderer = textRenderer;
        this.selectionRenderer = selectionRenderer;
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

        //Delete the selection
        textActionManager.RemoveText();

        textActionManager.AddCharacter(textToInsert, false);

        coreTextbox.ChangeCursor(InputSystemCursorShape.IBeam);
        isDragDropSelection = false;
        canvasUpdateManager.UpdateAll();
    }
    public void EndDragDropSelection(bool clearSelectedText = true)
    {
        isDragDropSelection = false;
        if (clearSelectedText)
            selectionManager.ForceClearSelection(canvasUpdateManager);

        coreTextbox.ChangeCursor(InputSystemCursorShape.IBeam);
        selectionRenderer.IsSelecting = false;
        canvasUpdateManager.UpdateCursor();
    }
    public bool DragDropOverSelection(Point curPos)
    {
        bool res = SelectionHelper.CursorIsInSelection(selectionManager, cursorManager.currentCursorPosition) ||
            SelectionHelper.PointerIsOverSelection(textRenderer, selectionManager, curPos);

        coreTextbox.ChangeCursor(res ? InputSystemCursorShape.UniversalNo : InputSystemCursorShape.IBeam);

        return res;
    }
}
