using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Core.Selection;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Languages;
using TextControlBoxNS.Models;
using TextControlBoxNS.Models.Enums;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System;

namespace TextControlBoxNS.Core;

internal sealed partial class CoreTextControlBox : UserControl, IDisposable
{
    public readonly SelectionRenderer selectionRenderer;
    public readonly FlyoutHelper flyoutHelper;
    public readonly TabSpaceManager tabSpaceManager;
    public readonly StringManager stringManager;
    public readonly SearchManager searchManager;
    public readonly CanvasUpdateManager canvasUpdateManager;
    public readonly LineNumberRenderer lineNumberRenderer;
    public readonly TextManager textManager;
    public readonly UndoRedo undoRedo;
    public readonly SelectionManager selectionManager;
    public readonly CursorManager cursorManager;
    public readonly TextActionManager textActionManager;
    public readonly TextRenderer textRenderer;
    public readonly CursorRenderer cursorRenderer;
    public readonly CaretBlinkManager caretBlinkManager;
    public readonly ScrollManager scrollManager;
    public readonly CurrentLineManager currentLineManager;
    public readonly LongestLineManager longestLineManager;
    public readonly ZoomManager zoomManager;
    public readonly DesignHelper designHelper;
    public readonly LineHighlighterManager lineHighlighterManager;
    public readonly LineNumberManager lineNumberManager;
    public readonly EventsManager eventsManager;
    public readonly FocusManager focusManager;
    public readonly PointerActionsManager pointerActionsManager;
    public readonly TextLayoutManager textLayoutManager;
    public readonly LineHighlighterRenderer lineHighlighterRenderer;
    public readonly AutoIndentionManager autoIndentionManager;
    public readonly ReplaceManager replaceManager;
    public readonly InitializationManager initializationManager;
    public readonly MoveLineManager moveLineManager;
    private readonly WhitespaceCharactersRenderer invisibleCharactersRenderer;
    private readonly WhitespaceCharactersManager whitespaceCharactersManager;
    private readonly LinkHighlightManager linkHighlightManager;
    private readonly LinkRenderer linkRenderer;
    internal bool _isTouchScrolling = false;

    public CanvasControl canvasText;
    public CanvasControl canvasCursor;
    public CanvasControl canvasSelection;
    public CanvasControl canvasLineNumber;
    public Grid mainGrid;
    public Grid scrollGrid;
    public ScrollBar horizontalScrollBar;
    public ScrollBar verticalScrollBar;

    public CoreTextControlBox()
    {
        this.InitializeComponent();

        canvasText = Canvas_Text;
        canvasCursor = Canvas_Cursor;
        canvasSelection = Canvas_Selection;
        canvasLineNumber = Canvas_LineNumber;
        mainGrid = MainGrid;
        scrollGrid = ScrollGrid;
        horizontalScrollBar = HorizontalScrollbar;
        verticalScrollBar = VerticalScrollbar;

        //Classes & Variables:
        textManager = new TextManager();
        cursorManager = new CursorManager();
        selectionManager = new SelectionManager();
        undoRedo = new UndoRedo();
        selectionRenderer = new SelectionRenderer();
        flyoutHelper = new FlyoutHelper();
        stringManager = new StringManager();
        canvasUpdateManager = new CanvasUpdateManager();
        textActionManager = new TextActionManager();
        textRenderer = new TextRenderer();
        cursorRenderer = new CursorRenderer();
        caretBlinkManager = new CaretBlinkManager();
        scrollManager = new ScrollManager();
        currentLineManager = new CurrentLineManager();
        longestLineManager = new LongestLineManager();
        designHelper = new DesignHelper();
        tabSpaceManager = new TabSpaceManager();
        lineHighlighterManager = new LineHighlighterManager();
        lineNumberManager = new LineNumberManager();
        searchManager = new SearchManager();
        eventsManager = new EventsManager();
        lineNumberRenderer = new LineNumberRenderer();
        zoomManager = new ZoomManager();
        focusManager = new FocusManager();
        pointerActionsManager = new PointerActionsManager();
        textLayoutManager = new TextLayoutManager();
        lineHighlighterRenderer = new LineHighlighterRenderer();
        autoIndentionManager = new AutoIndentionManager();
        replaceManager = new ReplaceManager();
        initializationManager = new InitializationManager();
        moveLineManager = new MoveLineManager();
        invisibleCharactersRenderer = new WhitespaceCharactersRenderer();
        whitespaceCharactersManager = new WhitespaceCharactersManager();
        linkHighlightManager = new LinkHighlightManager();
        linkRenderer = new LinkRenderer();

        textManager.Init(eventsManager);
        stringManager.Init(textManager, tabSpaceManager);
        lineHighlighterRenderer.Init(lineHighlighterManager, selectionManager, textRenderer);
        cursorManager.Init(textManager, currentLineManager);
        selectionManager.Init(textManager, cursorManager, eventsManager);
        undoRedo.Init(textManager, selectionManager, cursorManager, eventsManager, tabSpaceManager);
        selectionRenderer.Init(selectionManager, textRenderer, eventsManager, scrollManager, zoomManager, designHelper, textManager);
        flyoutHelper.Init(this);
        canvasUpdateManager.Init(this);
        caretBlinkManager.Init(canvasUpdateManager);
        textActionManager.Init(this, textRenderer, undoRedo, currentLineManager, longestLineManager, canvasUpdateManager, textManager, selectionRenderer, cursorManager, scrollManager, eventsManager, stringManager, selectionManager, autoIndentionManager);
        textRenderer.Init(cursorManager, designHelper, textLayoutManager, textManager, scrollManager, lineNumberRenderer, longestLineManager, this, searchManager, canvasUpdateManager, zoomManager, invisibleCharactersRenderer, linkRenderer, linkHighlightManager);
        cursorRenderer.Init(cursorManager, currentLineManager, textRenderer, focusManager, textManager, scrollManager, zoomManager, designHelper, lineHighlighterRenderer, eventsManager, longestLineManager, caretBlinkManager);
        scrollManager.Init(this, canvasUpdateManager, textManager, textRenderer, cursorManager, zoomManager, VerticalScrollbar, HorizontalScrollbar);
        HookScrollbarTouchHandlers();
        currentLineManager.Init(cursorManager, textManager);
        longestLineManager.Init(selectionManager, textManager, textRenderer);
        designHelper.Init(this, textRenderer, canvasUpdateManager);
        tabSpaceManager.Init(textManager, selectionManager, cursorManager, textActionManager, undoRedo, longestLineManager, eventsManager);
        searchManager.Init(textManager, selectionManager);
        eventsManager.Init(searchManager, cursorManager);
        lineNumberRenderer.Init(textManager, textLayoutManager, textRenderer, designHelper, lineNumberManager, zoomManager);
        zoomManager.Init(textManager, textRenderer, canvasUpdateManager, eventsManager, lineNumberRenderer, scrollManager);
        focusManager.Init(this, canvasUpdateManager, inputHandler, eventsManager);
        pointerActionsManager.Init(this, textRenderer, textManager, cursorManager, canvasUpdateManager, scrollManager, selectionRenderer, currentLineManager, selectionManager, linkHighlightManager);
        textLayoutManager.Init(textManager, zoomManager);
        autoIndentionManager.Init(textManager, tabSpaceManager);
        replaceManager.Init(canvasUpdateManager, undoRedo, textManager, searchManager, cursorManager, textActionManager, selectionRenderer, selectionManager, eventsManager, longestLineManager);
        initializationManager.Init(eventsManager);
        moveLineManager.Init(selectionManager, cursorManager, textManager, undoRedo);
        invisibleCharactersRenderer.Init(designHelper, scrollManager, zoomManager, textLayoutManager, whitespaceCharactersManager);
        linkHighlightManager.Init(textRenderer, this, eventsManager);
        linkRenderer.Init(textRenderer, linkHighlightManager);

        inputHandler.CompositionStarted += InputHandler_CompositionStarted;
        inputHandler.CompositionChanged += InputHandler_CompositionChanged;

        // Two-axis precision-touchpad panning: wire the composition InteractionTracker once the control
        // (and its selection canvas' visual) is in the tree. See CoreTextControlBox.DiagonalScroll.cs.
        Loaded += CoreTextControlBox_Loaded;
        Unloaded += CoreTextControlBox_Unloaded;

        ActualThemeChanged += CoreTextControlBox_ActualThemeChanged;
    }

