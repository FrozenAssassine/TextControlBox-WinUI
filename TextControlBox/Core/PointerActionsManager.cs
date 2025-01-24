using Microsoft.Graphics.Canvas;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using System;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Core.Renderer;
using Windows.Foundation;
using TextControlBoxNS.Core.Text;

namespace TextControlBoxNS.Core;

internal class PointerActionsManager
{
    Point? OldTouchPosition = null;
    public int PointerClickCount = 0;
    public DispatcherTimer PointerClickTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 200) };

    private SelectionRenderer selectionRenderer;
    private SelectionDragDropManager selectionDragDropManager;
    private CoreTextControlBox coreTextbox;
    private ScrollManager scrollManager;
    private CanvasUpdateManager canvasUpdateManager;
    private CursorManager cursorManager;
    private TextManager textManager;
    private TextRenderer textRenderer;
    private CurrentLineManager currentLineManager;
    private SelectionManager selectionManager;

    public void Init(
        CoreTextControlBox coreTextbox,
        TextRenderer textRenderer, 
        TextManager textManager, 
        CursorManager cursorManager, 
        CanvasUpdateManager canvasUpdateManager, 
        ScrollManager scrollManager, 
        SelectionRenderer selectionRenderer, 
        SelectionDragDropManager selectionDragDropManager,
        CurrentLineManager currentLineManager,
        SelectionManager selectionManager
        )
    {
        this.currentLineManager = currentLineManager;
        this.selectionRenderer = selectionRenderer;
        this.selectionDragDropManager = selectionDragDropManager;
        this.coreTextbox = coreTextbox;
        this.cursorManager = cursorManager;
        this.textManager = textManager;
        this.textRenderer = textRenderer;
        this.scrollManager = scrollManager;
        this.canvasUpdateManager = canvasUpdateManager;
        this.selectionManager = selectionManager;
    }


    public void PointerReleasedAction(Point point)
    {
        OldTouchPosition = null;
        selectionRenderer.IsSelectingOverLinenumbers = false;

        //End text drag/drop -> insert text at cursorposition
        if (selectionDragDropManager.isDragDropSelection && !selectionDragDropManager.DragDropOverSelection(point))
            selectionDragDropManager.DoDragDropSelection();
        else if (selectionDragDropManager.isDragDropSelection)
            selectionDragDropManager.EndDragDropSelection();

        if (selectionRenderer.IsSelecting)
            coreTextbox.Focus(FocusState.Programmatic);

        selectionRenderer.IsSelecting = false;
    }
    public void PointerMovedAction(Point point)
    {
        if (selectionRenderer.IsSelecting)
        {
            double canvasWidth = Math.Round(coreTextbox.ActualWidth, 2);
            double canvasHeight = Math.Round(coreTextbox.ActualHeight, 2);
            double curPosX = Math.Round(point.X, 2);
            double curPosY = Math.Round(point.Y, 2);

            if (curPosY > canvasHeight - 50)
            {
                scrollManager.VerticalScroll += (curPosY > canvasHeight + 30 ? 20 : (canvasHeight - curPosY) / 180);
                scrollManager.UpdateWhenScrolled();
            }
            else if (curPosY < 50)
            {
                scrollManager.VerticalScroll += curPosY < -30 ? -20 : -(50 - curPosY) / 20;
                scrollManager.UpdateWhenScrolled();
            }

            //Horizontal
            if (curPosX > canvasWidth - 100 || curPosX < 100)
            {
                scrollManager.ScrollIntoViewHorizontal(coreTextbox.canvasText);
                canvasUpdateManager.UpdateAll();
            }
        }

        //Drag drop text -> move the cursor to get the insertion point
        if (selectionDragDropManager.isDragDropSelection)
        {
            selectionDragDropManager.DragDropOverSelection(point);
            CursorHelper.UpdateCursorPosFromPoint(
                coreTextbox.canvasText,
                currentLineManager,
                textRenderer,
                scrollManager,
                point,
                cursorManager.currentCursorPosition);
            
            canvasUpdateManager.UpdateCursor();
        }
        if (selectionRenderer.IsSelecting && !selectionDragDropManager.isDragDropSelection)
        {
            //selection started over the linenumbers:
            if (selectionRenderer.IsSelectingOverLinenumbers)
            {
                Point pointerPos = point;
                pointerPos.Y += textRenderer.SingleLineHeight; //add one more line

                //When the selection reaches the end of the textbox select the last line completely
                if (cursorManager.LineNumber == textManager.LinesCount - 1)
                {
                    pointerPos.Y -= textRenderer.SingleLineHeight; //add one more line
                    pointerPos.X = Utils.MeasureLineLenght(CanvasDevice.GetSharedDevice(), textManager.GetLineText(-1), textRenderer.TextFormat).Width + 10;
                }

                CursorHelper.UpdateCursorPosFromPoint(
                    coreTextbox.canvasText,
                    currentLineManager,
                    textRenderer,
                    scrollManager,
                    pointerPos,
                    cursorManager.currentCursorPosition);
            }
            else //Default selection
                CursorHelper.UpdateCursorPosFromPoint(
                coreTextbox.canvasText,
                currentLineManager,
                textRenderer,
                scrollManager,
                point,
                cursorManager.currentCursorPosition);

            //Update:
            canvasUpdateManager.UpdateCursor();
            selectionRenderer.SetSelectionEnd(cursorManager.LineNumber, cursorManager.CharacterPosition);
            canvasUpdateManager.UpdateSelection();
        }
    }
    public bool CheckTouchInput(PointerPoint point)
    {
        if (point.PointerDeviceType == PointerDeviceType.Touch || point.PointerDeviceType == PointerDeviceType.Pen)
        {
            //Get the touch start position:
            if (!OldTouchPosition.HasValue)
                return true;

            //GEt the dragged offset:
            double scrollX = OldTouchPosition.Value.X - point.Position.X;
            double scrollY = OldTouchPosition.Value.Y - point.Position.Y;
            scrollManager.VerticalScroll += scrollY > 2 ? 2 : scrollY < -2 ? -2 : scrollY;
            scrollManager.HorizontalScroll += scrollX > 2 ? 2 : scrollX < -2 ? -2 : scrollX;
            canvasUpdateManager.UpdateAll();
            return true;
        }
        return false;
    }
    public bool CheckTouchInput_Click(PointerPoint point)
    {
        if (point.PointerDeviceType == PointerDeviceType.Touch || point.PointerDeviceType == PointerDeviceType.Pen)
        {
            OldTouchPosition = point.Position;
            return true;
        }
        return false;
    }
}
