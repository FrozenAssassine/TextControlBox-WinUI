using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TextControlBoxNS.Extensions;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Languages;
using TextControlBoxNS.Core.Renderer;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Models.Enums;
using TextControlBoxNS.Models;
using System.Linq;

namespace TextControlBoxNS.Core;

internal sealed partial class CoreTextControlBox : UserControl
{
    private readonly SelectionRenderer selectionRenderer;
    private readonly FlyoutHelper flyoutHelper;
    private readonly TabSpaceHelper tabSpaceHelper;
    private readonly StringManager stringManager;
    private readonly SearchManager searchManager;
    private readonly CanvasUpdateManager canvasUpdateManager;
    private readonly LineNumberRenderer lineNumberRenderer;
    private readonly TextManager textManager = new TextManager();
    private readonly UndoRedo undoRedo;
    private readonly SelectionManager selectionManager;
    private readonly CursorManager cursorManager;
    private readonly TextActionManager textActionManager;
    private readonly TextRenderer textRenderer;
    private readonly CursorRenderer cursorRenderer;
    private readonly ScrollManager scrollManager;
    private readonly CurrentLineManager currentLineManager;
    private readonly LongestLineManager longestLineManager;
    private readonly ZoomManager zoomManager;
    private readonly DesignHelper designHelper;
    private readonly LineHighlighterManager lineHighlighterManager;
    private readonly LineNumberManager lineNumberManager;
    public readonly EventsManager eventsManager;
    private readonly SelectionDragDropManager selectionDragDropManager;
    private readonly FocusManager focusManager;
    private readonly PointerActionsManager pointerActionsManager;
    private readonly TextLayoutManager textLayoutManager;
    private readonly LineHighlighterRenderer lineHighlighterRenderer;
    private readonly LoadingManager loadingManager;

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
        scrollManager = new ScrollManager();
        currentLineManager = new CurrentLineManager();
        longestLineManager = new LongestLineManager();
        designHelper = new DesignHelper();
        tabSpaceHelper = new TabSpaceHelper();
        lineHighlighterManager = new LineHighlighterManager();
        lineNumberManager = new LineNumberManager();
        searchManager = new SearchManager();
        eventsManager = new EventsManager();
        lineNumberRenderer = new LineNumberRenderer();
        zoomManager = new ZoomManager();
        selectionDragDropManager = new SelectionDragDropManager();
        focusManager = new FocusManager();
        pointerActionsManager = new PointerActionsManager();
        textLayoutManager = new TextLayoutManager();
        loadingManager = new LoadingManager();
        lineHighlighterRenderer = new LineHighlighterRenderer();

        stringManager.Init(textManager, tabSpaceHelper);
        lineHighlighterRenderer.Init(lineHighlighterManager, selectionManager, textRenderer);
        cursorManager.Init(textManager, currentLineManager);
        selectionManager.Init(textManager, cursorManager, selectionRenderer);
        undoRedo.Init(textManager, selectionManager);
        selectionRenderer.Init(selectionManager, textRenderer, eventsManager, scrollManager, zoomManager, designHelper);
        flyoutHelper.Init(this);
        canvasUpdateManager.Init(this);
        textActionManager.Init(this, textRenderer, undoRedo, currentLineManager, longestLineManager, canvasUpdateManager, textManager, selectionRenderer, cursorManager, scrollManager, eventsManager, stringManager, selectionManager);
        textRenderer.Init(cursorManager, designHelper, textLayoutManager, textManager, scrollManager, lineNumberRenderer, longestLineManager, this, searchManager, canvasUpdateManager);
        cursorRenderer.Init(cursorManager, currentLineManager, textRenderer, focusManager, textManager, scrollManager, zoomManager, designHelper, lineHighlighterRenderer, eventsManager);
        scrollManager.Init(this, canvasUpdateManager, textManager, textRenderer, cursorManager, VerticalScrollbar, HorizontalScrollbar);
        currentLineManager.Init(cursorManager, textManager);
        longestLineManager.Init(selectionManager, textManager);
        designHelper.Init(this, textRenderer, canvasUpdateManager);
        tabSpaceHelper.Init(textManager, selectionManager);
        searchManager.Init(textManager);
        eventsManager.Init(searchManager, selectionManager, cursorManager, selectionRenderer);
        lineNumberRenderer.Init(textManager, textLayoutManager, textRenderer, designHelper, lineNumberManager);
        zoomManager.Init(textManager, textRenderer, scrollManager, canvasUpdateManager, lineNumberRenderer, cursorManager, eventsManager);
        selectionDragDropManager.Init(this, cursorManager, selectionManager, textManager, textActionManager, canvasUpdateManager, selectionRenderer, textRenderer);
        focusManager.Init(this, canvasUpdateManager, inputHandler, eventsManager);
        pointerActionsManager.Init(this, textRenderer, textManager, cursorManager, canvasUpdateManager, scrollManager, selectionRenderer, selectionDragDropManager, currentLineManager);
        textLayoutManager.Init(textManager, zoomManager);