    private void CoreTextControlBox_Loaded(object sender, RoutedEventArgs e)
    {
        SetupDiagonalScroll();
        if (focusManager.HasFocus)
        {
            caretBlinkManager?.Start();
        }
        canvasUpdateManager?.UpdateAll();
    }

    private void CoreTextControlBox_Unloaded(object sender, RoutedEventArgs e)
    {
        TeardownDiagonalScroll();
        caretBlinkManager?.Stop();
        pointerActionsManager?.CleanUp();
    }

    private void CoreTextControlBox_ActualThemeChanged(FrameworkElement sender, object args)
    {
        if (designHelper.RequestedTheme == ElementTheme.Default)
        {
            designHelper.RequestedTheme = ElementTheme.Default;
        }
    }

    public void InitialiseOnStart()
    {
        if (designHelper.RequestedTheme == ElementTheme.Default)
        {
            designHelper.RequestedTheme = ElementTheme.Default;
        }

        if (textManager.LinesCount == 0)
            textManager.AddLine();

        cursorManager.SetCursorPosition(0, 0);

        selectionManager.ClearSelection();

        zoomManager.UpdateZoom();
        focusManager.SetFocus();

        initializationManager.TextboxInitDone();
    }


    //Handle keyinputs
    private void InputHandler_TextEntered(object sender, TextChangedEventArgs e)
    {
        if (inputHandler.IsComposing)
            return;

        if (IsReadOnly || inputHandler.Text.Equals("\t", StringComparison.OrdinalIgnoreCase))
        {
            inputHandler.Text = ""; //clear text, otherwise in readonly mode, the text is still added in the textbox.
            return;
        }

        //Prevent key-entering if control key is pressed 
        var ctrl = Utils.IsKeyPressed(VirtualKey.Control);
        var menu = Utils.IsKeyPressed(VirtualKey.Menu);
        if (ctrl && !menu || menu && !ctrl)
            return;

        textActionManager.AddCharacter(inputHandler.Text);
        inputHandler.Text = "";
        UpdateInputHandlerPosition();
    }
    private void InputHandler_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        UpdateInputHandlerPosition();

        if (inputHandler.IsComposing || (int)e.Key == 229)
            return;

        if (e.Key == VirtualKey.Tab)
        {
            undoRedo.EndBatch();
            if (IsReadOnly)
            {
                e.Handled = true;
                return;
            }

            if (Utils.IsKeyPressed(VirtualKey.Shift))
                tabSpaceManager.MoveTabBack();
            else
                tabSpaceManager.MoveTab();

            canvasUpdateManager.UpdateAll();

            //mark as handled to not change focus
            e.Handled = true;
        }

        if (!focusManager.HasFocus)
            return;

        var ctrl = Utils.IsKeyPressed(VirtualKey.Control);
        var shift = Utils.IsKeyPressed(VirtualKey.Shift);
        var menu = Utils.IsKeyPressed(VirtualKey.Menu);
        if (ctrl && !shift && !menu)
        {
            switch (e.Key)
            {
                case VirtualKey.Up:
                    ScrollOneLineUp();
                    e.Handled = true;
                    break;
                case VirtualKey.Down:
                    ScrollOneLineDown();
                    e.Handled = true;
                    break;
                case VirtualKey.V:
                    Paste();
                    e.Handled = true;
                    break;
                case VirtualKey.Z:
                    Undo();
                    e.Handled = true;
                    break;
                case VirtualKey.Y:
                    Redo();
                    e.Handled = true;
                    break;
                case VirtualKey.C:
                    Copy();
                    e.Handled = true;
                    break;
                case VirtualKey.X:
                    Cut();
                    e.Handled = true;
                    break;
                case VirtualKey.A:
                    SelectAll();
                    e.Handled = true;
                    break;
                case VirtualKey.W:
                    if (ControlW_SelectWord)
                    {
                        selectionManager.SelectSingleWord(canvasUpdateManager);
                        e.Handled = true;
                    }
                    break;
            }

            if (e.Key != VirtualKey.Home && e.Key != VirtualKey.End && e.Key != VirtualKey.Left && e.Key != VirtualKey.Right && e.Key != VirtualKey.Back && e.Key != VirtualKey.Delete)
                return;
        }

        if (menu)
        {
            if (!IsReadOnly && (e.Key == VirtualKey.Down || e.Key == VirtualKey.Up ))
            {
                moveLineManager.Move(e.Key == VirtualKey.Down ? LineMoveDirection.Down : LineMoveDirection.Up);

                if (textRenderer.OutOfRenderedArea(cursorManager.LineNumber))
                {
                    if (e.Key == VirtualKey.Down)
                        ScrollOneLineDown(false);
                    else if (e.Key == VirtualKey.Up)
                        ScrollOneLineUp(false);
                }

                selectionManager.ClearSelection();
                canvasUpdateManager.UpdateAll();
                e.Handled = true;
                return;
            }
        }

