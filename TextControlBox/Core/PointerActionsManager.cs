using Microsoft.Graphics.Canvas;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using System;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Core.Selection;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Helper;
using Windows.Foundation;
using Windows.System;

namespace TextControlBoxNS.Core;

internal class PointerActionsManager
{
    private Point? OldTouchPosition = null;
    public int PointerClickCount = 0;
    public DispatcherTimer PointerClickTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 200) };
    private DispatcherTimer selectionTimer;
    private bool isPendingCursorPlacement = false;

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
        InitTimer();
    }

    private void InitTimer()
    {
        selectionTimer = new DispatcherTimer();
        selectionTimer.Interval = TimeSpan.FromMilliseconds(200);
        selectionTimer.Tick += (s, e) =>
        {
            selectionTimer.Stop();
            if (isPendingCursorPlacement)
            {
                selectionManager.IsSelecting = false;
                canvasUpdateManager.UpdateCursor();
            }
        };
    }

    private void HandleDoubleClicked(Point pointerPosition)
    {
        isPendingCursorPlacement = true;
        selectionTimer.Start();

        CursorHelper.UpdateCursorPosFromPoint(coreTextbox.canvasText,
                currentLineManager,
                textRenderer,
                scrollManager,
                pointerPosition,
                cursorManager.currentCursorPosition);

        selectionManager.SelectSingleWord(canvasUpdateManager);
    }

    private void HandleTripleClick()
    {
        PointerClickTimer.Stop();
        PointerClickCount = 0;

        coreTextbox.SelectLine(cursorManager.LineNumber);
        return;
    }

    private void HandleSingleRightClick(object sender, Point pointerPosition)
    {
        if (!SelectionHelper.PointerIsOverSelection(textRenderer, selectionManager, pointerPosition))
        {
            if (!selectionManager.HasSelection)
            {
                CursorHelper.UpdateCursorPosFromPoint(coreTextbox.canvasText,
                    currentLineManager,
                    textRenderer,
                    scrollManager,
                    pointerPosition,
                    cursorManager.currentCursorPosition);
            }
        }

        if (!coreTextbox.ContextFlyoutDisabled && coreTextbox.ContextFlyout != null)
        {
            coreTextbox.ContextFlyout.ShowAt(sender as FrameworkElement, new FlyoutShowOptions { Position = pointerPosition });
        }
    }

    private void HandleSingleLeftClick(Point pointerPosition)
    {
        isPendingCursorPlacement = true;
        selectionTimer.Start();

        CursorHelper.UpdateCursorPosFromPoint(coreTextbox.canvasText,
            currentLineManager,
            textRenderer,
            scrollManager,
            pointerPosition,
            cursorManager.currentCursorPosition);

        //Text drag/drop
        if (selectionManager.HasSelection)
        {
            if (SelectionHelper.PointerIsOverSelection(textRenderer, selectionManager, pointerPosition) && !selectionDragDropManager.isDragDropSelection)
            {
                PointerClickCount = 0;
                selectionDragDropManager.isDragDropSelection = true;

                return;
            }

            //End the selection by pressing on it
            if (selectionDragDropManager.isDragDropSelection && selectionDragDropManager.DragDropOverSelection(pointerPosition))
            {
                selectionDragDropManager.EndDragDropSelection(true);
            }
        }

        //Clear the selection when pressing anywhere
        if (selectionManager.HasSelection)
        {
            selectionManager.ClearSelection();
            selectionManager.SetSelectionStart(cursorManager.currentCursorPosition);
        }
        else
        {
            selectionManager.SetSelectionStart(cursorManager.currentCursorPosition);
        }
    }

    private void HandleSingleClick(Point pointerPosition, bool rightButtonPressed, bool leftButtonPressed, object sender)
    {
        //show the rightclick menu or clear selection
        if (rightButtonPressed)
            HandleSingleRightClick(sender, pointerPosition);

        //Shift + click = set selection
        if (Utils.IsKeyPressed(VirtualKey.Shift) && leftButtonPressed)
        {
            if (selectionManager.selectionStart.IsNull)
                selectionManager.SetSelectionStart(cursorManager.currentCursorPosition);

            CursorHelper.UpdateCursorPosFromPoint(coreTextbox.canvasText,
                currentLineManager,
                textRenderer,
                scrollManager,
                pointerPosition,
                cursorManager.currentCursorPosition);

            selectionManager.SetSelectionEnd(cursorManager.currentCursorPosition);
            canvasUpdateManager.UpdateSelection();
            canvasUpdateManager.UpdateCursor();
            return;
        }

        //click
        if (leftButtonPressed)
        {
            HandleSingleLeftClick(pointerPosition);
        }
        canvasUpdateManager.UpdateSelection();
        canvasUpdateManager.UpdateCursor();
    }

    public void PointerPressedAction(object sender, Point pointerPosition, PointerPointProperties properties)
    {
        bool leftButtonPressed = properties.IsLeftButtonPressed;
        bool rightButtonPressed = properties.IsRightButtonPressed;


        if (leftButtonPressed && !Utils.IsKeyPressed(VirtualKey.Shift))
            PointerClickCount++;

        if (PointerClickTimer.IsEnabled)
        {
            PointerClickTimer.Stop();
        }

        PointerClickTimer.Start();
        PointerClickTimer.Tick += (s, t) =>
        {
            PointerClickTimer.Stop();
            PointerClickCount = 0;
        };

        if (PointerClickCount == 3)
            HandleTripleClick();
        else if (PointerClickCount == 2)
            HandleDoubleClicked(pointerPosition);
        else
            HandleSingleClick(pointerPosition, rightButtonPressed, leftButtonPressed, sender);
    }

    public void PointerReleasedAction(Point point)
    {
        selectionTimer.Stop();
        isPendingCursorPlacement = false;

        OldTouchPosition = null;
        selectionManager.IsSelectingOverLinenumbers = false;

        if (selectionDragDropManager.isDragDropSelection && !selectionDragDropManager.DragDropOverSelection(point))
            selectionDragDropManager.DoDragDropSelection();
        else if (selectionDragDropManager.isDragDropSelection)
            selectionDragDropManager.EndDragDropSelection();

        if (selectionManager.IsSelecting)
            coreTextbox.Focus(FocusState.Programmatic);

        selectionManager.IsSelecting = false;
    }

    private void HandleScrollingWhileSelecting(Point point)
    {
        double canvasWidth = Math.Round(coreTextbox.ActualWidth, 2);
        double canvasHeight = Math.Round(coreTextbox.ActualHeight, 2);
        double curPosX = Math.Round(point.X, 2);
        double curPosY = Math.Round(point.Y, 2);

        //vertical Scrolling
        double verticalSpeed = 0;
        if (curPosY > canvasHeight - 50)  //near bottom
        {
            double distance = curPosY - (canvasHeight - 50); 
            verticalSpeed = Math.Pow(distance / 10, 1.5);
            verticalSpeed = Math.Min(verticalSpeed, 20);
        }
        else if (curPosY < 50)  //near top
        {
            double distance = 50 - curPosY;
            verticalSpeed = -Math.Pow(distance / 10, 1.5);
            verticalSpeed = Math.Max(verticalSpeed, -20);
        }

        if (verticalSpeed != 0)
        {
            scrollManager.VerticalScroll += verticalSpeed;
            scrollManager.UpdateWhenScrolled();
        }

        //horizontal scrolling
        double horizontalSpeed = 0;
        if (curPosX > canvasWidth - 80)  //near right edge
        {
            double distance = curPosX - (canvasWidth - 80);
            horizontalSpeed = Math.Pow(distance / 10, 1.5);
            horizontalSpeed = Math.Min(horizontalSpeed, 15);
        }
        else if (curPosX < 80)  //near left edge
        {
            double distance = 80 - curPosX;
            horizontalSpeed = -Math.Pow(distance / 10, 1.5);
            horizontalSpeed = Math.Max(horizontalSpeed, -15);
        }

        if (horizontalSpeed != 0)
        {
            scrollManager.HorizontalScroll += horizontalSpeed;
            scrollManager.UpdateWhenScrolled();
        }
    }
    
    
    private void PointerMovedDragDrop(Point point)
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
    private void PointerMovedOverLinenumbers(Point point)
    {
        Point pointerPos = point;
        pointerPos.Y += textRenderer.SingleLineHeight; //add one more line

        //if the selection reaches the end of the textbox select the last line completely
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

    public void PointerMovedAction(Point point)
    {
        //if the user moves the pointer before the delay expires, it is a selection
        if (selectionManager.IsSelecting || selectionDragDropManager.isDragDropSelection)
        {
            //handle pointer moved
            HandleScrollingWhileSelecting(point);

            //Drag drop text -> move the cursor to get the insertion point
            if (selectionDragDropManager.isDragDropSelection)
            {
                PointerMovedDragDrop(point);
            }
            else if (selectionManager.IsSelecting)
            {
                //Selection over linenumbers
                if (selectionManager.IsSelectingOverLinenumbers)
                {
                    PointerMovedOverLinenumbers(point);
                }
                else //Default selection
                {
                    CursorHelper.UpdateCursorPosFromPoint(
                        coreTextbox.canvasText,
                        currentLineManager,
                        textRenderer,
                        scrollManager,
                        point,
                        cursorManager.currentCursorPosition);
                }

                canvasUpdateManager.UpdateCursor();
                selectionManager.SetSelectionEnd(cursorManager.LineNumber, cursorManager.CharacterPosition);
                canvasUpdateManager.UpdateSelection();
            }
            return;
        }

        if (isPendingCursorPlacement)
        {
            isPendingCursorPlacement = false;
            selectionTimer.Stop();
            selectionManager.IsSelecting = true;
        }
        return;
    }
    
    public void PointerWheelAction(ZoomManager zoomManager, PointerRoutedEventArgs e)
    {
        var delta = e.GetCurrentPoint(coreTextbox.canvasSelection).Properties.MouseWheelDelta;
        bool needsUpdate = false;
        //Zoom using mousewheel
        if (Utils.IsKeyPressed(VirtualKey.Control))
        {
            zoomManager._ZoomFactor += delta / 20;
            zoomManager.UpdateZoom();
            return;
        }
        //Scroll horizontal using mousewheel
        else if (Utils.IsKeyPressed(VirtualKey.Shift))
        {
            scrollManager.horizontalScrollBar.Value -= delta * scrollManager._HorizontalScrollSensitivity;
            needsUpdate = true;
        }
        //Scroll horizontal using touchpad
        else if (e.GetCurrentPoint(coreTextbox.canvasSelection).Properties.IsHorizontalMouseWheel)
        {
            scrollManager.horizontalScrollBar.Value += delta * scrollManager._HorizontalScrollSensitivity;
            needsUpdate = true;
        }
        //Scroll vertical using mousewheel
        else
        {
            scrollManager.verticalScrollBar.Value -= (delta * scrollManager._VerticalScrollSensitivity) / scrollManager.DefaultVerticalScrollSensitivity;
            //Only update when a line was scrolled
            if ((int)(scrollManager.verticalScrollBar.Value / textRenderer.SingleLineHeight * scrollManager.DefaultVerticalScrollSensitivity) != textRenderer.NumberOfStartLine)
            {
                needsUpdate = true;
            }
        }

        if (selectionManager.IsSelecting)
        {
            CursorHelper.UpdateCursorPosFromPoint(coreTextbox.canvasText,
                currentLineManager,
                textRenderer,
                scrollManager,
                e.GetCurrentPoint(coreTextbox.canvasSelection).Position,
                cursorManager.currentCursorPosition);

            canvasUpdateManager.UpdateCursor();

            selectionManager.SetSelectionEnd(cursorManager.currentCursorPosition);
            selectionManager.IsSelecting = true;
            needsUpdate = true;
        }
        if (needsUpdate)
            canvasUpdateManager.UpdateAll();
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