        //subscribe to events:

        //set default values
        RequestedTheme = ElementTheme.Default;
        LineEnding = LineEnding.CRLF;

        InitialiseOnStart();
        focusManager.SetFocus();

        loadingManager.IsTextboxLoaded = true;
    }

    public void ChangeCursor(InputSystemCursorShape cursor)
    {
        this.ProtectedCursor = InputSystemCursor.Create(cursor);
    }
    private void InitialiseOnStart()
    {
        zoomManager.UpdateZoom();
        if (textManager.LinesCount == 0)
        {
            textManager.AddLine();
        }
    }

    //Handle keyinputs
    private void InputHandler_TextEntered(object sender, TextChangedEventArgs e)
    {
        if (IsReadonly || inputHandler.Text == "\t")
            return;

        //Prevent key-entering if control key is pressed 
        var ctrl = Utils.IsKeyPressed(VirtualKey.Control);
        var menu = Utils.IsKeyPressed(VirtualKey.Menu);
        if (ctrl && !menu || menu && !ctrl)
            return;

        textActionManager.AddCharacter(inputHandler.Text);
        inputHandler.Text = "";
    }
    private void InputHandler_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Tab)
        {
            TextSelection selection;
            if (Utils.IsKeyPressed(VirtualKey.Shift))
                selection = tabSpaceHelper.MoveTabBack(selectionManager.currentTextSelection, CursorPosition, tabSpaceHelper.TabCharacter, undoRedo);
            else
                selection = tabSpaceHelper.MoveTab(selectionManager.currentTextSelection, CursorPosition, tabSpaceHelper.TabCharacter, undoRedo);

            if (selection != null)
            {
                if (selection.EndPosition == null)
                {
                    CursorPosition = selection.StartPosition;
                }
                else
                {
                    selectionRenderer.SetSelection(selection);
                    CursorPosition = selection.EndPosition;
                }
            }
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
                    break;
                case VirtualKey.Down:
                    ScrollOneLineDown();
                    break;
                case VirtualKey.V:
                    Paste();
                    break;
                case VirtualKey.Z:
                    Undo();
                    break;
                case VirtualKey.Y:
                    Redo();
                    break;
                case VirtualKey.C:
                    Copy();
                    break;
                case VirtualKey.X:
                    Cut();
                    break;
                case VirtualKey.A:
                    SelectAll();
                    break;
                case VirtualKey.W:
                    selectionManager.SelectSingleWord(canvasUpdateManager, CursorPosition);
                    break;
            }

            if (e.Key != VirtualKey.Left && e.Key != VirtualKey.Right && e.Key != VirtualKey.Back && e.Key != VirtualKey.Delete)
                return;
        }

        if (menu)
        {
            if (e.Key == VirtualKey.Down || e.Key == VirtualKey.Up)
            {
                MoveLine.Move(
                    selectionManager,
                    textManager,
                    selectionManager.currentTextSelection,
                    cursorManager.currentCursorPosition,
                    undoRedo,
                    e.Key == VirtualKey.Down ? LineMoveDirection.Down : LineMoveDirection.Up
                    );

                if (textRenderer.OutOfRenderedArea(cursorManager.LineNumber))
                {
                    if (e.Key == VirtualKey.Down)
                        ScrollOneLineDown();
                    else if (e.Key == VirtualKey.Up)
                        ScrollOneLineUp();
                }

                selectionManager.ForceClearSelection(canvasUpdateManager);
                canvasUpdateManager.UpdateAll();
                return;
            }
        }

        switch (e.Key)
        {
            case VirtualKey.Enter:
                textActionManager.AddNewLine();
                break;
            case VirtualKey.Back:
                textActionManager.RemoveText(ctrl);
                break;
            case VirtualKey.Delete:
                textActionManager.DeleteText(ctrl, shift);
                break;
            case VirtualKey.Left:
                {
                    if (shift)
                    {
                        selectionManager.StartSelectionIfNeeded();
                        cursorManager.MoveLeft(cursorManager.currentCursorPosition);
                        selectionRenderer.SetSelectionEnd(cursorManager.currentCursorPosition);
                    }
                    else
                    {
                        //Move the cursor to the start of the selection
                        if (selectionRenderer.HasSelection && selectionManager.currentTextSelection != null)
                            cursorManager.SetCursorPositionCopyValues(selectionManager.GetMin(selectionManager.currentTextSelection));
                        else
                            cursorManager.MoveLeft(CursorPosition);

                        selectionManager.ClearSelectionIfNeeded(this);
                    }

                    scrollManager.UpdateScrollToShowCursor();
                    canvasUpdateManager.UpdateAll();
                    break;
                }
            case VirtualKey.Right:
                {
                    if (shift)
                    {
                        selectionManager.StartSelectionIfNeeded();
                        cursorManager.MoveRight(cursorManager.currentCursorPosition);
                        selectionRenderer.SetSelectionEnd(cursorManager.currentCursorPosition);
                    }
                    else
                    {
                        //Move the cursor to the end of the selection
                        if (selectionRenderer.HasSelection && selectionManager.currentTextSelection != null)
                            cursorManager.SetCursorPositionCopyValues(selectionManager.GetMax(selectionManager.currentTextSelection));
                        else
                            cursorManager.MoveRight(cursorManager.currentCursorPosition);

                        selectionManager.ClearSelectionIfNeeded(this);
                    }

                    scrollManager.UpdateScrollToShowCursor(false);
                    canvasUpdateManager.UpdateAll();
                    break;
                }
            case VirtualKey.Down:
                {
                    if (shift)
                    {
                        selectionManager.StartSelectionIfNeeded();
                        cursorManager.MoveDown(cursorManager.currentCursorPosition);
                        selectionRenderer.SetSelectionEnd(cursorManager.currentCursorPosition);
                    }
                    else
                    {
                        selectionManager.ClearSelectionIfNeeded(this);
                        cursorManager.MoveDown(cursorManager.currentCursorPosition);
                    }

                    scrollManager.UpdateScrollToShowCursor(false);
                    canvasUpdateManager.UpdateAll();
                    break;
                }
            case VirtualKey.Up:
                {
                    if (shift)
                    {
                        selectionManager.StartSelectionIfNeeded();
                        cursorManager.MoveUp(cursorManager.currentCursorPosition);
                        selectionRenderer.SetSelectionEnd(cursorManager.currentCursorPosition);
                    }
                    else
                    {
                        selectionManager.ClearSelectionIfNeeded(this);
                        cursorManager.MoveUp(cursorManager.currentCursorPosition);
                    }

                    scrollManager.UpdateScrollToShowCursor(false);
                    canvasUpdateManager.UpdateAll();
                    break;
                }
            case VirtualKey.Escape:
                {
                    selectionDragDropManager.EndDragDropSelection();
                    ClearSelection();
                    break;
                }
            case VirtualKey.PageUp:
                ScrollPageUp();
                break;
            case VirtualKey.PageDown:
                ScrollPageDown();
                break;
            case VirtualKey.End:
                {
                    if (shift)
                    {
                        selectionRenderer.HasSelection = true;

                        if (selectionRenderer.SelectionStartPosition == null)
                            selectionRenderer.SelectionStartPosition = new CursorPosition(CursorPosition);

                        cursorManager.MoveToLineEnd(CursorPosition);
                        selectionRenderer.SelectionEndPosition = CursorPosition;
                        canvasUpdateManager.UpdateSelection();
                        canvasUpdateManager.UpdateCursor();
                    }
                    else
                    {
                        cursorManager.MoveToLineEnd(CursorPosition);
                        canvasUpdateManager.UpdateCursor();
                        canvasUpdateManager.UpdateText();
                    }
                    break;
                }
            case VirtualKey.Home:
                {
                    if (shift)
                    {
                        selectionRenderer.HasSelection = true;

                        if (selectionRenderer.SelectionStartPosition == null)
                            selectionRenderer.SelectionStartPosition = new CursorPosition(CursorPosition);
                        cursorManager.MoveToLineStart(CursorPosition);
                        selectionRenderer.SelectionEndPosition = CursorPosition;

                        canvasUpdateManager.UpdateSelection();
                        canvasUpdateManager.UpdateCursor();
                    }
                    else
                    {
                        cursorManager.MoveToLineStart(CursorPosition);
                        canvasUpdateManager.UpdateCursor();
                        canvasUpdateManager.UpdateText();
                    }
                    break;
                }
        }
    }

    private void Canvas_Selection_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        Canvas_Selection.ReleasePointerCapture(e.Pointer);

        pointerActionsManager.PointerReleasedAction(e.GetCurrentPoint(Canvas_Selection).Position);
    }
    private void Canvas_Selection_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!focusManager.HasFocus)
            return;

        var point = e.GetCurrentPoint(Canvas_Selection);

        if (pointerActionsManager.CheckTouchInput(point))
            return;

        if (point.Properties.IsLeftButtonPressed)
        {
            selectionRenderer.IsSelecting = true;
        }
        pointerActionsManager.PointerMovedAction(point.Position);

    }
    private void Canvas_Selection_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        Canvas_Selection.CapturePointer(e.Pointer);
        selectionRenderer.IsSelectingOverLinenumbers = false;

        var point = e.GetCurrentPoint(Canvas_Selection);
        if (pointerActionsManager.CheckTouchInput_Click(point))
            return;

        Point pointerPosition = point.Position;
        bool leftButtonPressed = point.Properties.IsLeftButtonPressed;
        bool ightButtonPressed = point.Properties.IsRightButtonPressed;

        if (leftButtonPressed && !Utils.IsKeyPressed(VirtualKey.Shift))
            pointerActionsManager.PointerClickCount++;

        if (pointerActionsManager.PointerClickCount == 3)
        {
            SelectLine(CursorPosition.LineNumber);
            pointerActionsManager.PointerClickCount = 0;
            return;
        }
        else if (pointerActionsManager.PointerClickCount == 2)
        {
            CursorHelper.UpdateCursorPosFromPoint(Canvas_Text,
                currentLineManager,
                textRenderer,
                scrollManager,
                pointerPosition,
                cursorManager.currentCursorPosition);

            selectionManager.SelectSingleWord(canvasUpdateManager, CursorPosition);
        }
        else
        {
            //TODO: Show the on screen keyboard if no physical keyboard is attached

            //Show the contextflyout
            if (ightButtonPressed)
            {
                if (!SelectionHelper.PointerIsOverSelection(textRenderer, pointerPosition, selectionManager.currentTextSelection))
                {
                    selectionManager.ForceClearSelection(canvasUpdateManager);

                    CursorHelper.UpdateCursorPosFromPoint(Canvas_Text,
                        currentLineManager,
                        textRenderer,
                        scrollManager,
                        pointerPosition,
                        cursorManager.currentCursorPosition);
                }

                if (!ContextFlyoutDisabled && ContextFlyout != null)
                {
                    ContextFlyout.ShowAt(sender as FrameworkElement, new FlyoutShowOptions { Position = pointerPosition });
                }
            }

            //Shift + click = set selection
            if (Utils.IsKeyPressed(VirtualKey.Shift) && leftButtonPressed)
            {
                if (selectionRenderer.SelectionStartPosition == null)
                    selectionRenderer.SetSelectionStart(new CursorPosition(CursorPosition));

                CursorHelper.UpdateCursorPosFromPoint(Canvas_Text,
                    currentLineManager,
                    textRenderer,
                    scrollManager,
                    pointerPosition,
                    cursorManager.currentCursorPosition);

                selectionRenderer.SetSelectionEnd(new CursorPosition(CursorPosition));
                canvasUpdateManager.UpdateSelection();
                canvasUpdateManager.UpdateCursor();
                return;
            }

            if (leftButtonPressed)
            {
                CursorHelper.UpdateCursorPosFromPoint(Canvas_Text,
                    currentLineManager,
                    textRenderer,
                    scrollManager,
                    pointerPosition,
                    cursorManager.currentCursorPosition);

                //Text drag/drop
                if (selectionManager.currentTextSelection != null)
                {
                    if (SelectionHelper.PointerIsOverSelection(textRenderer, pointerPosition, selectionManager.currentTextSelection) && !selectionDragDropManager.isDragDropSelection)
                    {
                        pointerActionsManager.PointerClickCount = 0;
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
                if (selectionRenderer.HasSelection)
                {
                    selectionManager.ForceClearSelection(canvasUpdateManager);
                    selectionRenderer.SelectionStartPosition = new CursorPosition(CursorPosition);
                }
                else
                {
                    selectionRenderer.SetSelectionStart(new CursorPosition(CursorPosition));
                    selectionRenderer.IsSelecting = true;
                }
            }
            canvasUpdateManager.UpdateCursor();
        }

        pointerActionsManager.PointerClickTimer.Start();
        pointerActionsManager.PointerClickTimer.Tick += (s, t) =>
        {
            pointerActionsManager.PointerClickTimer.Stop();
            pointerActionsManager.PointerClickCount = 0;
        };
    }
    private void Canvas_Selection_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        var delta = e.GetCurrentPoint(Canvas_Selection).Properties.MouseWheelDelta;
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
            HorizontalScrollbar.Value -= delta * HorizontalScrollSensitivity;
            needsUpdate = true;
        }
        //Scroll horizontal using touchpad
        else if (e.GetCurrentPoint(Canvas_Selection).Properties.IsHorizontalMouseWheel)
        {
            HorizontalScrollbar.Value += delta * HorizontalScrollSensitivity;
            needsUpdate = true;
        }
        //Scroll vertical using mousewheel
        else
        {
            VerticalScrollbar.Value -= (delta * VerticalScrollSensitivity) / scrollManager.DefaultVerticalScrollSensitivity;
            //Only update when a line was scrolled
            if ((int)(VerticalScrollbar.Value / textRenderer.SingleLineHeight * scrollManager.DefaultVerticalScrollSensitivity) != textRenderer.NumberOfStartLine)
            {
                needsUpdate = true;
            }
        }

        if (selectionRenderer.IsSelecting)
        {
            CursorHelper.UpdateCursorPosFromPoint(Canvas_Text,
                currentLineManager,
                textRenderer,
                scrollManager,
                e.GetCurrentPoint(Canvas_Selection).Position,
                cursorManager.currentCursorPosition);

            canvasUpdateManager.UpdateCursor();

            selectionRenderer.SelectionEndPosition = new CursorPosition(CursorPosition.CharacterPosition, CursorPosition.LineNumber);
            needsUpdate = true;
        }
        if (needsUpdate)
            canvasUpdateManager.UpdateAll();
    }
    private void Canvas_LineNumber_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        Canvas_Selection.CapturePointer(e.Pointer);

        var point = e.GetCurrentPoint(Canvas_Selection);
        if (pointerActionsManager.CheckTouchInput_Click(point))
            return;

        //Select the line where the cursor is over
        SelectLine(CursorHelper.GetCursorLineFromPoint(textRenderer, point.Position));

        selectionRenderer.IsSelecting = true;
        selectionRenderer.IsSelectingOverLinenumbers = true;
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
    private void Canvas_Text_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        if (!loadingManager.IsTextboxLoaded)
            return;

        textRenderer.Draw(sender, args);
    }
    private void Canvas_Selection_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        if (!loadingManager.IsTextboxLoaded)
            return;

        selectionRenderer.Draw(sender, args);
    }
    private void Canvas_Cursor_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        if (!loadingManager.IsTextboxLoaded)
            return;

        cursorRenderer.Draw(Canvas_Text, Canvas_Cursor, args);
    }
    private void Canvas_LineNumber_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        if (!loadingManager.IsTextboxLoaded)
            return;

        if (!lineNumberManager._ShowLineNumbers)
        {
            lineNumberRenderer.HideLineNumbers(sender, lineNumberManager._SpaceBetweenLineNumberAndText);
            return;
        }

        lineNumberRenderer.Draw(Canvas_LineNumber, args, lineNumberManager._SpaceBetweenLineNumberAndText);
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
    private void UserControl_GotFocus(object sender, RoutedEventArgs e)
    {
        focusManager.SetFocus();
    }
    private void UserControl_LostFocus(object sender, RoutedEventArgs e)
    {
        focusManager.RemoveFocus();
    }
    //Cursor:
    private void Canvas_LineNumber_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ChangeCursor(InputSystemCursorShape.Arrow);
    }
    private void Canvas_LineNumber_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        ChangeCursor(InputSystemCursorShape.IBeam);
    }
    private void Scrollbar_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        ChangeCursor(InputSystemCursorShape.IBeam);
    }
    private void Scrollbar_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ChangeCursor(InputSystemCursorShape.Arrow);
    }
    //Drag Drop text
    private async void UserControl_Drop(object sender, DragEventArgs e)
    {
        if (textManager._IsReadonly)
            return;

        if (e.DataView.Contains(StandardDataFormats.Text))
        {
            textActionManager.AddCharacter(stringManager.CleanUpString(await e.DataView.GetTextAsync()), true);
        }
    }
    private void UserControl_DragOver(object sender, DragEventArgs e)
    {
        if (selectionRenderer.IsSelecting || textManager._IsReadonly || !e.DataView.Contains(StandardDataFormats.Text))
            return;

        var deferral = e.GetDeferral();

        e.AcceptedOperation = DataPackageOperation.Copy;
        e.DragUIOverride.IsGlyphVisible = false;
        e.DragUIOverride.IsContentVisible = false;
        deferral.Complete();

        CursorHelper.UpdateCursorPosFromPoint(Canvas_Text, currentLineManager, textRenderer, scrollManager, e.GetPosition(Canvas_Text), cursorManager.currentCursorPosition);
        canvasUpdateManager.UpdateCursor();
    }

    public void SelectLine(int line)
    {
        selectionRenderer.SetSelection(new CursorPosition(0, line), new CursorPosition(textManager.GetLineLength(line), line));
        CursorPosition = selectionRenderer.SelectionEndPosition;

        canvasUpdateManager.UpdateSelection();
        canvasUpdateManager.UpdateCursor();
    }

    public void GoToLine(int line)
    {
        if (line >= textManager.LinesCount || line < 0)
            return;

        selectionRenderer.SelectionEndPosition = null;
        CursorPosition = selectionRenderer.SelectionStartPosition = new CursorPosition(0, line);

        ScrollLineIntoView(line);
        this.Focus(FocusState.Programmatic);

        canvasUpdateManager.UpdateAll();
    }

    public void LoadText(string text)
    {
        textActionManager.Safe_LoadText(text);
    }

    public void SetText(string text)
    {
        textActionManager.Safe_SetText(text);
    }

    public void LoadLines(IEnumerable<string> lines, LineEnding lineEnding = LineEnding.CRLF)
    {
        textActionManager.Safe_LoadLines(lines, lineEnding);
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
        var result = selectionManager.GetSelectionFromPosition(start, length, CharacterCount);
        if (result != null)
        {
            selectionRenderer.SetSelection(result.StartPosition, result.EndPosition);
            if (result.EndPosition != null)
                CursorPosition = result.EndPosition;
        }

        canvasUpdateManager.UpdateSelection();
        canvasUpdateManager.UpdateCursor();
    }

    public void SelectAll()
    {
        textActionManager.SelectAll();
    }

    public void ClearSelection()
    {
        selectionManager.ForceClearSelection(canvasUpdateManager);
    }

    public void Undo()
    {
        textActionManager.Undo();
    }

    public void Redo()
    {
        textActionManager.Redo();
    }

    public void ScrollLineToCenter(int line)
    {
        scrollManager.ScrollLineToCenter(line);
    }

    public void ScrollOneLineUp()
    {
        scrollManager.ScrollOneLineUp();

    }

    public void ScrollOneLineDown()
    {
        scrollManager.ScrollOneLineDown();
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
        if (startLine + length >= textManager.LinesCount)
            return textManager.GetString();

        return textManager.GetLines(startLine, length).GetString(textManager.NewLineCharacter);
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

    public void SurroundSelectionWith(string text)
    {
        text = stringManager.CleanUpString(text);
        SurroundSelectionWith(text, text);
    }

    public void SurroundSelectionWith(string text1, string text2)
    {
        if (!selectionManager.SelectionIsNull(selectionManager.currentTextSelection))
        {
            textActionManager.AddCharacter(stringManager.CleanUpString(text1) + SelectedText + stringManager.CleanUpString(text2));
        }
    }

    public void DuplicateLine(int line)
    {
        textActionManager.DuplicateLine(line);
    }
    public void DuplicateLine()
    {
        textActionManager.DuplicateLine(CursorPosition.LineNumber);
    }

    public SearchResult ReplaceAll(string word, string replaceWord, bool matchCase, bool wholeWord)
    {
        if (word.Length == 0 || replaceWord.Length == 0)
            return SearchResult.InvalidInput;

        SearchParameter searchParameter = new SearchParameter(word, wholeWord, matchCase);

        bool isFound = false;
        undoRedo.RecordUndoAction(() =>
        {
            for (int i = 0; i < textManager.LinesCount; i++)
            {
                if (textManager.totalLines[i].Contains(searchParameter))
                {
                    isFound = true;
                    SetLineText(i, Regex.Replace(textManager.totalLines[i], searchParameter.SearchExpression, replaceWord));
                }
            }
        }, 0, textManager.LinesCount, textManager.LinesCount);
        canvasUpdateManager.UpdateText();
        return isFound ? SearchResult.Found : SearchResult.NotFound;
    }

    public SearchResult FindNext()
    {
        if (!searchManager.IsSearchOpen)
            return SearchResult.SearchNotOpened;

        var res = searchManager.FindNext(CursorPosition);
        if (res.Selection != null)
        {
            selectionRenderer.SetSelection(res.Selection);
            ScrollLineIntoView(CursorPosition.LineNumber);
            canvasUpdateManager.UpdateText();
            canvasUpdateManager.UpdateSelection();
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
            selectionRenderer.SetSelection(res.Selection);
            ScrollLineIntoView(CursorPosition.LineNumber);
            canvasUpdateManager.UpdateText();
            canvasUpdateManager.UpdateSelection();
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

    public void Unload()
    {
        //Unsubscribe from events:
        inputHandler.PreviewKeyDown -= InputHandler_KeyDown;
        inputHandler.TextEntered -= InputHandler_TextEntered;

        //Dispose and null larger objects
        textManager.totalLines.Dispose();
        textRenderer.RenderedLines = null;
        lineNumberRenderer.LineNumberTextToRender = lineNumberRenderer.OldLineNumberTextToRender = null;
        undoRedo.NullAll();
    }

    public void ClearUndoRedoHistory()
    {
        undoRedo.ClearAll();
    }

    public Point GetCursorPosition()
    {
        return new Point
        {
            Y = (float)((CursorPosition.LineNumber - textRenderer.NumberOfStartLine) * textRenderer.SingleLineHeight) + textRenderer.SingleLineHeight / scrollManager.DefaultVerticalScrollSensitivity,
            X = CursorHelper.GetCursorPositionInLine(textRenderer.CurrentLineTextLayout, CursorPosition, 0)
        };
    }

    public void SelectCodeLanguage(CodeLanguageId languageId)
    {
        if (CodeLanguages.TryGetValue(languageId, out SyntaxHighlightLanguage codelanguage))
            HighlightLanguage = codelanguage;
    }

    public bool SyntaxHighlighting { get; set; } = true;

    public SyntaxHighlightLanguage HighlightLanguage
    {
        get => textManager._CodeLanguage;
        set
        {
            textManager._CodeLanguage = value;
            textRenderer.NeedsUpdateTextLayout = true;
            canvasUpdateManager.UpdateText();
        }
    }

    public LineEnding LineEnding
    {
        get => textManager._LineEnding;
        set
        {
            textManager._LineEnding = value;
        }
    }

    public float SpaceBetweenLineNumberAndText { get => lineNumberManager._SpaceBetweenLineNumberAndText; set { lineNumberManager._SpaceBetweenLineNumberAndText = value; lineNumberRenderer.NeedsUpdateLineNumbers(); canvasUpdateManager.UpdateAll(); } }

    public CursorPosition CursorPosition
    {
        get => cursorManager.currentCursorPosition;
        set { cursorManager.LineNumber = value.LineNumber;  cursorManager.CharacterPosition = value.CharacterPosition; canvasUpdateManager.UpdateCursor(); }
    }

    public new FontFamily FontFamily { get => textManager._FontFamily; set { textManager._FontFamily = value; textRenderer.NeedsTextFormatUpdate = true; canvasUpdateManager.UpdateAll(); } }

    public new int FontSize { get => textManager._FontSize; set { textManager._FontSize = value; zoomManager.UpdateZoom(); } }

    public float RenderedFontSize => zoomManager.ZoomedFontSize;

    public string Text { get => GetText(); set { SetText(value); } }

    public new ElementTheme RequestedTheme
    {
        get => designHelper.RequestedTheme;
        set => designHelper.RequestedTheme = value;
    }

    public TextControlBoxDesign Design
    {
        get => designHelper.Design;
        set => designHelper.Design = value;
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

    public bool IsReadonly { get; set; } //TODO

    public CursorSize CursorSize { get => cursorRenderer._CursorSize; set { cursorRenderer._CursorSize = value; canvasUpdateManager.UpdateCursor(); } }

    public new MenuFlyout ContextFlyout
    {
        get { return flyoutHelper.MenuFlyout; }
        set
        {
            if (value == null) //Use the builtin flyout
            {
                flyoutHelper.CreateFlyout(this);
            }
            else //Use a custom flyout
            {
                flyoutHelper.MenuFlyout = value;
            }
        }
    }
    public bool ContextFlyoutDisabled { get; set; }
    public int SelectionStart { get => selectionRenderer.SelectionStart; set { SetSelection(value, SelectionLength); } }
    public int SelectionLength { get => selectionRenderer.SelectionLength; set { SetSelection(SelectionStart, value); } }
    public string SelectedText
    {
        get
        {
            if (selectionManager.currentTextSelection != null && selectionManager.WholeTextSelected(selectionManager.currentTextSelection))
                return GetText();
            return selectionManager.GetSelectedText(selectionManager.currentTextSelection, CursorPosition.LineNumber);
        }
        set => textActionManager.AddCharacter(stringManager.CleanUpString(value));
    }
    public int NumberOfLines { get => textManager.LinesCount; }

    public int CurrentLineIndex { get => CursorPosition.LineNumber; }
    public ScrollBarPosition ScrollBarPosition
    {
        get => new ScrollBarPosition(HorizontalScrollbar.Value, VerticalScroll);
        set { HorizontalScrollbar.Value = value.ValueX; VerticalScroll = value.ValueY; }
    }
    public int CharacterCount => textManager.CountCharacters();
    public double VerticalScrollSensitivity { get => scrollManager._VerticalScrollSensitivity; set => scrollManager._VerticalScrollSensitivity = value < 1 ? 1 : value; }
    public double HorizontalScrollSensitivity { get => scrollManager._HorizontalScrollSensitivity; set => scrollManager._HorizontalScrollSensitivity = value < 1 ? 1 : value; }
    public double VerticalScroll { get => VerticalScrollbar.Value; set { VerticalScrollbar.Value = value < 0 ? 0 : value; canvasUpdateManager.UpdateAll(); } }
    public double HorizontalScroll { get => HorizontalScrollbar.Value; set { HorizontalScrollbar.Value = value < 0 ? 0 : value; canvasUpdateManager.UpdateAll(); } }
    public new CornerRadius CornerRadius { get => MainGrid.CornerRadius; set => MainGrid.CornerRadius = value; }
    public bool UseSpacesInsteadTabs { get => tabSpaceHelper.UseSpacesInsteadTabs; set { tabSpaceHelper.UseSpacesInsteadTabs = value; tabSpaceHelper.UpdateTabs(); canvasUpdateManager.UpdateAll(); } }
    public int NumberOfSpacesForTab { get => tabSpaceHelper.NumberOfSpaces; set { tabSpaceHelper.NumberOfSpaces = value; tabSpaceHelper.UpdateNumberOfSpaces(); canvasUpdateManager.UpdateAll(); } }
    public bool SearchIsOpen => searchManager.IsSearchOpen;
    public IEnumerable<string> Lines => textManager.totalLines;
    public bool DoAutoPairing { get; set; } = true;

    public delegate void TextChangedEvent();
    public event TextChangedEvent TextChanged;

    public delegate void SelectionChangedEvent(SelectionChangedEventHandler args);
    public event SelectionChangedEvent SelectionChanged;

    public delegate void ZoomChangedEvent(int zoomFactor);
    public event ZoomChangedEvent ZoomChanged;

    public delegate void GotFocusEvent();
    public new event GotFocusEvent GotFocus;

    public delegate void LostFocusEvent();
    public new event LostFocusEvent LostFocus;


    public static Dictionary<CodeLanguageId, SyntaxHighlightLanguage> CodeLanguages => new Dictionary<CodeLanguageId, SyntaxHighlightLanguage>()
        {
            { CodeLanguageId.Batch, new Batch() },
            { CodeLanguageId.Cpp, new Cpp() },
            { CodeLanguageId.CSharp, new CSharp() },
            { CodeLanguageId.ConfigFile, new ConfigFile() },
            { CodeLanguageId.CSS, new CSS() },
            { CodeLanguageId.CSV, new CSV() },
            { CodeLanguageId.GCode, new GCode() },
            { CodeLanguageId.HexFile, new HexFile() },
            { CodeLanguageId.Html, new Html() },
            { CodeLanguageId.Java, new Java() },
            { CodeLanguageId.Javascript, new Javascript() },
            { CodeLanguageId.Json, new Json() },
            { CodeLanguageId.Latex, new LaTex() },
            { CodeLanguageId.Markdown, new Markdown() },
            { CodeLanguageId.PHP, new PHP() },
            { CodeLanguageId.Python, new Python() },
            { CodeLanguageId.QSharp, new QSharp() },
            { CodeLanguageId.TOML, new TOML() },
            { CodeLanguageId.XML, new XML() },
            { CodeLanguageId.None, null },
        };

    public static SyntaxHighlightLanguage GetCodeLanguageFromId(CodeLanguageId languageId)
    {
        if (CodeLanguages.TryGetValue(languageId, out SyntaxHighlightLanguage codelanguage))
            return codelanguage;
        return null;
    }

    public static JsonLoadResult GetCodeLanguageFromJson(string Json)
    {
        return SyntaxHighlightingRenderer.GetCodeLanguageFromJson(Json);
    }
}