        switch (e.Key)
        {
            case VirtualKey.Enter:
                textActionManager.AddNewLine();
                e.Handled = true;
                break;
            case VirtualKey.Back:
                textActionManager.RemoveText(ctrl);
                e.Handled = true;
                break;
            case VirtualKey.Delete:
                textActionManager.DeleteText(ctrl, shift);
                e.Handled = true;
                break;
            case VirtualKey.Left:
                {
                    undoRedo.EndBatch();
                    if (shift)
                    {
                        selectionManager.StartSelectionIfNeeded();
                        cursorManager.MoveLeft();
                        selectionManager.SetSelectionEnd(cursorManager.currentCursorPosition);
                    }
                    else
                    {
                        //Move the cursor to the start of the selection
                        if (selectionManager.HasSelection && selectionManager.HasSelection)
                            cursorManager.SetCursorPositionCopyValues(selectionManager.GetMin(selectionManager.currentTextSelection));
                        else
                            cursorManager.MoveLeft();

                        selectionManager.ClearSelectionIfNeeded(this);
                    }

                    scrollManager.UpdateScrollToShowCursor(true);
                    e.Handled = true;
                    break;
                }
            case VirtualKey.Right:
                {
                    undoRedo.EndBatch();
                    if (shift)
                    {
                        selectionManager.StartSelectionIfNeeded();
                        cursorManager.MoveRight();
                        selectionManager.SetSelectionEnd(cursorManager.currentCursorPosition);
                    }
                    else
                    {
                        //Move the cursor to the end of the selection
                        if (selectionManager.HasSelection && selectionManager.HasSelection)
                            cursorManager.SetCursorPositionCopyValues(selectionManager.GetMax(selectionManager.currentTextSelection));
                        else
                            cursorManager.MoveRight();

                        selectionManager.ClearSelectionIfNeeded(this);
                    }

                    scrollManager.UpdateScrollToShowCursor(true);
                    e.Handled = true;
                    break;
                }
            case VirtualKey.Down:
                {
                    undoRedo.EndBatch();
                    if (shift)
                    {
                        selectionManager.StartSelectionIfNeeded();
                        if (WordWrap)
                            textRenderer.MoveCursorByVisualRows(canvasText, cursorManager.currentCursorPosition, 1);
                        else
                            cursorManager.MoveDown();
                        selectionManager.SetSelectionEnd(cursorManager.currentCursorPosition);
                    }
                    else
                    {
                        selectionManager.ClearSelectionIfNeeded(this);
                        if (WordWrap)
                            textRenderer.MoveCursorByVisualRows(canvasText, cursorManager.currentCursorPosition, 1);
                        else
                            cursorManager.MoveDown();
                    }

                    scrollManager.UpdateScrollToShowCursor(true);
                    e.Handled = true;
                    break;
                }
            case VirtualKey.Up:
                {
                    undoRedo.EndBatch();
                    if (shift)
                    {
                        selectionManager.StartSelectionIfNeeded();
                        if (WordWrap)
                            textRenderer.MoveCursorByVisualRows(canvasText, cursorManager.currentCursorPosition, -1);
                        else
                            cursorManager.MoveUp();
                        selectionManager.SetSelectionEnd(cursorManager.currentCursorPosition);
                    }
                    else
                    {
                        selectionManager.ClearSelectionIfNeeded(this);
                        if (WordWrap)
                            textRenderer.MoveCursorByVisualRows(canvasText, cursorManager.currentCursorPosition, -1);
                        else
                            cursorManager.MoveUp();
                    }

                    scrollManager.UpdateScrollToShowCursor(true);
                    e.Handled = true;
                    break;
                }
            case VirtualKey.Escape:
                {
                    ClearSelection();
                    e.Handled = true;
                    break;
                }
            case VirtualKey.PageUp:
                undoRedo.EndBatch();
                ScrollPageUp();
                e.Handled = true;
                break;
            case VirtualKey.PageDown:
                undoRedo.EndBatch();
                ScrollPageDown();
                e.Handled = true;
                break;
            case VirtualKey.Home:
            case VirtualKey.End:
                {
                    undoRedo.EndBatch();
                    bool isHome = e.Key == VirtualKey.Home;

                    //start or clear selection
                    if (shift)
                    {
                        selectionManager.StartSelectionIfNeeded();
                    }
                    else
                    {
                        selectionManager.ClearSelection();
                        canvasUpdateManager.UpdateSelection();
                    }

                    //just move the cursor around
                    if (ctrl)
                    {
                        if (isHome)
                            cursorManager.SetToTextStart();
                        else
                            cursorManager.SetToTextEnd();

                        scrollManager.UpdateScrollToShowCursor(true);
                    }
                    else
                    {
                        if (isHome)
                            cursorManager.MoveToLineStart(CursorPosition);
                        else
                            cursorManager.MoveToLineEnd(CursorPosition);

                        scrollManager.ScrollLineIntoViewIfOutside(CursorPosition.LineNumber);
                    }

                    // finish selection
                    if (shift)
                    {
                        selectionManager.SetSelectionEnd(CursorPosition);
                        canvasUpdateManager.UpdateSelection();
                    }
                    else
                    {
                        canvasUpdateManager.UpdateText();
                    }

                    canvasUpdateManager.UpdateCursor();
                    e.Handled = true;
                    break;
                }
        }
        UpdateInputHandlerPosition();
    }

    private void Canvas_Selection_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        Canvas_Selection.ReleasePointerCapture(e.Pointer);

        var point = e.GetCurrentPoint(Canvas_Selection);
        if (point.PointerDeviceType == PointerDeviceType.Touch)
        {
            if (selectionManager.IsSelectingOverLinenumbers)
            {
                pointerActionsManager.PointerReleasedAction(point.Position);
                return;
            }

            pointerActionsManager.HandleTouchReleased(point, e);
            return;
        }

        pointerActionsManager.PointerReleasedAction(point.Position);
    }
    private void Canvas_Selection_PointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        Canvas_Selection.ReleasePointerCapture(e.Pointer);
        pointerActionsManager.HandleTouchCanceled();
    }
    private void Canvas_Selection_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
        pointerActionsManager.HandleTouchCanceled();
    }
    private void Canvas_Selection_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(Canvas_Selection);
        if (point.PointerDeviceType == PointerDeviceType.Touch)
        {
            if (selectionManager.IsSelectingOverLinenumbers)
            {
                pointerActionsManager.PointerMovedAction(point.Position);
                return;
            }

            pointerActionsManager.HandleTouchMoved(point, e);
            return;
        }

        if (!focusManager.HasFocus)
            return;

        pointerActionsManager.PointerMovedAction(point.Position);
    }
    private void Canvas_Selection_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        Canvas_Selection.CapturePointer(e.Pointer);
        selectionManager.IsSelectingOverLinenumbers = false;

        var point = e.GetCurrentPoint(Canvas_Selection);
        if (point.PointerDeviceType == PointerDeviceType.Touch)
        {
            pointerActionsManager.HandleTouchPressed(sender, point, e);
            return;
        }

        pointerActionsManager.PointerPressedAction(sender, point.Position, point.Properties);
    }
    private void Canvas_Selection_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        pointerActionsManager.PointerWheelAction(zoomManager, e);
    }
    private void Canvas_Selection_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (e.PointerDeviceType == PointerDeviceType.Mouse)
        {
            var pos = e.GetPosition(Canvas_Selection);
            pointerActionsManager.ShowContextFlyout(Canvas_Selection, pos);
        }
        e.Handled = true;
    }

    private void HookScrollbarTouchHandlers()
    {
        VerticalScrollbar.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(VerticalScrollbar_PointerPressed), true);
        VerticalScrollbar.AddHandler(UIElement.PointerMovedEvent, new PointerEventHandler(VerticalScrollbar_PointerMoved), true);
        VerticalScrollbar.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(VerticalScrollbar_PointerReleased), true);
        VerticalScrollbar.AddHandler(UIElement.PointerCanceledEvent, new PointerEventHandler(VerticalScrollbar_PointerCanceled), true);
        VerticalScrollbar.AddHandler(UIElement.PointerCaptureLostEvent, new PointerEventHandler(VerticalScrollbar_PointerCaptureLost), true);

        VerticalScrollbarContainer.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(VerticalScrollbar_PointerPressed), true);
        VerticalScrollbarContainer.AddHandler(UIElement.PointerMovedEvent, new PointerEventHandler(VerticalScrollbar_PointerMoved), true);
        VerticalScrollbarContainer.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(VerticalScrollbar_PointerReleased), true);
        VerticalScrollbarContainer.AddHandler(UIElement.PointerCanceledEvent, new PointerEventHandler(VerticalScrollbar_PointerCanceled), true);
        VerticalScrollbarContainer.AddHandler(UIElement.PointerCaptureLostEvent, new PointerEventHandler(VerticalScrollbar_PointerCaptureLost), true);

        HorizontalScrollbar.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(HorizontalScrollbar_PointerPressed), true);
        HorizontalScrollbar.AddHandler(UIElement.PointerMovedEvent, new PointerEventHandler(HorizontalScrollbar_PointerMoved), true);
        HorizontalScrollbar.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(HorizontalScrollbar_PointerReleased), true);
        HorizontalScrollbar.AddHandler(UIElement.PointerCanceledEvent, new PointerEventHandler(HorizontalScrollbar_PointerCanceled), true);
        HorizontalScrollbar.AddHandler(UIElement.PointerCaptureLostEvent, new PointerEventHandler(HorizontalScrollbar_PointerCaptureLost), true);

        HorizontalScrollbarContainer.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(HorizontalScrollbar_PointerPressed), true);
        HorizontalScrollbarContainer.AddHandler(UIElement.PointerMovedEvent, new PointerEventHandler(HorizontalScrollbar_PointerMoved), true);
        HorizontalScrollbarContainer.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(HorizontalScrollbar_PointerReleased), true);
        HorizontalScrollbarContainer.AddHandler(UIElement.PointerCanceledEvent, new PointerEventHandler(HorizontalScrollbar_PointerCanceled), true);
        HorizontalScrollbarContainer.AddHandler(UIElement.PointerCaptureLostEvent, new PointerEventHandler(HorizontalScrollbar_PointerCaptureLost), true);
    }

    private bool _isVerticalScrollbarTouchDragging = false;
    private double _initialTouchScrollbarPosY = 0;
    private double _initialScrollOffsetY = 0;
    private double _touchScrollbarTotalMovementY = 0;

    private bool _isHorizontalScrollbarTouchDragging = false;
    private double _initialTouchScrollbarPosX = 0;
    private double _initialScrollOffsetX = 0;
    private double _touchScrollbarTotalMovementX = 0;

    private void VerticalScrollbar_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        zoomManager.ResetZoomAnchors();
        var point = e.GetCurrentPoint(VerticalScrollbar);
        if (point.PointerDeviceType == PointerDeviceType.Touch)
        {
            if (sender is UIElement el)
            {
                el.CapturePointer(e.Pointer);
            }
            _isVerticalScrollbarTouchDragging = true;
            _isTouchScrolling = true;
            _initialTouchScrollbarPosY = point.Position.Y;
            _initialScrollOffsetY = scrollManager.VerticalScroll;
            _touchScrollbarTotalMovementY = 0;
            e.Handled = true;
        }
    }

    private void VerticalScrollbar_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_isVerticalScrollbarTouchDragging)
        {
            var point = e.GetCurrentPoint(VerticalScrollbar);
            double deltaY = point.Position.Y - _initialTouchScrollbarPosY;
            _touchScrollbarTotalMovementY += Math.Abs(deltaY);
            UpdateVerticalScrollbarFromDelta(deltaY);
            e.Handled = true;
        }
    }

    private void VerticalScrollbar_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_isVerticalScrollbarTouchDragging)
        {
            if (sender is UIElement el)
            {
                el.ReleasePointerCapture(e.Pointer);
            }
            _isVerticalScrollbarTouchDragging = false;
            _isTouchScrolling = false;

            var point = e.GetCurrentPoint(VerticalScrollbar);
            double trackLength = VerticalScrollbar.ActualHeight;
            const double buttonHeight = 16.0;

            if (_touchScrollbarTotalMovementY < 8.0 && trackLength > 0)
            {
                if (point.Position.Y < buttonHeight)
                {
                    scrollManager.ScrollOneLineUp();
                }
                else if (point.Position.Y > trackLength - buttonHeight)
                {
                    scrollManager.ScrollOneLineDown();
                }
                else if (VerticalScrollbar.Maximum > 0)
                {
                    double effectiveLength = trackLength > buttonHeight * 2 ? trackLength - buttonHeight * 2 : trackLength;
                    double effectiveY = Math.Clamp(point.Position.Y - buttonHeight, 0.0, effectiveLength);
                    double fraction = effectiveLength > 0 ? effectiveY / effectiveLength : 0.0;
                    scrollManager.VerticalScroll = fraction * VerticalScrollbar.Maximum;
                }
            }

            SyncScrollTrackerToOffsetNow();
            e.Handled = true;
        }
    }

    private void VerticalScrollbar_PointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        if (_isVerticalScrollbarTouchDragging)
        {
            if (sender is UIElement el)
            {
                el.ReleasePointerCapture(e.Pointer);
            }
            _isVerticalScrollbarTouchDragging = false;
            _isTouchScrolling = false;
            SyncScrollTrackerToOffsetNow();
        }
    }

    private void VerticalScrollbar_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
        _isVerticalScrollbarTouchDragging = false;
        _isTouchScrolling = false;
        SyncScrollTrackerToOffsetNow();
    }

    private void UpdateVerticalScrollbarFromDelta(double deltaY)
    {
        double trackLength = VerticalScrollbar.ActualHeight;
        if (trackLength <= 0 || VerticalScrollbar.Maximum <= 0)
            return;

        const double buttonHeight = 16.0;
        double effectiveLength = trackLength > buttonHeight * 2 ? trackLength - buttonHeight * 2 : trackLength;
        if (effectiveLength <= 0)
            return;

        double scrollDelta = (deltaY / effectiveLength) * VerticalScrollbar.Maximum;
        scrollManager.VerticalScroll = Math.Clamp(_initialScrollOffsetY + scrollDelta, 0, VerticalScrollbar.Maximum);
    }

    private void HorizontalScrollbar_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        zoomManager.ResetZoomAnchors();
        var point = e.GetCurrentPoint(HorizontalScrollbar);
        if (point.PointerDeviceType == PointerDeviceType.Touch)
        {
            if (sender is UIElement el)
            {
                el.CapturePointer(e.Pointer);
            }
            _isHorizontalScrollbarTouchDragging = true;
            _isTouchScrolling = true;
            _initialTouchScrollbarPosX = point.Position.X;
            _initialScrollOffsetX = scrollManager.HorizontalScroll;
            _touchScrollbarTotalMovementX = 0;
            e.Handled = true;
        }
    }

    private void HorizontalScrollbar_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_isHorizontalScrollbarTouchDragging)
        {
            var point = e.GetCurrentPoint(HorizontalScrollbar);
            double deltaX = point.Position.X - _initialTouchScrollbarPosX;
            _touchScrollbarTotalMovementX += Math.Abs(deltaX);
            UpdateHorizontalScrollbarFromDelta(deltaX);
            e.Handled = true;
        }
    }

    private void HorizontalScrollbar_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_isHorizontalScrollbarTouchDragging)
        {
            if (sender is UIElement el)
            {
                el.ReleasePointerCapture(e.Pointer);
            }
            _isHorizontalScrollbarTouchDragging = false;
            _isTouchScrolling = false;

            var point = e.GetCurrentPoint(HorizontalScrollbar);
            double trackLength = HorizontalScrollbar.ActualWidth;
            const double buttonWidth = 16.0;

            if (_touchScrollbarTotalMovementX < 8.0 && trackLength > 0)
            {
                if (point.Position.X < buttonWidth)
                {
                    scrollManager.HorizontalScroll = Math.Max(0, scrollManager.HorizontalScroll - 40);
                }
                else if (point.Position.X > trackLength - buttonWidth)
                {
                    scrollManager.HorizontalScroll = Math.Min(HorizontalScrollbar.Maximum, scrollManager.HorizontalScroll + 40);
                }
                else if (HorizontalScrollbar.Maximum > 0)
                {
                    double effectiveLength = trackLength > buttonWidth * 2 ? trackLength - buttonWidth * 2 : trackLength;
                    double effectiveX = Math.Clamp(point.Position.X - buttonWidth, 0.0, effectiveLength);
                    double fraction = effectiveLength > 0 ? effectiveX / effectiveLength : 0.0;
                    scrollManager.HorizontalScroll = fraction * HorizontalScrollbar.Maximum;
                }
            }

            SyncScrollTrackerToOffsetNow();
            e.Handled = true;
        }
    }

    private void HorizontalScrollbar_PointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        if (_isHorizontalScrollbarTouchDragging)
        {
            if (sender is UIElement el)
            {
                el.ReleasePointerCapture(e.Pointer);
            }
            _isHorizontalScrollbarTouchDragging = false;
            _isTouchScrolling = false;
            SyncScrollTrackerToOffsetNow();
        }
    }

    private void HorizontalScrollbar_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
        _isHorizontalScrollbarTouchDragging = false;
        _isTouchScrolling = false;
        SyncScrollTrackerToOffsetNow();
    }

    private void UpdateHorizontalScrollbarFromDelta(double deltaX)
    {
        double trackLength = HorizontalScrollbar.ActualWidth;
        if (trackLength <= 0 || HorizontalScrollbar.Maximum <= 0)
            return;

        const double buttonWidth = 16.0;
        double effectiveLength = trackLength > buttonWidth * 2 ? trackLength - buttonWidth * 2 : trackLength;
        if (effectiveLength <= 0)
            return;

        double scrollDelta = (deltaX / effectiveLength) * HorizontalScrollbar.Maximum;
        scrollManager.HorizontalScroll = Math.Clamp(_initialScrollOffsetX + scrollDelta, 0, HorizontalScrollbar.Maximum);
    }
    private void Canvas_LineNumber_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        Canvas_Selection.CapturePointer(e.Pointer);

        var point = e.GetCurrentPoint(Canvas_Selection);

        //Select the line where the cursor is over
        int line = CursorHelper.GetCursorLineFromPoint(textRenderer, point.Position);
        
        if (textManager.LinesCount > 0)
        {
            line = Math.Clamp(line, 0, textManager.LinesCount - 1);
        }
        else
            return;

        SelectLine(line);

        pointerActionsManager.StartLineSelection(line);
    }
    //Change the cursor when entering/leaving the control
    private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ChangeCursor(InputSystemCursorShape.IBeam);
    }
    private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        ChangeCursor(InputSystemCursorShape.Arrow);
    }

    //Canvas event
    // A Win2D draw callback runs inside the composition/render pipeline. If it throws, WinUI
    // re-raises the exception as a stowed exception (0xc000027b) that fail-fasts the whole
    // process and bypasses the managed UnhandledException handler, so it cannot be caught by
    // the app. Transient out-of-range layout/geometry can occur while rendering very large or
    // rapidly-changing content (e.g. the rebuilt text layout and a computed selection/slice
    // index momentarily disagree and CanvasTextLayout.GetCharacterRegions throws E_INVALIDARG).
    // Guarding each draw handler turns the worst case into a single skipped frame (the canvas
    // redraws on the next invalidation) instead of a crash.
    private void Canvas_Text_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        try
        {
            textRenderer.Draw(sender, args);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TextControlBox: Canvas_Text_Draw failed: {ex}");
        }
        initializationManager.CanvasDrawed(0);
    }
    private void Canvas_Selection_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        try
        {
            selectionRenderer.Draw(sender, args);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TextControlBox: Canvas_Selection_Draw failed: {ex}");
        }
        initializationManager.CanvasDrawed(1);
    }
    private void Canvas_Cursor_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        try
        {
            cursorRenderer.Draw(Canvas_Text, Canvas_Cursor, args);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TextControlBox: Canvas_Cursor_Draw failed: {ex}");
        }
        initializationManager.CanvasDrawed(2);
    }
    private void Canvas_LineNumber_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        try
        {
            if (!lineNumberManager._ShowLineNumbers)
            {
                lineNumberRenderer.HideLineNumbers(sender, lineNumberManager._SpaceBetweenLineNumberAndText);
                return;
            }

            lineNumberRenderer.Draw(Canvas_LineNumber, args, lineNumberManager._SpaceBetweenLineNumberAndText);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TextControlBox: Canvas_LineNumber_Draw failed: {ex}");
        }
    }

    //Focus:
    private void UserControl_LosingFocus(UIElement sender, LosingFocusEventArgs args)
    {
        //Prevent the focus switching to the RootScrollViewer when double clicking.
        //It was the only way, I could think of.
        //https://stackoverflow.com/questions/74802534/double-tap-on-uwp-usercontrol-removes-focus
        if (args.NewFocusedElement is ScrollViewer sv && sv.Content is Border)
        {
            args.TryCancel();
        }
    }
    private void UserControl_Tapped(object sender, TappedRoutedEventArgs e)
    {
        this.Focus(FocusState.Programmatic);
    }
    private void InputManager_GotFocus(object sender, RoutedEventArgs e)
    {
        focusManager.SetFocus();
        UpdateInputHandlerPosition();
    }
    private void InputManager_LostFocus(object sender, RoutedEventArgs e)
    {
        undoRedo.EndBatch();
        focusManager.RemoveFocus();
        inputHandler.ResetComposition();
    }

    public new void Focus(FocusState state)
    {
        UpdateInputHandlerPosition();
        inputHandler.Focus(state);
    }

    internal void UpdateInputHandlerPosition()
    {
        if (inputHandler == null || textRenderer == null)
            return;

        var pos = GetCursorPosition();
        var newMargin = new Thickness(pos.X, pos.Y, 0, 0);
        if (inputHandler.Margin != newMargin)
        {
            inputHandler.Margin = newMargin;
        }
        float lineHeight = textRenderer.SingleLineHeight;
        if (lineHeight > 0 && Math.Abs(inputHandler.Height - lineHeight) > 0.1)
        {
            inputHandler.Height = lineHeight;
        }
    }

    private void InputHandler_CompositionStarted(object sender, TextCompositionStartedEventArgs e)
    {
        UpdateInputHandlerPosition();
    }

    private void InputHandler_CompositionChanged(object sender, TextCompositionChangedEventArgs e)
    {
        UpdateInputHandlerPosition();
    }

    internal TextControlBoxNS.Controls.InputHandlerControl InputHandler => inputHandler;

    //Cursor:
    private void Canvas_LineNumber_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ChangeCursor(InputSystemCursorShape.Arrow);
    }
    private void Canvas_LineNumber_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        ChangeCursor(InputSystemCursorShape.IBeam);
    }

    //Drag Drop text
    private async void UserControl_Drop(object sender, DragEventArgs e)
    {
        if (textManager._IsReadOnly)
            return;

        if (e.DataView.Contains(StandardDataFormats.Text))
        {
            textActionManager.AddCharacter(stringManager.CleanUpString(await e.DataView.GetTextAsync()), true);
        }
    }
    private void UserControl_DragOver(object sender, DragEventArgs e)
    {
        if (selectionManager.IsSelecting || textManager._IsReadOnly || !e.DataView.Contains(StandardDataFormats.Text))
            return;

        var deferral = e.GetDeferral();

        e.AcceptedOperation = DataPackageOperation.Copy;
        e.DragUIOverride.IsGlyphVisible = false;
        e.DragUIOverride.IsContentVisible = false;
        deferral.Complete();

        CursorHelper.UpdateCursorPosFromPoint(Canvas_Text, currentLineManager, textRenderer, scrollManager, e.GetPosition(Canvas_Text), cursorManager.currentCursorPosition);
        canvasUpdateManager.UpdateCursor();
    }

    public void ChangeCursor(InputSystemCursorShape cursor)
    {
        this.ProtectedCursor = InputSystemCursor.Create(cursor);
    }

    public bool SelectLine(int line)
    {
        if (line >= textManager.LinesCount || line < 0)
            return false;

        int lineLength = textManager.GetLineLength(line);
        selectionManager.SetSelection(line, 0, line, lineLength + 1);
        cursorManager.SetCursorPosition(line, 0);

        canvasUpdateManager.UpdateSelection();
        canvasUpdateManager.UpdateCursor();
        return true;
    }

    public bool SelectLines(int start, int count)
    {
        if (count <= 0)
            return false;

        var endLine = start + count - 1;
        if (start < 0 || endLine < 0 || endLine >= textManager.LinesCount)
            return false;

        int endLineLength = textManager.GetLineLength(endLine);

        selectionManager.SetSelection(start, 0, endLine, endLineLength);
        cursorManager.SetCursorPosition(endLine, endLineLength);

        canvasUpdateManager.UpdateSelection();
        canvasUpdateManager.UpdateCursor();
        return true;
    }

    public bool GoToLine(int line)
    {
        if (line >= textManager.LinesCount || line < 0)
            return false;

        selectionManager.selectionEnd.IsNull = true;
        cursorManager.SetCursorPosition(line, 0);
        selectionManager.SetSelectionStart(line, 0);

        ScrollLineIntoView(line);
        this.Focus(FocusState.Programmatic);

        canvasUpdateManager.UpdateAll();
        return true;
    }

    public void LoadText(string text, bool autodetectTabsSpaces = true)
    {
        textActionManager.Safe_LoadText(text, autodetectTabsSpaces);
    }

    public void SetText(string text)
    {
        textActionManager.Safe_SetText(text);
    }

    public void LoadLines(IEnumerable<string> lines, bool autodetectTabsSpaces = true, LineEnding lineEnding = LineEnding.CRLF)
    {
        textActionManager.Safe_LoadLines(lines, autodetectTabsSpaces, lineEnding);
    }

    public void Paste()
    {
        textActionManager.Safe_Paste();
    }

    public void Copy()
    {
        textActionManager.Safe_Copy();
    }

    public void Cut()
    {
        textActionManager.Safe_Cut();
    }

    public string GetText()
    {
        return textActionManager.Safe_Gettext();
    }

    public void SetSelection(int start, int length)
    {
        var result = selectionManager.GetSelectionFromPosition(start, length, CharacterCount());
        if (result != null)
        {
            selectionManager.SetSelection(result.StartPosition, result.EndPosition);
            if (!result.EndPosition.IsNull)
                CursorPosition.SetChangeValues(result.EndPosition);
        }

        canvasUpdateManager.UpdateSelection();
        canvasUpdateManager.UpdateCursor();
    }

    public void SetSelection(int startLine, int startChar, int endLine, int endChar)
    {
        selectionManager.SetSelection(startLine, startChar, endLine, endChar);
        CursorPosition.SetChangeValues(endLine, endChar);

        canvasUpdateManager.UpdateSelection();
        canvasUpdateManager.UpdateCursor();
    }

    public void SelectAll()
    {
        textActionManager.SelectAll();
    }

    public void Delete()
    {
        if (IsReadOnly)
            return;

        if (selectionManager.HasSelection)
        {
            textActionManager.DeleteSelection();
        }
        else
        {
            textActionManager.DeleteText();
        }
        canvasUpdateManager.UpdateAll();
    }

    public void ClearSelection()
    {
        selectionManager.ClearSelection();
        canvasUpdateManager.UpdateAll();
    }

    public void Undo()
    {
        textActionManager.Undo();
    }

    public void Redo()
    {
        textActionManager.Redo();
    }

    public void ScrollIntoViewHorizontally()
    {
        scrollManager.ScrollIntoViewHorizontal(canvasText);
    }

    public void ScrollLineToCenter(int line)
    {
        scrollManager.ScrollLineIntoViewIfOutside(line);
    }

    public void ScrollOneLineUp(bool update = true)
    {
        scrollManager.ScrollOneLineUp(update);

    }

    public void ScrollOneLineDown(bool update = true)
    {
        scrollManager.ScrollOneLineDown(update);
    }

    public void ScrollLineIntoView(int line)
    {
        scrollManager.ScrollLineIntoView(line);
    }

    public void ScrollTopIntoView()
    {
        scrollManager.ScrollTopIntoView();
    }

    public void ScrollBottomIntoView()
    {
        scrollManager.ScrollBottomIntoView();
    }

    /// <summary>Horizontally centers the current cursor column in the viewport (no-op in word-wrap mode).
    /// Useful for revealing a search match in the middle of the visible width instead of pinned to an edge.</summary>
    public void ScrollIntoViewHorizontallyCentered()
    {
        scrollManager.ScrollCursorIntoViewHorizontallyCentered(canvasText);
    }

    public void ScrollPageUp()
    {
        scrollManager.ScrollPageUp();
    }

    public void ScrollPageDown()
    {
        scrollManager.ScrollPageDown();
    }

    public string GetLineText(int line)
    {
        return textManager.GetLineText(line);
    }

    public string GetLinesText(int startLine, int length)
    {
        return textManager.GetLinesAsString(startLine, length);
    }

    public bool SetLineText(int line, string text)
    {
        return textActionManager.SetLineText(line, text);
    }

    public bool DeleteLine(int line)
    {
        return textActionManager.DeleteLine(line);
    }

    public bool AddLine(int line, string text)
    {
        return textActionManager.AddLine(line, text);
    }
    public bool AddLines(int start, string[] text)
    {
        return textActionManager.AddLines(start, text);
    }

    public bool SurroundSelectionWith(string text)
    {
        return SurroundSelectionWith(text, text);
    }

    public bool SurroundSelectionWith(string text1, string text2)
    {
        if (selectionManager.HasSelection)
        {
            if(stringManager.HasMultilineCharacters(text1) || stringManager.HasMultilineCharacters(text2))
                throw new ArgumentException(
                    "The text contains multiline characters, which are not allowed.");

            textActionManager.AddCharacter(stringManager.CleanUpString(text1) + SelectedText + stringManager.CleanUpString(text2));
            return true;
        }
        return false;
    }

    public bool DuplicateLine(int line, bool ignoreIsReadOnly = false)
    {
        if (!ignoreIsReadOnly && IsReadOnly)
            return false;

        if (line >= textManager.LinesCount || line < 0)
            return false;

        textActionManager.DuplicateLine(line);
        return true;
    }
    public void DuplicateCurrentLine(bool ignoreIsReadOnly = false)
    {
        if (!ignoreIsReadOnly && IsReadOnly)
            return;

        textActionManager.DuplicateLine(CursorPosition.LineNumber);
    }

    public SearchResult ReplaceAll(string word, string replaceWord, bool matchCase, bool wholeWord, bool ignoreIsReadOnly = false)
    {
        if (!ignoreIsReadOnly && IsReadOnly)
            return SearchResult.ReplaceNotAllowedInReadonly;

        return replaceManager.ReplaceAll(word, replaceWord, matchCase, wholeWord);
    }

    public SearchResult ReplaceNext(string replaceWord, bool ignoreIsReadOnly = false)
    {
        if (!ignoreIsReadOnly && IsReadOnly)
            return SearchResult.ReplaceNotAllowedInReadonly;

        var res = replaceManager.ReplaceNext(replaceWord);
        if (res.Selection != null)
        {
            ScrollLineIntoView(CursorPosition.LineNumber);
        }
        return res.Result;
    }

    public SearchResult FindNext()
    {
        if (!searchManager.IsSearchOpen)
            return SearchResult.SearchNotOpened;

        var res = searchManager.FindNext(CursorPosition);
        if (res.Selection != null)
        {
            selectionManager.SetSelection(res.Selection);
            ScrollLineIntoView(CursorPosition.LineNumber);
        }
        return res.Result;
    }

    public SearchResult FindPrevious()
    {
        if (!searchManager.IsSearchOpen)
            return SearchResult.SearchNotOpened;

        var res = searchManager.FindPrevious(CursorPosition);
        if (res.Selection != null)
        {
            selectionManager.SetSelection(res.Selection);
            ScrollLineIntoView(CursorPosition.LineNumber);
        }
        return res.Result;
    }

    public SearchResult BeginSearch(string word, bool wholeWord, bool matchCase)
    {
        var res = searchManager.BeginSearch(word, wholeWord, matchCase);
        canvasUpdateManager.UpdateText();
        return res;
    }

    public void EndSearch()
    {
        searchManager.EndSearch();
        canvasUpdateManager.UpdateText();
    }

    public void Dispose()
    {
        Unload();
    }

    private bool _isUnloaded = false;

    public void Unload()
    {
        if (_isUnloaded)
            return;
        _isUnloaded = true;

        Loaded -= CoreTextControlBox_Loaded;
        Unloaded -= CoreTextControlBox_Unloaded;

        //Unsubscribe from events:
        if (inputHandler != null)
        {
            inputHandler.PreviewKeyDown -= InputHandler_KeyDown;
            inputHandler.TextEntered -= InputHandler_TextEntered;
            inputHandler.CompositionStarted -= InputHandler_CompositionStarted;
            inputHandler.CompositionChanged -= InputHandler_CompositionChanged;
        }

        if (verticalScrollBar != null)
        {
            verticalScrollBar.Loaded -= scrollManager.VerticalScrollbar_Loaded;
            verticalScrollBar.Scroll -= scrollManager.VerticalScrollBar_Scroll;
        }

        if (horizontalScrollBar != null)
        {
            horizontalScrollBar.Scroll -= scrollManager.HorizontalScrollBar_Scroll;
        }

        caretBlinkManager?.Stop();
        pointerActionsManager?.CleanUp();
        TeardownDiagonalScroll();

        textRenderer.CheckDispose();
        lineNumberRenderer.CheckDispose();

        // Release Win2D canvas resources cleanly
        try { Canvas_LineNumber?.RemoveFromVisualTree(); } catch { }
        try { Canvas_Cursor?.RemoveFromVisualTree(); } catch { }
        try { Canvas_Text?.RemoveFromVisualTree(); } catch { }
        try { Canvas_Selection?.RemoveFromVisualTree(); } catch { }

        //Dispose and null larger objects
        textManager.totalLines.Dispose();
        lineNumberRenderer.LineNumberTextToRender = lineNumberRenderer.OldLineNumberTextToRender = null;
        undoRedo.NullAll();
    }

    public void ClearUndoRedoHistory()
    {
        undoRedo.ClearAll();
    }

    public Point GetCursorPosition()
    {
        if (textRenderer.CurrentLineTextLayout == null && canvasText != null)
            textRenderer.UpdateCurrentLineTextLayout(canvasText);

        float withinLineRowOffset = 0;
        float cursorX = 0;
        bool cursorXComputed = false;
        if (textRenderer.IsWordWrapEnabled && textRenderer.CurrentLineTextLayout != null)
        {
            int renderedPos = textRenderer.GetRenderedCharacterIndexForDocumentCharacter(CursorPosition.LineNumber, CursorPosition.CharacterPosition);
            if (renderedPos >= 0)
            {
                float baseRowY = textRenderer.CurrentLineTextLayout.GetCaretPosition(0, false).Y;
                var vector = textRenderer.CurrentLineTextLayout.GetCaretPosition(renderedPos, CursorPosition.IsTrailing);
                int visualRow = (int)Math.Round((vector.Y - baseRowY) / Math.Max(1, textRenderer.SingleLineHeight));
                withinLineRowOffset = visualRow * textRenderer.SingleLineHeight;
                cursorX = vector.X;
                cursorXComputed = true;
            }
        }
        return new Point
        {
            Y = textRenderer.GetCurrentLineLayoutTopY(CursorPosition.LineNumber) + withinLineRowOffset + textRenderer.TopInset,
            X = cursorXComputed ? cursorX : CursorHelper.GetCursorPositionInLine(textRenderer.CurrentLineTextLayout, CursorPosition, textRenderer.IsWordWrapEnabled ? 0 : textRenderer.HorizontalOffset)
        };
    }

    public int GetLineFromPoint(Point point)
    {
        return CursorHelper.GetCursorLineFromPoint(textRenderer, point);
    }

    public float SingleLineHeight => textRenderer.SingleLineHeight;

    public void SetCursorPosition(int lineNumber, int characterPos, bool scrollIntoView = true, bool autoClamp = true)
    {
        if (autoClamp)
        {
            lineNumber = Math.Clamp(lineNumber, 0, textManager.totalLines.Count - 1);
            int length = textManager.GetLineLength(lineNumber);
            characterPos = Math.Clamp(characterPos, 0, length);
        }
        else
        {
            if (lineNumber < 0 || lineNumber >= textManager.totalLines.Count)
                throw new IndexOutOfRangeException("Invalid line number provided for SetCursorPosition");

            var length = textManager.GetLineLength(lineNumber);
            if (characterPos < 0 || characterPos > length)
                throw new IndexOutOfRangeException("Invalid character position provided for SetCursorPosition");
        }

        cursorManager.currentCursorPosition.LineNumber = lineNumber;
        cursorManager.currentCursorPosition.CharacterPosition = characterPos;

        if (scrollIntoView)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                scrollManager.ScrollLineIntoView(lineNumber);
                scrollManager.ScrollIntoViewHorizontal(canvasText);
            });
        }
        else //updates in the if condition happen on scrolling, only update cursor when not scrolling:
            canvasUpdateManager.UpdateCursor();
    }

    public void SelectSyntaxHighlightingById(SyntaxHighlightID languageId)
    {
        if (SyntaxHighlightings.TryGetValue(languageId, out SyntaxHighlightLanguage syntaxLanguage))
            SyntaxHighlighting = syntaxLanguage;
    }

    public TextSelectionPosition CalculateSelectionPosition()
    {
        var pos = selectionManager.CalculateSelectionStartLength();
        return new TextSelectionPosition(pos.start, pos.length);
    }
    public int CharacterCount()
    {
        return textManager.CountCharacters();
    }
    public int WordCount()
    {
        return textManager.CountWords();
    }

    public void ExecuteActionGroup(Action actionGroup)
    {
        undoRedo.ExecuteActionGroup(actionGroup);
    }

    public void BeginActionGroup()
    {
        undoRedo.BeginActionGroup();
    }

    public void EndActionGroup()
    {
        undoRedo.EndActionGroup();
    }
    public bool IsGroupingActions => undoRedo.IsGroupingActions;

    public bool EnableSyntaxHighlighting { get; set; } = true;

    public SyntaxHighlightLanguage SyntaxHighlighting
    {
        get => textManager._SyntaxHighlighting;
        set
        {
            textManager._SyntaxHighlighting = value;

            if (textManager._SyntaxHighlighting != null)
                textManager._SyntaxHighlighting.CompileAllRegex();

            textRenderer.NeedsUpdateTextLayout = true;
            canvasUpdateManager.UpdateText();
        }
    }

    public LineEnding LineEnding
    {
        get => textManager.LineEnding;
        set
        {
            if (textManager.LineEnding == value)
                return;

            textManager.LineEnding = value;
            textRenderer.InvalidateWrapMetrics();
            textRenderer.NeedsUpdateTextLayout = true;
            textRenderer.OldRenderedText = null;
            textRenderer.InvalidateRenderedText();
            canvasUpdateManager.UpdateAll();
        }
    }

    public float SpaceBetweenLineNumberAndText { get => lineNumberManager._SpaceBetweenLineNumberAndText; set { lineNumberManager._SpaceBetweenLineNumberAndText = value; lineNumberRenderer.NeedsUpdateLineNumbers(); canvasUpdateManager.UpdateAll(); } }

    public CursorPosition CursorPosition
    {
        get => cursorManager.currentCursorPosition;
        set { cursorManager.LineNumber = value.LineNumber; cursorManager.CharacterPosition = value.CharacterPosition; canvasUpdateManager.UpdateCursor(); }
    }

    public new FontFamily FontFamily { get => textManager._FontFamily; set { textManager._FontFamily = value; textRenderer.NeedsTextFormatUpdate = true; canvasUpdateManager.UpdateAll(); } }

    /// <summary>When true, long lines wrap at the control width instead of scrolling horizontally. Toggling
    /// rebuilds the text format (with the new wrapping mode) and the wrap metrics, and resets horizontal
    /// scroll (there is no horizontal scrolling in wrap mode).</summary>
    public bool WordWrap
    {
        get => textLayoutManager.WordWrap;
        set
        {
            if (textLayoutManager.WordWrap == value)
                return;

            textRenderer.NeedsTextFormatUpdate = true;
            if (value)
            {
                textLayoutManager.WordWrap = true;
                textRenderer.EnsureWrapMetrics(canvasText);
                int targetVisualRow = textRenderer.GetLineVisualStartRow(textRenderer.NumberOfStartLine);
                scrollManager.VerticalScroll = (targetVisualRow * textRenderer.SingleLineHeight) / scrollManager.DefaultVerticalScrollSensitivity;
                scrollManager.HorizontalScroll = 0;
            }
            else
            {
                int currentDocLine = textRenderer.GetDocumentLineFromVisualRow(textRenderer.StartVisualRow);
                textLayoutManager.WordWrap = false;
                scrollManager.VerticalScroll = (currentDocLine * textRenderer.SingleLineHeight) / scrollManager.DefaultVerticalScrollSensitivity;
                scrollManager.HorizontalScroll = 0;
                longestLineManager.needsRecalculation = true;
                longestLineManager.CheckRecalculateLongestLine(true);
            }

            textRenderer.NeedsUpdateTextLayout = true;
            textRenderer.OldRenderedText = null;
            textRenderer.InvalidateWrapMetrics();
            lineNumberRenderer.NeedsUpdateLineNumbers();
            canvasUpdateManager.UpdateLineNumbers();
            canvasUpdateManager.UpdateAll();
        }
    }

    public new int FontSize { get => textManager._FontSize; set { textManager._FontSize = value; zoomManager.UpdateZoom(); } }

    public float RenderedFontSize => zoomManager.ZoomedFontSize;

    public string Text { get => GetText(); set { SetText(value); } }

    public new ElementTheme RequestedTheme
    {
        get => designHelper.RequestedTheme;
        set
        {
            base.RequestedTheme = value;
            designHelper.RequestedTheme = value;
        }
    }

    public TextControlBoxDesign Design
    {
        get => designHelper.Design;
        set => designHelper.Design = value;
    }

    public Windows.UI.Color TextColor
    {
        get => designHelper._Design.TextColor;
        set
        {
            if (designHelper._Design.TextColor.Equals(value))
                return;

            designHelper._Design.TextColor = value;
            designHelper.ColorResourcesCreated = false;
            textRenderer.NeedsUpdateTextLayout = true;
            canvasUpdateManager.UpdateAll();
        }
    }

    public bool ShowLineNumbers
    {
        get => lineNumberManager._ShowLineNumbers;
        set
        {
            lineNumberManager._ShowLineNumbers = value;
            textRenderer.NeedsUpdateTextLayout = true;
            lineNumberRenderer.NeedsUpdateLineNumbers();
            canvasUpdateManager.UpdateAll();
        }
    }

    public bool ShowLineHighlighter
    {
        get => lineHighlighterManager._ShowLineHighlighter;
        set { lineHighlighterManager._ShowLineHighlighter = value; canvasUpdateManager.UpdateCursor(); }
    }

    public int ZoomFactor { get => zoomManager._ZoomFactor; set { zoomManager._ZoomFactor = value; zoomManager.UpdateZoom(); } } //%

    public bool IsReadOnly { get => textManager._IsReadOnly; set { textManager._IsReadOnly = inputHandler.IsReadOnly = value; } }

    public CursorSize CursorSize { get => cursorRenderer._CursorSize; set { cursorRenderer._CursorSize = value; canvasUpdateManager.UpdateCursor(); } }

    public new MenuFlyout ContextFlyout
    {
        get { return flyoutHelper.menuFlyout; }
        set
        {
            if (value == null) //Use the builtin flyout
            {
                flyoutHelper.CreateFlyout(this);
            }
            else //Use a custom flyout
            {
                flyoutHelper.menuFlyout = value;
            }
        }
    }
    public bool ContextFlyoutDisabled { get; set; }
    public string SelectedText
    {
        get
        {
            if (selectionManager.WholeTextSelected())
                return GetText();
            return selectionManager.GetSelectedText(CursorPosition.LineNumber);
        }
        //we ignore isReadOnly here, to allow setting text in readonly mode via code.
        set => textActionManager.AddCharacter(stringManager.CleanUpString(value), ignoreIsReadOnly: true);
    }
    public void RewriteTabsSpaces(int spaces, bool useSpacesInsteadTabs, bool ignoreIsReadonly = false)
    {
        if (spaces <= 0)
            throw new ArgumentOutOfRangeException("Spaces must be greater than zero.");

        if (!ignoreIsReadonly && IsReadOnly)
            return;

        tabSpaceManager.RewriteTabsSpaces(useSpacesInsteadTabs ? spaces : -1);

        canvasUpdateManager.UpdateAll();
    }

    public (bool useSpacesInsteadTabs, int spaces) DetectTabsSpaces()
    {
        return TabsSpacesHelper.DetectTabsSpaces(textManager.totalLines);
    }

    public int NumberOfLines { get => textManager.LinesCount; }

    public int CurrentLineIndex { get => CursorPosition.LineNumber; }
    public ScrollBarPosition ScrollBarPosition
    {
        get => new ScrollBarPosition(HorizontalScrollbar.Value, VerticalScroll);
        set { HorizontalScrollbar.Value = value.ValueX; VerticalScroll = value.ValueY; }
    }
    public double VerticalScrollSensitivity { get => scrollManager._VerticalScrollSensitivity; set => scrollManager._VerticalScrollSensitivity = value < 1 ? 1 : value; }
    public double HorizontalScrollSensitivity { get => scrollManager._HorizontalScrollSensitivity; set => scrollManager._HorizontalScrollSensitivity = value < 1 ? 1 : value; }
    public double VerticalScroll { get => VerticalScrollbar.Value; set { VerticalScrollbar.Value = value < 0 ? 0 : value; canvasUpdateManager.UpdateAll(); } }
    public double HorizontalScroll { get => HorizontalScrollbar.Value; set { HorizontalScrollbar.Value = value < 0 ? 0 : value; canvasUpdateManager.UpdateAll(); } }
    public new CornerRadius CornerRadius { get => MainGrid.CornerRadius; set => MainGrid.CornerRadius = value; }
    public bool UseSpacesInsteadTabs { get => tabSpaceManager.UseSpacesInsteadTabs; set { tabSpaceManager.UseSpacesInsteadTabs = value; } }
    public int NumberOfSpacesForTab { get => tabSpaceManager.NumberOfSpaces; set { tabSpaceManager.NumberOfSpaces = value; } }
    public bool SearchIsOpen => searchManager.IsSearchOpen;
    public IEnumerable<string> Lines => textManager.totalLines;
    public bool DoAutoPairing { get; set; } = true;
    public bool AutoPairOnlyOnSelection { get; set; } = true;

    public bool ControlW_SelectWord = true;
    public bool HasSelection => selectionManager.HasSelection;
    public TextControlBoxSelection? CurrentSelection => selectionManager.HasSelection ? new TextControlBoxSelection(this.selectionManager.currentTextSelection) : null;
    public TextControlBoxSelection? CurrentSelectionOrdered => selectionManager.HasSelection ? new TextControlBoxSelection(selectionManager) : null;
    public new bool IsLoaded => initializationManager.initDone;
    public bool ShowWhitespaceCharacters { get => whitespaceCharactersManager.ShowWhitespaceCharacters; set { whitespaceCharactersManager.ShowWhitespaceCharacters = value; canvasUpdateManager.UpdateText(); } }
    public Thickness SelectionScrollStartBorderDistance { get; set; } = new Thickness(35, 35, 35, 35);
    public bool HighlightLinks { get => linkHighlightManager.HighlightLinks; set { linkHighlightManager.HighlightLinks = value; canvasUpdateManager.UpdateAll(); } }
    public bool HighlightLineWhenNotFocused { get => lineHighlighterManager._HighlightLineWhenNotFocused; set { lineHighlighterManager._HighlightLineWhenNotFocused = value; canvasUpdateManager.UpdateText(); } }
    public bool CanUndo => undoRedo.CanUndo;
    public bool CanRedo => undoRedo.CanRedo;

    public float ActualLineHeight => textRenderer.SingleLineHeight;

    public static readonly Dictionary<SyntaxHighlightID, SyntaxHighlightLanguage> SyntaxHighlightings =
        new Dictionary<SyntaxHighlightID, SyntaxHighlightLanguage>()
        {
            { SyntaxHighlightID.None, null },
            { SyntaxHighlightID.x86Assembly, new x86Assembly() },
            { SyntaxHighlightID.Batch, new Batch() },
            { SyntaxHighlightID.Cpp, new Cpp() },
            { SyntaxHighlightID.CSharp, new CSharp() },
            { SyntaxHighlightID.Klipper, new KlipperHighlighter() },
            { SyntaxHighlightID.TOML, new TomlHighlighter() },
            { SyntaxHighlightID.Inifile, new IniHighlighter() },
            { SyntaxHighlightID.CSS, new CSS() },
            { SyntaxHighlightID.CSVImproved, new CSVEnhanced() },
            { SyntaxHighlightID.Log, new LogHighlighter() },
            { SyntaxHighlightID.CSV, new CSV() },
            { SyntaxHighlightID.GCode, new GCode() },
            { SyntaxHighlightID.Gitignore, new GitIgnore() },
            { SyntaxHighlightID.HexFile, new HexFile() },
            { SyntaxHighlightID.Html, new Html() },
            { SyntaxHighlightID.Java, new Java() },
            { SyntaxHighlightID.Javascript, new Javascript() },
            { SyntaxHighlightID.Json, new Json() },
            { SyntaxHighlightID.Latex, new LaTex() },
            { SyntaxHighlightID.Lua, new Lua() },
            { SyntaxHighlightID.Markdown, new Markdown() },
            { SyntaxHighlightID.PHP, new PHP() },
            { SyntaxHighlightID.Python, new Python() },
            { SyntaxHighlightID.QSharp, new QSharp() },
            { SyntaxHighlightID.XML, new XML() },
            { SyntaxHighlightID.SQL, new SQL() },
        };

    public static SyntaxHighlightLanguage GetSyntaxHighlightingFromID(SyntaxHighlightID languageId)
    {
        if (SyntaxHighlightings.TryGetValue(languageId, out SyntaxHighlightLanguage syntaxLanguage))
            return syntaxLanguage;
        return null;
    }

    public static JsonLoadResult GetSyntaxHighlightingFromJson(string Json)
    {
        return SyntaxHighlightingRenderer.GetSyntaxHighlightingFromJson(Json);
    }

}