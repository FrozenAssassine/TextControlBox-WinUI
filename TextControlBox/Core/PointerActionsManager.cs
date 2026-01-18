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
    private CoreTextControlBox coreTextbox;
    private ScrollManager scrollManager;
    private CanvasUpdateManager canvasUpdateManager;
    private CursorManager cursorManager;
    private TextManager textManager;
    private TextRenderer textRenderer;
    private CurrentLineManager currentLineManager;
    private SelectionManager selectionManager;
    private LinkHighlightManager linkHighlightManager;

    private int _storedSelectionStartLine = -1;

    public void StartLineSelection(int line)
    {
        _storedSelectionStartLine = line;
        selectionManager.IsSelecting = true;
        selectionManager.IsSelectingOverLinenumbers = true;
    }

    public void Init(
        CoreTextControlBox coreTextbox,
        TextRenderer textRenderer,
        TextManager textManager,
        CursorManager cursorManager,
        CanvasUpdateManager canvasUpdateManager,
        ScrollManager scrollManager,
        SelectionRenderer selectionRenderer,
        CurrentLineManager currentLineManager,
        SelectionManager selectionManager,
        LinkHighlightManager linkHighlightManager
        )
    {
        this.currentLineManager = currentLineManager;
        this.selectionRenderer = selectionRenderer;
        this.coreTextbox = coreTextbox;
        this.cursorManager = cursorManager;
        this.textManager = textManager;
        this.textRenderer = textRenderer;
        this.scrollManager = scrollManager;
        this.canvasUpdateManager = canvasUpdateManager;
        this.selectionManager = selectionManager;
        this.linkHighlightManager = linkHighlightManager;
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

        //change cursor when clicking on links
        if (linkHighlightManager.NeedsCheckLinkHighlights())
        {
            if (Utils.IsKeyPressed(VirtualKey.Control))
            {
                linkHighlightManager.CheckLinkClicked(pointerPosition);
                return;
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
        _storedSelectionStartLine = -1;

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
        if (curPosY > canvasHeight - coreTextbox.SelectionScrollStartBorderDistance.Bottom)  //near bottom
        {
            double distance = curPosY - (canvasHeight - coreTextbox.SelectionScrollStartBorderDistance.Bottom); 
            verticalSpeed = Math.Pow(distance / 10, 1.5);
            verticalSpeed = Math.Min(verticalSpeed, 20);
        }
        else if (curPosY < coreTextbox.SelectionScrollStartBorderDistance.Top)  //near top
        {
            double distance = coreTextbox.SelectionScrollStartBorderDistance.Top - curPosY;
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
        if (curPosX > canvasWidth - coreTextbox.SelectionScrollStartBorderDistance.Right)  //near right edge
        {
            double distance = curPosX - (canvasWidth - coreTextbox.SelectionScrollStartBorderDistance.Right);
            horizontalSpeed = Math.Pow(distance / 10, 1.5);
            horizontalSpeed = Math.Min(horizontalSpeed, 15);
        }
        else if (curPosX < coreTextbox.SelectionScrollStartBorderDistance.Left)  //near left edge
        {
            double distance = coreTextbox.SelectionScrollStartBorderDistance.Left - curPosX;
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
        CursorHelper.UpdateCursorPosFromPoint(
            coreTextbox.canvasText,
            currentLineManager,
            textRenderer,
            scrollManager,
            point,
            cursorManager.currentCursorPosition);

        int currentLine = cursorManager.LineNumber;

        // If selection started on line numbers, adjust anchor to behave like line selection
        // Default to current behavior if _storedSelectionStartLine is invalid (e.g. not set)
        if (_storedSelectionStartLine != -1)
        {
            if (currentLine <= _storedSelectionStartLine)
            {
                // Dragging UP or Same Line: Anchor should be at End of Start Line
                int anchorLine = _storedSelectionStartLine;

                if (anchorLine < textManager.LinesCount - 1)
                {
                    selectionManager.selectionStart.LineNumber = anchorLine + 1;
                    selectionManager.selectionStart.CharacterPosition = 0;
                }
                else
                {
                    selectionManager.selectionStart.LineNumber = anchorLine;
                    selectionManager.selectionStart.CharacterPosition = textManager.GetLineLength(anchorLine);
                }

                // Cursor is at start of current line
                cursorManager.currentCursorPosition.CharacterPosition = 0;
            }
            else
            {
                // Dragging DOWN: Anchor should be at Start of Start Line
                selectionManager.selectionStart.LineNumber = _storedSelectionStartLine;
                selectionManager.selectionStart.CharacterPosition = 0;

                // Cursor should include the full current line
                if (currentLine < textManager.LinesCount - 1)
                {
                    cursorManager.currentCursorPosition.LineNumber = currentLine + 1;
                    cursorManager.currentCursorPosition.CharacterPosition = 0;
                }
                else
                {
                    cursorManager.currentCursorPosition.LineNumber = currentLine;
                    cursorManager.currentCursorPosition.CharacterPosition = textManager.GetLineLength(currentLine);
                }
            }
        }
        else
        {
            if (cursorManager.LineNumber < textManager.LinesCount - 1)
            {
                cursorManager.currentCursorPosition.LineNumber += 1;
                cursorManager.currentCursorPosition.CharacterPosition = 0;
            }
            else
            {
                cursorManager.currentCursorPosition.CharacterPosition = textManager.GetLineLength(cursorManager.LineNumber);
            }
        }
    }

    public void PointerMovedAction(Point point)
    {
        //if the user moves the pointer before the delay expires, it is a selection
        if (selectionManager.IsSelecting)
        {
            //handle pointer moved
            HandleScrollingWhileSelecting(point);

            //Drag drop text -> move the cursor to get the insertion point
            if (selectionManager.IsSelecting)
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

        //change cursor over links
        if (linkHighlightManager.NeedsCheckLinkHighlights())
        {
            if (Utils.IsKeyPressed(VirtualKey.Control))
                linkHighlightManager.CheckLinkHover(point);
            else
                linkHighlightManager.ResetCursorAfterHover();
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
