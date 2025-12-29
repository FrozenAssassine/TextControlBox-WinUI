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

internal sealed partial class CoreTextControlBox : UserControl
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
    private readonly MoveLineManager moveLineManager;
    private readonly WhitespaceCharactersRenderer invisibleCharactersRenderer;
    private readonly WhitespaceCharactersManager whitespaceCharactersManager;
    private readonly LinkHighlightManager linkHighlightManager;
    private readonly LinkRenderer linkRenderer;

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
        textActionManager.Init(this, textRenderer, undoRedo, currentLineManager, longestLineManager, canvasUpdateManager, textManager, selectionRenderer, cursorManager, scrollManager, eventsManager, stringManager, selectionManager, autoIndentionManager);
        textRenderer.Init(cursorManager, designHelper, textLayoutManager, textManager, scrollManager, lineNumberRenderer, longestLineManager, this, searchManager, canvasUpdateManager, zoomManager, invisibleCharactersRenderer, linkRenderer, linkHighlightManager);
        cursorRenderer.Init(cursorManager, currentLineManager, textRenderer, focusManager, textManager, scrollManager, zoomManager, designHelper, lineHighlighterRenderer, eventsManager, longestLineManager);
        scrollManager.Init(this, canvasUpdateManager, textManager, textRenderer, cursorManager, zoomManager, VerticalScrollbar, HorizontalScrollbar);
        currentLineManager.Init(cursorManager, textManager);
        longestLineManager.Init(selectionManager, textManager, textRenderer);
        designHelper.Init(this, textRenderer, canvasUpdateManager);
        tabSpaceManager.Init(textManager, selectionManager, cursorManager, textActionManager, undoRedo, longestLineManager, eventsManager);
        searchManager.Init(textManager);
        eventsManager.Init(searchManager, cursorManager);
        lineNumberRenderer.Init(textManager, textLayoutManager, textRenderer, designHelper, lineNumberManager);
        zoomManager.Init(textManager, textRenderer, canvasUpdateManager, eventsManager, lineNumberRenderer);
        focusManager.Init(this, canvasUpdateManager, inputHandler, eventsManager);
        pointerActionsManager.Init(this, textRenderer, textManager, cursorManager, canvasUpdateManager, scrollManager, selectionRenderer, currentLineManager, selectionManager, linkHighlightManager);
        textLayoutManager.Init(textManager, zoomManager);
        autoIndentionManager.Init(textManager, tabSpaceManager);
        replaceManager.Init(canvasUpdateManager, undoRedo, textManager, searchManager, cursorManager, textActionManager, selectionRenderer, selectionManager, eventsManager);
        initializationManager.Init(eventsManager);
        moveLineManager.Init(selectionManager, cursorManager, textManager, undoRedo);
        invisibleCharactersRenderer.Init(designHelper, scrollManager, zoomManager, textLayoutManager, whitespaceCharactersManager);
        linkHighlightManager.Init(textRenderer, this, eventsManager);
        linkRenderer.Init(textRenderer, linkHighlightManager);
    }

    public void InitialiseOnStart()
    {
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
        if (IsReadOnly || inputHandler.Text.Equals("\t", StringComparison.OrdinalIgnoreCase))
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
                    if (ControlW_SelectWord)
                        selectionManager.SelectSingleWord(canvasUpdateManager);
                    break;
            }

            if (e.Key != VirtualKey.Left && e.Key != VirtualKey.Right && e.Key != VirtualKey.Back && e.Key != VirtualKey.Delete)
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
                    break;
                }
            case VirtualKey.Right:
                {
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
                    break;
                }
            case VirtualKey.Down:
                {
                    if (shift)
                    {
                        selectionManager.StartSelectionIfNeeded();
                        cursorManager.MoveDown();
                        selectionManager.SetSelectionEnd(cursorManager.currentCursorPosition);
                    }
                    else
                    {
                        selectionManager.ClearSelectionIfNeeded(this);
                        cursorManager.MoveDown();
                    }

                    scrollManager.UpdateScrollToShowCursor(true);
                    break;
                }
            case VirtualKey.Up:
                {
                    if (shift)
                    {
                        selectionManager.StartSelectionIfNeeded();
                        cursorManager.MoveUp();
                        selectionManager.SetSelectionEnd(cursorManager.currentCursorPosition);
                    }
                    else
                    {
                        selectionManager.ClearSelectionIfNeeded(this);
                        cursorManager.MoveUp();
                    }

                    scrollManager.UpdateScrollToShowCursor(true);
                    break;
                }
            case VirtualKey.Escape:
                {
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
                        selectionManager.HasSelection = true;

                        if (selectionManager.selectionStart.IsNull)
                            selectionManager.SetSelectionStart(CursorPosition);

                        cursorManager.MoveToLineEnd(CursorPosition);
                        selectionManager.SetSelectionEnd(CursorPosition);
                        canvasUpdateManager.UpdateSelection();
                    }
                    else
                    {
                        cursorManager.MoveToLineEnd(CursorPosition);
                        canvasUpdateManager.UpdateText();
                    }
                    canvasUpdateManager.UpdateCursor();

                    break;
                }
            case VirtualKey.Home:
                {
                    if (shift)
                    {
                        selectionManager.HasSelection = true;

                        if (selectionManager.selectionStart.IsNull)
                            selectionManager.SetSelectionStart(CursorPosition);

                        cursorManager.MoveToLineStart(CursorPosition);
                        selectionManager.SetSelectionEnd(CursorPosition);

                        canvasUpdateManager.UpdateSelection();
                    }
                    else
                    {
                        cursorManager.MoveToLineStart(CursorPosition);
                        canvasUpdateManager.UpdateText();
                    }
                    canvasUpdateManager.UpdateCursor();
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

        pointerActionsManager.PointerMovedAction(point.Position);

    }
    private void Canvas_Selection_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        Canvas_Selection.CapturePointer(e.Pointer);
        selectionManager.IsSelectingOverLinenumbers = false;

        var point = e.GetCurrentPoint(Canvas_Selection);
        if (pointerActionsManager.CheckTouchInput_Click(point))
            return;

        pointerActionsManager.PointerPressedAction(sender, point.Position, point.Properties);
    }
    private void Canvas_Selection_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        pointerActionsManager.PointerWheelAction(zoomManager, e);
    }
    private void Canvas_LineNumber_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        Canvas_Selection.CapturePointer(e.Pointer);

        var point = e.GetCurrentPoint(Canvas_Selection);
        if (pointerActionsManager.CheckTouchInput_Click(point))
            return;

        //Select the line where the cursor is over
        SelectLine(CursorHelper.GetCursorLineFromPoint(textRenderer, point.Position));

        selectionManager.IsSelecting = true;
        selectionManager.IsSelectingOverLinenumbers = true;
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
        textRenderer.Draw(sender, args);
        initializationManager.CanvasDrawed(0);
    }
    private void Canvas_Selection_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        selectionRenderer.Draw(sender, args);
        initializationManager.CanvasDrawed(1);
    }
    private void Canvas_Cursor_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        cursorRenderer.Draw(Canvas_Text, Canvas_Cursor, args);
        initializationManager.CanvasDrawed(2);
    }
    private void Canvas_LineNumber_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
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
    private void InputManager_GotFocus(object sender, RoutedEventArgs e)
    {
        focusManager.SetFocus();
    }
    private void InputManager_LostFocus(object sender, RoutedEventArgs e)
    {
        focusManager.RemoveFocus();
    }

    public new void Focus(FocusState state)
    {
        inputHandler.Focus(state);
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
        if (line >= textManager.LinesCount)
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
        if (start + count >= textManager.LinesCount)
            return false;

        int endLineLength = textManager.GetLineLength(start + count);
        
        selectionManager.SetSelection(start, 0, start + count, endLineLength);
        cursorManager.SetCursorPosition(start + count, endLineLength);

        canvasUpdateManager.UpdateSelection();
        canvasUpdateManager.UpdateCursor();
        return true;
    }

    public void GoToLine(int line)
    {
        if (line >= textManager.LinesCount || line < 0)
            return;

        selectionManager.selectionEnd.IsNull = true;
        cursorManager.SetCursorPosition(line, 0);
        selectionManager.SetSelectionStart(line, 0);

        ScrollLineIntoView(line);
        this.Focus(FocusState.Programmatic);

        canvasUpdateManager.UpdateAll();
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

    public void SurroundSelectionWith(string text)
    {
        SurroundSelectionWith(text, text);
    }

    public void SurroundSelectionWith(string text1, string text2)
    {
        if (selectionManager.HasSelection)
        {
            textActionManager.AddCharacter(stringManager.CleanUpString(text1) + SelectedText + stringManager.CleanUpString(text2));
        }
    }

    public bool DuplicateLine(int line)
    {
        if (line >= textManager.LinesCount || line < 0)
            return false;
        
        textActionManager.DuplicateLine(line);
        return true;
    }
    public void DuplicateCurrentLine()
    {
        textActionManager.DuplicateLine(CursorPosition.LineNumber);
    }

    public SearchResult ReplaceAll(string word, string replaceWord, bool matchCase, bool wholeWord, bool ignoreIsReadonly = false)
    {
        if (!ignoreIsReadonly && IsReadOnly)
            return SearchResult.ReplaceNotAllowedInReadonly;

        return replaceManager.ReplaceAll(word, replaceWord, matchCase, wholeWord);
    }

    public SearchResult ReplaceNext(string replaceWord, bool ignoreIsReadonly = false)
    {
        if (!ignoreIsReadonly && IsReadOnly)
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

    public void Unload()
    {
        //Unsubscribe from events:
        inputHandler.PreviewKeyDown -= InputHandler_KeyDown;
        inputHandler.TextEntered -= InputHandler_TextEntered;

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
        return new Point
        {
            Y = (float)((CursorPosition.LineNumber - textRenderer.NumberOfStartLine) * textRenderer.SingleLineHeight) + textRenderer.SingleLineHeight / scrollManager.DefaultVerticalScrollSensitivity,
            X = CursorHelper.GetCursorPositionInLine(textRenderer.CurrentLineTextLayout, CursorPosition, 0)
        };
    }

    public void SetCursorPosition(int lineNumber, int characterPos, bool scrollIntoView = true, bool autoClamp = true)
    {
        if (autoClamp)
        {
            lineNumber = Math.Clamp(lineNumber, 0, textManager.totalLines.Count - 1);
            int length = textManager.GetLineLength(lineNumber);
            characterPos = Math.Clamp(characterPos, 0, length);
        }
        else if (lineNumber < 0 || lineNumber >= textManager.totalLines.Count || characterPos < 0 || characterPos >= textManager.GetLineLength(lineNumber))
                throw new IndexOutOfRangeException("Invalid line number or character position provided for SetCursorPosition");
        
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
        set => textManager.LineEnding = value;
    }

    public float SpaceBetweenLineNumberAndText { get => lineNumberManager._SpaceBetweenLineNumberAndText; set { lineNumberManager._SpaceBetweenLineNumberAndText = value; lineNumberRenderer.NeedsUpdateLineNumbers(); canvasUpdateManager.UpdateAll(); } }

    public CursorPosition CursorPosition
    {
        get => cursorManager.currentCursorPosition;
        set { cursorManager.LineNumber = value.LineNumber; cursorManager.CharacterPosition = value.CharacterPosition; canvasUpdateManager.UpdateCursor(); }
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

    public bool IsReadOnly { get => textManager._IsReadOnly; set => textManager._IsReadOnly = value; }

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
        set => textActionManager.AddCharacter(stringManager.CleanUpString(value));
    }
    public void RewriteTabsSpaces(int spaces, bool useSpacesInsteadTabs, bool ignoreIsReadonly = false)
    {
        if(spaces <= 0)
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
    public bool ControlW_SelectWord = true;
    public bool HasSelection => selectionManager.HasSelection;
    public TextControlBoxSelection? CurrentSelection => selectionManager.HasSelection ? new TextControlBoxSelection(this.selectionManager.currentTextSelection) : null;
    public TextControlBoxSelection? CurrentSelectionOrdered => selectionManager.HasSelection ? new TextControlBoxSelection(selectionManager) : null;
    public new bool IsLoaded => initializationManager.initDone;
    public bool ShowWhitespaceCharacters { get => whitespaceCharactersManager.ShowWhitespaceCharacters; set { whitespaceCharactersManager.ShowWhitespaceCharacters = value; canvasUpdateManager.UpdateText(); } }
    public Thickness SelectionScrollStartBorderDistance { get; set; } = new Thickness(0, 0, 0, 0);
    public bool HighlightLinks { get => linkHighlightManager.HighlightLinks; set { linkHighlightManager.HighlightLinks = value; canvasUpdateManager.UpdateAll(); } }
    public bool HighlightLineWhenNotFocused { get => lineHighlighterManager._HighlightLineWhenNotFocused; set { lineHighlighterManager._HighlightLineWhenNotFocused = value; canvasUpdateManager.UpdateText(); } }
    public bool CanUndo => undoRedo.CanUndo;
    public bool CanRedo => undoRedo.CanRedo;

    public static Dictionary<SyntaxHighlightID, SyntaxHighlightLanguage> SyntaxHighlightings => new Dictionary<SyntaxHighlightID, SyntaxHighlightLanguage>()
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
            { SyntaxHighlightID.CSV, new CSV() },
            { SyntaxHighlightID.GCode, new GCode() },
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