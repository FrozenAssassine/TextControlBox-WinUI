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

internal enum PointerSelectionMode
{
    Character,
    Word,
    Line
}

internal enum TouchInteractionState
{
    None,
    PendingTapOrScroll,
    Scrolling,
    Selecting,
    Pinching
}

internal class PointerActionsManager
{
    public int PointerClickCount = 0;
    public DispatcherTimer PointerClickTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 250) };
    private DispatcherTimer selectionTimer;
    private bool isPendingCursorPlacement = false;

    private PointerSelectionMode _selectionMode = PointerSelectionMode.Character;
    private CursorPosition _wordSelectionAnchorStart = new CursorPosition(0, 0);
    private CursorPosition _wordSelectionAnchorEnd = new CursorPosition(0, 0);

    private const double TouchSlopThreshold = 18.0;
    private TouchInteractionState _touchState = TouchInteractionState.None;
    public TouchInteractionState TouchState => _touchState;

    private uint _primaryTouchId = 0;
    private uint? _secondaryTouchId = null;
    private Point _touchStartPoint;
    private Point _lastTouchPoint;
    private Point _secondaryTouchPoint;
    private long _touchStartTimestamp;
    private long _lastTouchTimestamp;

    private DispatcherTimer _touchLongPressTimer;

    private int _touchTapCount = 0;
    private DispatcherTimer _touchTapTimer;
    private Point _lastTapPoint;

    private DispatcherTimer _selectionAutoScrollTimer;
    private DispatcherTimer _zoomAnchorResetTimer;
    private Point _lastSelectionPoint;
    private double _autoScrollSpeedY = 0;
    private double _autoScrollSpeedX = 0;

    private double _initialPinchDistance = 0;
    private int _initialPinchZoomFactor = 100;

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
        _selectionMode = PointerSelectionMode.Line;
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

        PointerClickTimer.Interval = TimeSpan.FromMilliseconds(250);
        PointerClickTimer.Tick += (s, e) =>
        {
            PointerClickTimer.Stop();
            PointerClickCount = 0;
        };

        _touchLongPressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(450) };
        _touchLongPressTimer.Tick += OnTouchLongPressTimerTick;

        _touchTapTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(380) };
        _touchTapTimer.Tick += (s, e) =>
        {
            _touchTapTimer.Stop();
            _touchTapCount = 0;
        };

        _selectionAutoScrollTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(25) };
        _selectionAutoScrollTimer.Tick += OnSelectionAutoScrollTimerTick;

        _zoomAnchorResetTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _zoomAnchorResetTimer.Tick += (s, e) =>
        {
            _zoomAnchorResetTimer.Stop();
            coreTextbox?.zoomManager?.ResetZoomAnchors();
            _accumulatedZoomDelta = 0;
            coreTextbox?.SyncScrollTrackerToOffsetNow();
        };
    }

    private void HandleDoubleClicked(Point pointerPosition)
    {
        isPendingCursorPlacement = false;
        selectionTimer.Stop();

        CursorHelper.UpdateCursorPosFromPoint(coreTextbox.canvasText,
                currentLineManager,
                textRenderer,
                scrollManager,
                pointerPosition,
                cursorManager.currentCursorPosition);

        if (textManager.LinesCount == 0)
            return;

        int line = cursorManager.LineNumber;
        string lineText = textManager.GetLineText(line);
        var (wordStart, wordEnd) = SelectionHelper.GetWordBoundaries(lineText, cursorManager.CharacterPosition);

        _wordSelectionAnchorStart.SetChangeValues(line, wordStart);
        _wordSelectionAnchorEnd.SetChangeValues(line, wordEnd);

        selectionManager.SetSelection(_wordSelectionAnchorStart, _wordSelectionAnchorEnd);
        cursorManager.SetCursorPosition(line, wordEnd);

        _selectionMode = PointerSelectionMode.Word;
        selectionManager.IsSelecting = true;

        canvasUpdateManager.UpdateSelection();
        canvasUpdateManager.UpdateCursor();
    }

    private void HandleTripleClick(Point pointerPosition)
    {
        PointerClickTimer.Stop();
        PointerClickCount = 0;
        isPendingCursorPlacement = false;
        selectionTimer.Stop();

        CursorHelper.UpdateCursorPosFromPoint(
            coreTextbox.canvasText,
            currentLineManager,
            textRenderer,
            scrollManager,
            pointerPosition,
            cursorManager.currentCursorPosition);

        if (textManager.LinesCount == 0)
            return;

        int line = cursorManager.LineNumber;
        _storedSelectionStartLine = line;
        _selectionMode = PointerSelectionMode.Line;
        selectionManager.IsSelecting = true;

        PointerMovedLineSelection(pointerPosition);
    }

    internal void ShowContextFlyout(FrameworkElement target, Point pointerPosition)
    {
        if (coreTextbox == null || coreTextbox.ContextFlyoutDisabled || coreTextbox.ContextFlyout == null)
            return;

        var element = target ?? coreTextbox.canvasSelection;
        coreTextbox.ContextFlyout.ShowAt(element, new FlyoutShowOptions { Position = pointerPosition });
    }

    internal void ShowSelectionFlyout(FrameworkElement target, Point pointerPosition)
    {
        if (coreTextbox == null || coreTextbox.ContextFlyoutDisabled || coreTextbox.flyoutHelper == null)
            return;

        var element = target ?? coreTextbox.canvasSelection;
        coreTextbox.flyoutHelper.ShowSelectionFlyout(element, pointerPosition, coreTextbox);
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
                canvasUpdateManager.UpdateCursor();
            }
        }

        ShowContextFlyout(sender as FrameworkElement, pointerPosition);
    }

    private void HandleSingleLeftClick(Point pointerPosition)
    {
        _selectionMode = PointerSelectionMode.Character;
        isPendingCursorPlacement = true;
        selectionTimer.Start();

        CursorHelper.UpdateCursorPosFromPoint(coreTextbox.canvasText,
            currentLineManager,
            textRenderer,
            scrollManager,
            pointerPosition,
            cursorManager.currentCursorPosition,
            isSelecting: false);

        //change cursor when clicking on links
        if (linkHighlightManager.NeedsCheckLinkHighlights())
        {
            if (Utils.IsKeyPressed(VirtualKey.Control))
            {
                linkHighlightManager.CheckLinkClicked(pointerPosition);
                return;
            }
        }

        // For selectionStart, compute position with isSelecting = true so dragging from right-outside includes the last character
        CursorPosition selStart = new CursorPosition(0, 0);
        CursorHelper.UpdateCursorPosFromPoint(coreTextbox.canvasText,
            currentLineManager,
            textRenderer,
            scrollManager,
            pointerPosition,
            selStart,
            isSelecting: true);

        //Clear the selection when pressing anywhere
        if (selectionManager.HasSelection)
        {
            selectionManager.ClearSelection();
            selectionManager.SetSelectionStart(selStart);
        }
        else
        {
            selectionManager.SetSelectionStart(selStart);
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
                cursorManager.currentCursorPosition,
                isSelecting: true);

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
        PointerPressedAction(sender, pointerPosition, properties.IsLeftButtonPressed, properties.IsRightButtonPressed);
    }

    public void PointerPressedAction(object sender, Point pointerPosition, bool leftButtonPressed, bool rightButtonPressed)
    {
        coreTextbox.Focus(FocusState.Programmatic);
        cursorManager.ResetPreferredPosition();
        coreTextbox.undoRedo.EndBatch();

        if (leftButtonPressed && !Utils.IsKeyPressed(VirtualKey.Shift))
            PointerClickCount++;

        if (PointerClickTimer.IsEnabled)
        {
            PointerClickTimer.Stop();
        }

        PointerClickTimer.Start();

        if (PointerClickCount == 3)
            HandleTripleClick(pointerPosition);
        else if (PointerClickCount == 2)
            HandleDoubleClicked(pointerPosition);
        else
            HandleSingleClick(pointerPosition, rightButtonPressed, leftButtonPressed, sender);
    }

    public void PointerReleasedAction(Point point)
    {
        selectionTimer.Stop();
        StopSelectionAutoScroll();
        isPendingCursorPlacement = false;

        selectionManager.IsSelectingOverLinenumbers = false;
        _storedSelectionStartLine = -1;

        if (selectionManager.IsSelecting)
            coreTextbox.Focus(FocusState.Programmatic);

        selectionManager.IsSelecting = false;

        // If drag modified word selection beyond initial word anchor, reset click count so next click starts fresh
        if (_selectionMode == PointerSelectionMode.Word &&
            (selectionManager.selectionEnd.LineNumber != _wordSelectionAnchorEnd.LineNumber ||
             selectionManager.selectionEnd.CharacterPosition != _wordSelectionAnchorEnd.CharacterPosition))
        {
            PointerClickTimer.Stop();
            PointerClickCount = 0;
        }

        _selectionMode = PointerSelectionMode.Character;
    }

    private void CalculateAutoScrollSpeeds(Point point)
    {
        double canvasWidth = Math.Round(coreTextbox.ActualWidth, 2);
        double canvasHeight = Math.Round(coreTextbox.ActualHeight, 2);
        double curPosX = Math.Round(point.X, 2);
        double curPosY = Math.Round(point.Y, 2);

        double borderBottom = coreTextbox.SelectionScrollStartBorderDistance.Bottom > 0 
            ? coreTextbox.SelectionScrollStartBorderDistance.Bottom 
            : 35.0;
        double borderTop = coreTextbox.SelectionScrollStartBorderDistance.Top > 0 
            ? coreTextbox.SelectionScrollStartBorderDistance.Top 
            : 35.0;
        double borderRight = coreTextbox.SelectionScrollStartBorderDistance.Right > 0 
            ? coreTextbox.SelectionScrollStartBorderDistance.Right 
            : 30.0;
        double borderLeft = coreTextbox.SelectionScrollStartBorderDistance.Left > 0 
            ? coreTextbox.SelectionScrollStartBorderDistance.Left 
            : 30.0;

        // Vertical Scrolling
        _autoScrollSpeedY = 0;
        if (curPosY > canvasHeight - borderBottom) // near bottom
        {
            double distance = curPosY - (canvasHeight - borderBottom);
            _autoScrollSpeedY = Math.Min(25, Math.Max(2, Math.Pow(distance / 8.0, 1.4)));
        }
        else if (curPosY < borderTop) // near top
        {
            double distance = borderTop - curPosY;
            _autoScrollSpeedY = -Math.Min(25, Math.Max(2, Math.Pow(distance / 8.0, 1.4)));
        }

        // Horizontal Scrolling
        _autoScrollSpeedX = 0;
        if (curPosX > canvasWidth - borderRight) // near right edge
        {
            double distance = curPosX - (canvasWidth - borderRight);
            _autoScrollSpeedX = Math.Min(20, Math.Max(2, Math.Pow(distance / 8.0, 1.4)));
        }
        else if (curPosX < borderLeft) // near left edge
        {
            double distance = borderLeft - curPosX;
            _autoScrollSpeedX = -Math.Min(20, Math.Max(2, Math.Pow(distance / 8.0, 1.4)));
        }
    }

    private void HandleScrollingWhileSelecting(Point point)
    {
        _lastSelectionPoint = point;
        CalculateAutoScrollSpeeds(point);

        if (_autoScrollSpeedY != 0 || _autoScrollSpeedX != 0)
        {
            if (_selectionAutoScrollTimer != null && !_selectionAutoScrollTimer.IsEnabled)
            {
                _selectionAutoScrollTimer.Start();
            }
        }
        else
        {
            StopSelectionAutoScroll();
        }
    }

    private void OnSelectionAutoScrollTimerTick(object sender, object e)
    {
        bool isSelecting = selectionManager.IsSelecting || _touchState == TouchInteractionState.Selecting;
        if (!isSelecting || (_autoScrollSpeedY == 0 && _autoScrollSpeedX == 0))
        {
            StopSelectionAutoScroll();
            return;
        }

        if (_autoScrollSpeedY != 0)
        {
            scrollManager.VerticalScroll += _autoScrollSpeedY;
        }
        if (_autoScrollSpeedX != 0)
        {
            scrollManager.HorizontalScroll += _autoScrollSpeedX;
        }

        scrollManager.UpdateWhenScrolled();

        UpdateSelectionDuringScroll(_lastSelectionPoint);
        CalculateAutoScrollSpeeds(_lastSelectionPoint);
    }

    private void StopSelectionAutoScroll()
    {
        _selectionAutoScrollTimer?.Stop();
        _autoScrollSpeedY = 0;
        _autoScrollSpeedX = 0;
    }

    private void UpdateSelectionDuringScroll(Point point)
    {
        if (_selectionMode == PointerSelectionMode.Line || selectionManager.IsSelectingOverLinenumbers)
        {
            PointerMovedLineSelection(point);
        }
        else if (_selectionMode == PointerSelectionMode.Word)
        {
            PointerMovedWordSelection(point);
        }
        else
        {
            CursorHelper.UpdateCursorPosFromPoint(
                coreTextbox.canvasText,
                currentLineManager,
                textRenderer,
                scrollManager,
                point,
                cursorManager.currentCursorPosition,
                isSelecting: true);

            canvasUpdateManager.UpdateCursor();
            selectionManager.SetSelectionEnd(cursorManager.LineNumber, cursorManager.CharacterPosition);
            canvasUpdateManager.UpdateSelection();
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

    private void PointerMovedLineSelection(Point point)
    {
        CursorHelper.UpdateCursorPosFromPoint(
            coreTextbox.canvasText,
            currentLineManager,
            textRenderer,
            scrollManager,
            point,
            cursorManager.currentCursorPosition);

        if (textManager.LinesCount == 0)
            return;

        int currentLine = cursorManager.LineNumber;

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
                cursorManager.currentCursorPosition.LineNumber = currentLine;
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

        selectionManager.selectionStart.IsNull = false;
        selectionManager.selectionEnd.IsNull = false;
        selectionManager.selectionEnd.SetChangeValues(cursorManager.currentCursorPosition);
        selectionManager.HasSelection = SelectionHelper.TextIsSelected(selectionManager.selectionStart, selectionManager.selectionEnd);

        canvasUpdateManager.UpdateCursor();
        canvasUpdateManager.UpdateSelection();
    }

    private void PointerMovedWordSelection(Point point)
    {
        CursorHelper.UpdateCursorPosFromPoint(
            coreTextbox.canvasText,
            currentLineManager,
            textRenderer,
            scrollManager,
            point,
            cursorManager.currentCursorPosition,
            isSelecting: true);

        if (textManager.LinesCount == 0)
            return;

        int curLine = cursorManager.LineNumber;
        int curChar = cursorManager.CharacterPosition;
        string lineText = textManager.GetLineText(curLine);
        var (targetWordStart, targetWordEnd) = SelectionHelper.GetWordBoundaries(lineText, curChar);

        // Forward: pointer is at or after anchor word end
        if (curLine > _wordSelectionAnchorEnd.LineNumber ||
            (curLine == _wordSelectionAnchorEnd.LineNumber && curChar >= _wordSelectionAnchorEnd.CharacterPosition))
        {
            selectionManager.selectionStart.SetChangeValues(_wordSelectionAnchorStart);
            cursorManager.currentCursorPosition.SetChangeValues(curLine, targetWordEnd);
            selectionManager.selectionEnd.SetChangeValues(cursorManager.currentCursorPosition);
        }
        // Backward: pointer is at or before anchor word start
        else if (curLine < _wordSelectionAnchorStart.LineNumber ||
                 (curLine == _wordSelectionAnchorStart.LineNumber && curChar <= _wordSelectionAnchorStart.CharacterPosition))
        {
            selectionManager.selectionStart.SetChangeValues(_wordSelectionAnchorEnd);
            cursorManager.currentCursorPosition.SetChangeValues(curLine, targetWordStart);
            selectionManager.selectionEnd.SetChangeValues(cursorManager.currentCursorPosition);
        }
        // Inside initial anchor word
        else
        {
            selectionManager.selectionStart.SetChangeValues(_wordSelectionAnchorStart);
            cursorManager.currentCursorPosition.SetChangeValues(_wordSelectionAnchorEnd);
            selectionManager.selectionEnd.SetChangeValues(_wordSelectionAnchorEnd);
        }

        selectionManager.selectionStart.IsNull = false;
        selectionManager.selectionEnd.IsNull = false;
        selectionManager.HasSelection = SelectionHelper.TextIsSelected(selectionManager.selectionStart, selectionManager.selectionEnd);

        canvasUpdateManager.UpdateCursor();
        canvasUpdateManager.UpdateSelection();
    }

    private void PointerMovedOverLinenumbers(Point point)
    {
        PointerMovedLineSelection(point);
    }

    public void PointerMovedAction(Point point)
    {
        //if the user moves the pointer before the delay expires, it is a selection
        if (selectionManager.IsSelecting)
        {
            //handle pointer moved
            HandleScrollingWhileSelecting(point);

            if (selectionManager.IsSelecting)
            {
                if (_selectionMode == PointerSelectionMode.Line || selectionManager.IsSelectingOverLinenumbers)
                {
                    PointerMovedLineSelection(point);
                }
                else if (_selectionMode == PointerSelectionMode.Word)
                {
                    PointerMovedWordSelection(point);
                }
                else //Default selection
                {
                    CursorHelper.UpdateCursorPosFromPoint(
                        coreTextbox.canvasText,
                        currentLineManager,
                        textRenderer,
                        scrollManager,
                        point,
                        cursorManager.currentCursorPosition,
                        isSelecting: true);

                    canvasUpdateManager.UpdateCursor();
                    selectionManager.SetSelectionEnd(cursorManager.LineNumber, cursorManager.CharacterPosition);
                    canvasUpdateManager.UpdateSelection();
                }
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
    
    private double _accumulatedZoomDelta = 0;

    internal void ApplyZoomDelta(ZoomManager zoomManager, int delta)
    {
        _zoomAnchorResetTimer?.Stop();
        _zoomAnchorResetTimer?.Start();

        if (_accumulatedZoomDelta != 0 && Math.Sign(delta) != Math.Sign(_accumulatedZoomDelta))
        {
            _accumulatedZoomDelta = 0;
        }
        _accumulatedZoomDelta += delta;
        const double divisor = 20.0;
        int zoomStep = (int)(_accumulatedZoomDelta / divisor);
        if (zoomStep == 0 && Math.Abs(_accumulatedZoomDelta) >= 10.0)
        {
            zoomStep = Math.Sign(_accumulatedZoomDelta);
        }
        if (zoomStep != 0)
        {
            _accumulatedZoomDelta -= zoomStep * divisor;
            int newZoom = (int)Math.Clamp(zoomManager._ZoomFactor + zoomStep, 4, 400);
            if (newZoom != zoomManager._ZoomFactor)
            {
                zoomManager._ZoomFactor = newZoom;
                zoomManager.UpdateZoom();
            }
        }
    }
    
    public void PointerWheelAction(ZoomManager zoomManager, PointerRoutedEventArgs e)
    {
        var properties = e.GetCurrentPoint(coreTextbox.canvasSelection).Properties;
        var delta = properties.MouseWheelDelta;
        bool needsUpdate = false;
        //Zoom using mousewheel or a precision-touchpad pinch. A pinch gesture is delivered by
        //Windows as a Ctrl-modified wheel, but that Control comes from the wheel message
        //(e.KeyModifiers), not the physical keyboard state, so Utils.IsKeyPressed alone misses it.
        //Check both so pinch-to-zoom works on precision touchpads.
        if (Utils.IsKeyPressed(VirtualKey.Control) || e.KeyModifiers.HasFlag(VirtualKeyModifiers.Control))
        {
            e.Handled = true;
            ApplyZoomDelta(zoomManager, delta);
            return;
        }

        _accumulatedZoomDelta = 0;

        //Scroll horizontal using mousewheel
        if (Utils.IsKeyPressed(VirtualKey.Shift))
        {
            zoomManager.ResetZoomAnchors();
            scrollManager.horizontalScrollBar.Value -= delta * scrollManager._HorizontalScrollSensitivity;
            needsUpdate = true;
        }
        //Scroll horizontal using touchpad
        else if (properties.IsHorizontalMouseWheel)
        {
            zoomManager.ResetZoomAnchors();
            scrollManager.horizontalScrollBar.Value += delta * scrollManager._HorizontalScrollSensitivity;
            needsUpdate = true;
        }
        //Scroll vertical using mousewheel
        else
        {
            zoomManager.ResetZoomAnchors();
            scrollManager.verticalScrollBar.Value -= (delta * scrollManager._VerticalScrollSensitivity) / scrollManager.DefaultVerticalScrollSensitivity;
            if (textRenderer.IsWordWrapEnabled)
            {
                needsUpdate = true;
            }
            else if ((int)(scrollManager.verticalScrollBar.Value / textRenderer.SingleLineHeight * scrollManager.DefaultVerticalScrollSensitivity) != textRenderer.NumberOfStartLine)
            {
                needsUpdate = true;
            }
        }

        if (selectionManager.IsSelecting)
        {
            Point mousePos = e.GetCurrentPoint(coreTextbox.canvasSelection).Position;
            if (_selectionMode == PointerSelectionMode.Line || selectionManager.IsSelectingOverLinenumbers)
            {
                PointerMovedLineSelection(mousePos);
            }
            else if (_selectionMode == PointerSelectionMode.Word)
            {
                PointerMovedWordSelection(mousePos);
            }
            else
            {
                CursorHelper.UpdateCursorPosFromPoint(coreTextbox.canvasText,
                    currentLineManager,
                    textRenderer,
                    scrollManager,
                    mousePos,
                    cursorManager.currentCursorPosition,
                    isSelecting: true);

                canvasUpdateManager.UpdateCursor();
                selectionManager.SetSelectionEnd(cursorManager.currentCursorPosition);
            }

            selectionManager.IsSelecting = true;
            needsUpdate = true;
        }
        if (needsUpdate)
            canvasUpdateManager.UpdateAll();
    }
    
    public void CleanUp()
    {
        PointerClickTimer?.Stop();
        selectionTimer?.Stop();
        _touchLongPressTimer?.Stop();
        _touchTapTimer?.Stop();
        StopSelectionAutoScroll();
    }

    internal void TriggerTouchLongPress()
    {
        OnTouchLongPressTimerTick(null, null);
    }

    private void OnTouchLongPressTimerTick(object sender, object e)
    {
        _touchLongPressTimer?.Stop();
        if (_touchState == TouchInteractionState.PendingTapOrScroll)
        {
            _touchState = TouchInteractionState.Selecting;
            _touchTapCount = 0;
            _touchTapTimer?.Stop();
            HandleDoubleClicked(_touchStartPoint);
        }
    }

    private void HandleTouchTap(Point tapPoint)
    {
        _selectionMode = PointerSelectionMode.Character;
        isPendingCursorPlacement = false;
        selectionTimer?.Stop();

        cursorManager.ResetPreferredPosition();
        coreTextbox.undoRedo.EndBatch();

        CursorHelper.UpdateCursorPosFromPoint(
            coreTextbox.canvasText,
            currentLineManager,
            textRenderer,
            scrollManager,
            tapPoint,
            cursorManager.currentCursorPosition,
            isSelecting: false);

        if (linkHighlightManager.NeedsCheckLinkHighlights() && Utils.IsKeyPressed(VirtualKey.Control))
        {
            linkHighlightManager.CheckLinkClicked(tapPoint);
            return;
        }

        CursorPosition selStart = new CursorPosition(0, 0);
        CursorHelper.UpdateCursorPosFromPoint(
            coreTextbox.canvasText,
            currentLineManager,
            textRenderer,
            scrollManager,
            tapPoint,
            selStart,
            isSelecting: true);

        selectionManager.ClearSelection();
        selectionManager.SetSelectionStart(selStart);

        canvasUpdateManager.UpdateSelection();
        canvasUpdateManager.UpdateCursor();
        coreTextbox.UpdateInputHandlerPosition();
    }

    public void HandleTouchPressed(object sender, PointerPoint point, PointerRoutedEventArgs e)
        => HandleTouchPressed(sender, point.PointerId, point.Position);

    public void HandleTouchPressed(object sender, uint pointerId, Point position)
    {
        coreTextbox.Focus(FocusState.Programmatic);

        if (_touchState == TouchInteractionState.None)
        {
            _primaryTouchId = pointerId;
            _touchStartPoint = position;
            _lastTouchPoint = position;
            _touchStartTimestamp = Environment.TickCount64;
            _lastTouchTimestamp = _touchStartTimestamp;
            _touchState = TouchInteractionState.PendingTapOrScroll;

            double dx = position.X - _lastTapPoint.X;
            double dy = position.Y - _lastTapPoint.Y;
            double tapDist = Math.Sqrt(dx * dx + dy * dy);

            if (_touchTapTimer != null && _touchTapTimer.IsEnabled && tapDist < 35.0)
            {
                _touchTapCount++;
            }
            else
            {
                _touchTapCount = 1;
            }

            _lastTapPoint = position;
            _touchTapTimer?.Stop();
            _touchTapTimer?.Start();

            if (_touchTapCount == 2)
            {
                _touchLongPressTimer?.Stop();
                HandleDoubleClicked(position);
                _touchState = TouchInteractionState.Selecting;
            }
            else if (_touchTapCount >= 3)
            {
                _touchLongPressTimer?.Stop();
                HandleTripleClick(position);
                _touchState = TouchInteractionState.Selecting;
                _touchTapCount = 0;
            }
            else
            {
                _touchLongPressTimer?.Stop();
                _touchLongPressTimer?.Start();
            }
        }
        else if (_touchState != TouchInteractionState.None && !_secondaryTouchId.HasValue && pointerId != _primaryTouchId)
        {
            _secondaryTouchId = pointerId;
            _secondaryTouchPoint = position;
            _touchState = TouchInteractionState.Pinching;
            _touchLongPressTimer?.Stop();

            double dx = _secondaryTouchPoint.X - _lastTouchPoint.X;
            double dy = _secondaryTouchPoint.Y - _lastTouchPoint.Y;
            _initialPinchDistance = Math.Sqrt(dx * dx + dy * dy);
            _initialPinchZoomFactor = coreTextbox?.zoomManager != null ? coreTextbox.zoomManager._ZoomFactor : 100;
        }
    }

    public void HandleTouchMoved(PointerPoint point, PointerRoutedEventArgs e)
        => HandleTouchMoved(point.PointerId, point.Position);

    public void HandleTouchMoved(uint pointerId, Point position)
    {
        if (_touchState == TouchInteractionState.Pinching)
        {
            if (pointerId == _secondaryTouchId)
            {
                _secondaryTouchPoint = position;
            }
            else if (pointerId == _primaryTouchId)
            {
                _lastTouchPoint = position;
            }
            else
            {
                return;
            }

            double dx = _secondaryTouchPoint.X - _lastTouchPoint.X;
            double dy = _secondaryTouchPoint.Y - _lastTouchPoint.Y;
            double currentDist = Math.Sqrt(dx * dx + dy * dy);

            if (_initialPinchDistance > 10 && coreTextbox?.zoomManager != null)
            {
                double scale = currentDist / _initialPinchDistance;
                int newZoom = (int)Math.Round(_initialPinchZoomFactor * scale);
                coreTextbox.zoomManager._ZoomFactor = Math.Clamp(newZoom, 10, 400);
                coreTextbox.zoomManager.UpdateZoom();
            }
            return;
        }

        if (pointerId != _primaryTouchId)
            return;

        double totalDx = position.X - _touchStartPoint.X;
        double totalDy = position.Y - _touchStartPoint.Y;
        double totalDist = Math.Sqrt(totalDx * totalDx + totalDy * totalDy);

        if (_touchState == TouchInteractionState.PendingTapOrScroll)
        {
            if (totalDist > TouchSlopThreshold)
            {
                _touchLongPressTimer?.Stop();
                _touchState = TouchInteractionState.Scrolling;
                coreTextbox._isTouchScrolling = true;
                _lastTouchPoint = position;
                _lastTouchTimestamp = Environment.TickCount64;
            }
        }

        if (_touchState == TouchInteractionState.Scrolling)
        {
            _touchLongPressTimer?.Stop();

            double deltaX = position.X - _lastTouchPoint.X;
            double deltaY = position.Y - _lastTouchPoint.Y;

            _lastTouchPoint = position;
            _lastTouchTimestamp = Environment.TickCount64;

            if (scrollManager?.OffsetSource != null)
            {
                scrollManager.OffsetSource.VerticalOffset -= deltaY;
                scrollManager.OffsetSource.HorizontalOffset -= deltaX;
                canvasUpdateManager.UpdateAll();
            }
        }
        else if (_touchState == TouchInteractionState.Selecting)
        {
            HandleScrollingWhileSelecting(position);

            if (_selectionMode == PointerSelectionMode.Line || selectionManager.IsSelectingOverLinenumbers)
            {
                PointerMovedLineSelection(position);
            }
            else if (_selectionMode == PointerSelectionMode.Word)
            {
                PointerMovedWordSelection(position);
            }
            else
            {
                CursorHelper.UpdateCursorPosFromPoint(
                    coreTextbox.canvasText,
                    currentLineManager,
                    textRenderer,
                    scrollManager,
                    position,
                    cursorManager.currentCursorPosition,
                    isSelecting: true);

                canvasUpdateManager.UpdateCursor();
                selectionManager.SetSelectionEnd(cursorManager.LineNumber, cursorManager.CharacterPosition);
                canvasUpdateManager.UpdateSelection();
            }
        }
    }

    public void HandleTouchReleased(PointerPoint point, PointerRoutedEventArgs e)
        => HandleTouchReleased(point.PointerId, point.Position);

    public void HandleTouchReleased(uint pointerId, Point position)
    {
        _touchLongPressTimer?.Stop();

        if (_touchState == TouchInteractionState.Pinching)
        {
            coreTextbox?.zoomManager?.ResetZoomAnchors();
            if (pointerId == _secondaryTouchId)
            {
                _secondaryTouchId = null;
                _touchState = TouchInteractionState.Scrolling;
                _lastTouchPoint = position;
                _lastTouchTimestamp = Environment.TickCount64;
            }
            else if (pointerId == _primaryTouchId && _secondaryTouchId.HasValue)
            {
                _primaryTouchId = _secondaryTouchId.Value;
                _secondaryTouchId = null;
                _lastTouchPoint = _secondaryTouchPoint;
                _lastTouchTimestamp = Environment.TickCount64;
                _touchState = TouchInteractionState.Scrolling;
            }
            else
            {
                _touchState = TouchInteractionState.None;
                _primaryTouchId = 0;
                _secondaryTouchId = null;
            }
            return;
        }

        if (pointerId != _primaryTouchId)
            return;

        double totalDx = position.X - _touchStartPoint.X;
        double totalDy = position.Y - _touchStartPoint.Y;
        double totalDist = Math.Sqrt(totalDx * totalDx + totalDy * totalDy);
        long duration = Environment.TickCount64 - _touchStartTimestamp;

        bool wasScrolling = _touchState == TouchInteractionState.Scrolling;

        if (_touchState == TouchInteractionState.PendingTapOrScroll ||
            (wasScrolling && totalDist <= TouchSlopThreshold && duration < 400))
        {
            if (_touchTapCount == 1)
            {
                HandleTouchTap(_touchStartPoint);
            }
        }
        else if (_touchState == TouchInteractionState.Selecting)
        {
            selectionManager.IsSelecting = false;
            _selectionMode = PointerSelectionMode.Character;
            StopSelectionAutoScroll();
            _touchTapCount = 0;
            _touchTapTimer?.Stop();
        }

        if (coreTextbox != null)
        {
            coreTextbox._isTouchScrolling = false;
            if (wasScrolling)
            {
                coreTextbox.SyncScrollTrackerToOffsetNow();
            }
        }

        _touchState = TouchInteractionState.None;
        _primaryTouchId = 0;
        _secondaryTouchId = null;
    }

    public void HandleTouchCanceled()
    {
        _touchLongPressTimer?.Stop();
        StopSelectionAutoScroll();
        coreTextbox?.zoomManager?.ResetZoomAnchors();
        if (coreTextbox != null)
        {
            coreTextbox._isTouchScrolling = false;
            coreTextbox.SyncScrollTrackerToOffsetNow();
        }
        _touchState = TouchInteractionState.None;
        _primaryTouchId = 0;
        _secondaryTouchId = null;
        if (selectionManager != null)
            selectionManager.IsSelecting = false;
    }

    public bool CheckTouchInput(PointerPoint point)
    {
        return false;
    }
    public bool CheckTouchInput_Click(PointerPoint point)
    {
        return false;
    }
}
