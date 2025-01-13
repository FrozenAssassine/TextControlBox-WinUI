using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TextControlBoxNS.Extensions;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Languages;
using TextControlBoxNS.Renderer;
using TextControlBoxNS.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System;

namespace TextControlBoxNS
{
    public partial class TextControlBox : UserControl
    {
        bool ColorResourcesCreated = false;
        bool HasFocus = true;

        CanvasTextLayout LineNumberTextLayout = null;
        CanvasTextFormat LineNumberTextFormat = null;

        //Handle double and triple -clicks:
        int PointerClickCount = 0;
        DispatcherTimer PointerClickTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 200) };

        Point? OldTouchPosition = null;

        //CursorPosition
        CursorPosition _CursorPosition = new CursorPosition(0, 0);
        CursorPosition OldCursorPosition = null;
        TextSelection TextSelection = null;
        TextSelection OldTextSelection = null;

        //Classes
        private readonly SelectionRenderer selectionrenderer;
        private readonly FlyoutHelper flyoutHelper;
        private readonly TabSpaceHelper tabSpaceHelper = new TabSpaceHelper();
        private readonly StringManager stringManager;
        private readonly SearchManager searchManager;
        private readonly CanvasHelper canvasHelper;
        private readonly LineNumberRenderer lineNumberRenderer = new LineNumberRenderer();
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
        private readonly EventsManager eventsManager;

        /// <summary>
        /// Initializes a new instance of the TextControlBox class.
        /// </summary>
        public TextControlBox()
        {
            this.InitializeComponent();

            //Classes & Variables:
            cursorManager = new CursorManager(textManager, currentLineManager);
            selectionManager = new SelectionManager(textManager, cursorManager);
            undoRedo = new UndoRedo(textManager, selectionManager);
            selectionrenderer = new SelectionRenderer(selectionManager);
            flyoutHelper = new FlyoutHelper(this);
            stringManager = new StringManager(tabSpaceHelper);
            canvasHelper = new CanvasHelper(Canvas_Selection, Canvas_LineNumber, Canvas_Text, Canvas_Cursor);
            textActionManager = new TextActionManager(this, undoRedo, currentLineManager, longestLineManager, canvasHelper, textManager, selectionrenderer, cursorManager);
            textRenderer = new TextRenderer(cursorManager, textManager, scrollManager);
            cursorRenderer = new CursorRenderer();
            scrollManager = new ScrollManager(canvasHelper,textManager, textRenderer, cursorManager, VerticalScrollbar, HorizontalScrollbar);
            currentLineManager = new CurrentLineManager(cursorManager);
            longestLineManager = new LongestLineManager(selectionManager);
            zoomManager = new ZoomManager(textManager, textRenderer, canvasHelper, lineNumberRenderer, cursorManager, eventsManager);
            designHelper = new DesignHelper();
            lineHighlighterManager = new LineHighlighterManager();
            lineNumberManager = new LineNumberManager();
            searchManager = new SearchManager(textManager);
            eventsManager = new EventsManager(searchManager, selectionManager, cursorManager, selectionrenderer, textManager);

            //subscribe to events:
            eventsManager.ZoomChanged += ZoomManager_ZoomChanged;
            eventsManager.TextChanged += EventsManager_TextChanged;
            eventsManager.SelectionChanged += EventsManager_SelectionChanged;

            //set default values
            RequestedTheme = ElementTheme.Default;
            LineEnding = LineEnding.CRLF;

            InitialiseOnStart();
            SetFocus();
        }

        private void EventsManager_SelectionChanged(SelectionChangedEventHandler args)
        {
            SelectionChanged?.Invoke(this, args);
        }
        private void EventsManager_TextChanged()
        {
            TextChanged?.Invoke(this);
        }
        private void ZoomManager_ZoomChanged(int zoomFactor)
        {
            ZoomChanged?.Invoke(this, zoomFactor);
        }

        private void ChangeCursor(InputSystemCursorShape cursor)
        {
            this.ProtectedCursor = InputSystemCursor.Create(cursor);
        }

        #region Random functions
        private void InitialiseOnStart()
        {
            zoomManager.UpdateZoom();
            if (textManager.LinesCount == 0)
            {
                textManager.AddLine();
            }
        }

        private void SetFocus()
        {
            if (!HasFocus)
                GotFocus?.Invoke(this);
            HasFocus = true;

            canvasHelper.UpdateCursor();
            inputHandler.Focus(FocusState.Programmatic);
            ChangeCursor(InputSystemCursorShape.IBeam);
        }
        private void RemoveFocus()
        {
            if (HasFocus)
                LostFocus?.Invoke(this);
            canvasHelper.UpdateCursor();

            HasFocus = false;
            ChangeCursor(InputSystemCursorShape.Arrow);
        }

        private void PointerReleasedAction(Point point)
        {
            OldTouchPosition = null;
            selectionrenderer.IsSelectingOverLinenumbers = false;

            //End text drag/drop -> insert text at cursorposition
            if (DragDropSelection && !DragDropOverSelection(point))
                DoDragDropSelection();
            else if (DragDropSelection)
                EndDragDropSelection();

            if (selectionrenderer.IsSelecting)
            {
                this.Focus(FocusState.Programmatic);
                selectionrenderer.HasSelection = true;
            }

            selectionrenderer.IsSelecting = false;
        }
        private void PointerMovedAction(Point point)
        {
            if (selectionrenderer.IsSelecting)
            {
                double canvasWidth = Math.Round(this.ActualWidth, 2);
                double canvasHeight = Math.Round(this.ActualHeight, 2);
                double curPosX = Math.Round(point.X, 2);
                double curPosY = Math.Round(point.Y, 2);

                if (curPosY > canvasHeight - 50)
                {
                    VerticalScrollbar.Value += (curPosY > canvasHeight + 30 ? 20 : (canvasHeight - curPosY) / 180);
                    UpdateWhenScrolled();
                }
                else if (curPosY < 50)
                {
                    VerticalScrollbar.Value += curPosY < -30 ? -20 : -(50 - curPosY) / 20;
                    UpdateWhenScrolled();
                }

                //Horizontal
                if (curPosX > canvasWidth - 100)
                {
                    ScrollIntoViewHorizontal();
                    canvasHelper.UpdateAll();
                }
                else if (curPosX < 100)
                {
                    ScrollIntoViewHorizontal();
                    canvasHelper.UpdateAll();
                }
            }

            //Drag drop text -> move the cursor to get the insertion point
            if (DragDropSelection)
            {
                DragDropOverSelection(point);
                UpdateCursorVariable(point);
                canvasHelper.UpdateCursor();
            }
            if (selectionrenderer.IsSelecting && !DragDropSelection)
            {
                //selection started over the linenumbers:
                if (selectionrenderer.IsSelectingOverLinenumbers)
                {
                    Point pointerPos = point;
                    pointerPos.Y += SingleLineHeight; //add one more line

                    //When the selection reaches the end of the textbox select the last line completely
                    if (CursorPosition.LineNumber == textManager.LinesCount - 1)
                    {
                        pointerPos.Y -= SingleLineHeight; //add one more line
                        pointerPos.X = Utils.MeasureLineLenght(CanvasDevice.GetSharedDevice(), textManager.totalLines.GetLineText(-1), TextFormat).Width + 10;
                    }
                    UpdateCursorVariable(pointerPos);
                }
                else //Default selection
                    UpdateCursorVariable(point);

                //Update:
                canvasHelper.UpdateCursor();
                selectionrenderer.SelectionEndPosition = new CursorPosition(CursorPosition.CharacterPosition, CursorPosition.LineNumber);
                canvasHelper.UpdateSelection();
            }
        }
        private bool CheckTouchInput(PointerPoint point)
        {
            if (point.PointerDeviceType == PointerDeviceType.Touch || point.PointerDeviceType == PointerDeviceType.Pen)
            {
                //Get the touch start position:
                if (!OldTouchPosition.HasValue)
                    return true;

                //GEt the dragged offset:
                double scrollX = OldTouchPosition.Value.X - point.Position.X;
                double scrollY = OldTouchPosition.Value.Y - point.Position.Y;
                VerticalScrollbar.Value += scrollY > 2 ? 2 : scrollY < -2 ? -2 : scrollY;
                HorizontalScrollbar.Value += scrollX > 2 ? 2 : scrollX < -2 ? -2 : scrollX;
                canvasHelper.UpdateAll();
                return true;
            }
            return false;
        }
        private bool CheckTouchInput_Click(PointerPoint point)
        {
            if (point.PointerDeviceType == PointerDeviceType.Touch || point.PointerDeviceType == PointerDeviceType.Pen)
            {
                OldTouchPosition = point.Position;
                return true;
            }
            return false;
        }


        #endregion

        #region Events
        //Handle keyinputs
        private void InputHandler_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsReadonly || inputHandler.Text == "\t")
                return;

            //Prevent key-entering if control key is pressed 
            var ctrl = Utils.IsKeyPressed(VirtualKey.Control);
            var menu = Utils.IsKeyPressed(VirtualKey.Menu);
            if (ctrl && !menu || menu && !ctrl)
                return;

            AddCharacter(inputHandler.Text);
            inputHandler.Text = "";
        }
        private void InputHandler_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Tab)
            {
                TextSelection selection;
                if (Utils.IsKeyPressed(VirtualKey.Shift))
                    selection = TabKey.MoveTabBack(textManager.totalLines, TextSelection, CursorPosition, tabSpaceHelper.TabCharacter, NewLineCharacter, undoRedo);
                else
                    selection = TabKey.MoveTab(textManager.totalLines, TextSelection, CursorPosition, tabSpaceHelper.TabCharacter, NewLineCharacter, undoRedo);

                if (selection != null)
                {
                    if (selection.EndPosition == null)
                    {
                        CursorPosition = selection.StartPosition;
                    }
                    else
                    {
                        selectionrenderer.SetSelection(selection);
                        CursorPosition = selection.EndPosition;
                    }
                }
                canvasHelper.UpdateAll();

                //mark as handled to not change focus
                e.Handled = true;
            }
            if (!HasFocus)
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
                        SelectionManager.SelectSingleWord(canvasHelper, selectionrenderer, CursorPosition, textManager.CurrentLine);
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
                        textManager.totalLines,
                        TextSelection,
                        CursorPosition,
                        undoRedo,
                        NewLineCharacter,
                        e.Key == VirtualKey.Down ? LineMoveDirection.Down : LineMoveDirection.Up
                        );

                    if (e.Key == VirtualKey.Down)
                        ScrollOneLineDown();
                    else if (e.Key == VirtualKey.Up)
                        ScrollOneLineUp();

                    ForceClearSelection();
                    canvasHelper.UpdateAll();
                    return;
                }
            }

            switch (e.Key)
            {
                case VirtualKey.Enter:
                    AddNewLine();
                    break;
                case VirtualKey.Back:
                    RemoveText(ctrl);
                    break;
                case VirtualKey.Delete:
                    DeleteText(ctrl, shift);
                    break;
                case VirtualKey.Left:
                    {
                        if (shift)
                        {
                            StartSelectionIfNeeded();
                            CursorManager.MoveLeft(CursorPosition, textManager.totalLines, textManager.CurrentLine);
                            selectionrenderer.SetSelectionEnd(CursorPosition);
                        }
                        else
                        {
                            //Move the cursor to the start of the selection
                            if (selectionrenderer.HasSelection && TextSelection != null)
                                CursorPosition = SelectionManager.GetMin(TextSelection);
                            else
                                CursorManager.MoveLeft(CursorPosition, textManager.totalLines, textManager.CurrentLine);

                            SelectionManager.ClearSelectionIfNeeded(this, selectionrenderer);
                        }

                        UpdateScrollToShowCursor();
                        canvasHelper.UpdateText();
                        canvasHelper.UpdateCursor();
                        canvasHelper.UpdateSelection();
                        break;
                    }
                case VirtualKey.Right:
                    {
                        if (shift)
                        {
                            StartSelectionIfNeeded();
                            CursorManager.MoveRight(CursorPosition, textManager.totalLines, textManager.CurrentLine);
                            selectionrenderer.SetSelectionEnd(CursorPosition);
                        }
                        else
                        {
                            //Move the cursor to the end of the selection
                            if (selectionrenderer.HasSelection && TextSelection != null)
                                CursorPosition = SelectionManager.GetMax(TextSelection);
                            else
                                CursorManager.MoveRight(CursorPosition, textManager.totalLines, textManager.totalLines.GetCurrentLineText());

                            SelectionManager.ClearSelectionIfNeeded(this, selectionrenderer);
                        }

                        UpdateScrollToShowCursor(false);
                        canvasHelper.UpdateAll();
                        break;
                    }
                case VirtualKey.Down:
                    {
                        if (shift)
                        {
                            StartSelectionIfNeeded();
                            selectionrenderer.IsSelecting = true;
                            CursorPosition = selectionrenderer.SelectionEndPosition = CursorManager.MoveDown(selectionrenderer.SelectionEndPosition, textManager.LinesCount);
                            selectionrenderer.IsSelecting = false;
                        }
                        else
                        {
                            SelectionManager.ClearSelectionIfNeeded(this, selectionrenderer);
                            CursorPosition = CursorManager.MoveDown(CursorPosition, textManager.LinesCount);
                        }

                        UpdateScrollToShowCursor(false);
                        canvasHelper.UpdateAll();
                        break;
                    }
                case VirtualKey.Up:
                    {
                        if (shift)
                        {
                            StartSelectionIfNeeded();
                            selectionrenderer.IsSelecting = true;
                            CursorPosition = selectionrenderer.SelectionEndPosition = CursorManager.MoveUp(selectionrenderer.SelectionEndPosition);
                            selectionrenderer.IsSelecting = false;
                        }
                        else
                        {
                            SelectionManager.ClearSelectionIfNeeded(this, selectionrenderer);
                            CursorPosition = CursorManager.MoveUp(CursorPosition);
                        }

                        UpdateScrollToShowCursor(false);
                        canvasHelper.UpdateAll();
                        break;
                    }
                case VirtualKey.Escape:
                    {
                        EndDragDropSelection();
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
                            selectionrenderer.HasSelection = true;

                            if (selectionrenderer.SelectionStartPosition == null)
                                selectionrenderer.SelectionStartPosition = new CursorPosition(CursorPosition);

                            CursorManager.MoveToLineEnd(CursorPosition, textManager.CurrentLine);
                            selectionrenderer.SelectionEndPosition = CursorPosition;
                            canvasHelper.UpdateSelection();
                            canvasHelper.UpdateCursor();
                        }
                        else
                        {
                            CursorManager.MoveToLineEnd(CursorPosition, textManager.CurrentLine);
                            canvasHelper.UpdateCursor();
                            canvasHelper.UpdateText();
                        }
                        break;
                    }
                case VirtualKey.Home:
                    {
                        if (shift)
                        {
                            selectionrenderer.HasSelection = true;

                            if (selectionrenderer.SelectionStartPosition == null)
                                selectionrenderer.SelectionStartPosition = new CursorPosition(CursorPosition);
                            CursorManager.MoveToLineStart(CursorPosition);
                            selectionrenderer.SelectionEndPosition = CursorPosition;

                            canvasHelper.UpdateSelection();
                            canvasHelper.UpdateCursor();
                        }
                        else
                        {
                            CursorManager.MoveToLineStart(CursorPosition);
                            canvasHelper.UpdateCursor();
                            canvasHelper.UpdateText();
                        }
                        break;
                    }
            }
        }
        //Pointer-events:

        //Need both the Canvas event and the CoreWindow event, because:
        //AppWindows does not handle CoreWindow events
        //Without coreWindow the selection outside of the window would not work

        private void Canvas_Selection_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Canvas_Selection.ReleasePointerCapture(e.Pointer);

            PointerReleasedAction(e.GetCurrentPoint(Canvas_Selection).Position);
        }
        private void Canvas_Selection_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!HasFocus)
                return;

            var point = e.GetCurrentPoint(Canvas_Selection);

            if (CheckTouchInput(point))
                return;

            if (point.Properties.IsLeftButtonPressed)
            {
                selectionrenderer.IsSelecting = true;
            }
            PointerMovedAction(point.Position);

        }
        private void Canvas_Selection_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Canvas_Selection.CapturePointer(e.Pointer);
            selectionrenderer.IsSelectingOverLinenumbers = false;

            var point = e.GetCurrentPoint(Canvas_Selection);
            if (CheckTouchInput_Click(point))
                return;

            Point pointerPosition = point.Position;
            bool leftButtonPressed = point.Properties.IsLeftButtonPressed;
            bool ightButtonPressed = point.Properties.IsRightButtonPressed;

            if (leftButtonPressed && !Utils.IsKeyPressed(VirtualKey.Shift))
                PointerClickCount++;

            if (PointerClickCount == 3)
            {
                SelectLine(CursorPosition.LineNumber);
                PointerClickCount = 0;
                return;
            }
            else if (PointerClickCount == 2)
            {
                UpdateCursorVariable(pointerPosition);
                SelectionManager.SelectSingleWord(canvasHelper, selectionrenderer, CursorPosition, CurrentLine);
            }
            else
            {
                //TODO: Show the on screen keyboard if no physical keyboard is attached

                //Show the contextflyout
                if (ightButtonPressed)
                {
                    if (!selectionrenderer.PointerIsOverSelection(pointerPosition, TextSelection, DrawnTextLayout))
                    {
                        ForceClearSelection();
                        UpdateCursorVariable(pointerPosition);
                    }

                    if (!ContextFlyoutDisabled && ContextFlyout != null)
                    {
                        ContextFlyout.ShowAt(sender as FrameworkElement, new FlyoutShowOptions { Position = pointerPosition });
                    }
                }

                //Shift + click = set selection
                if (Utils.IsKeyPressed(VirtualKey.Shift) && leftButtonPressed)
                {
                    if (selectionrenderer.SelectionStartPosition == null)
                        selectionrenderer.SetSelectionStart(new CursorPosition(CursorPosition));

                    UpdateCursorVariable(pointerPosition);

                    selectionrenderer.SetSelectionEnd(new CursorPosition(CursorPosition));
                    canvasHelper.UpdateSelection();
                    canvasHelper.UpdateCursor();
                    return;
                }

                if (leftButtonPressed)
                {
                    UpdateCursorVariable(pointerPosition);

                    //Text drag/drop
                    if (TextSelection != null)
                    {
                        if (selectionrenderer.PointerIsOverSelection(pointerPosition, TextSelection, DrawnTextLayout) && !DragDropSelection)
                        {
                            PointerClickCount = 0;
                            DragDropSelection = true;

                            return;
                        }
                        //End the selection by pressing on it
                        if (DragDropSelection && DragDropOverSelection(pointerPosition))
                        {
                            EndDragDropSelection(true);
                        }
                    }

                    //Clear the selection when pressing anywhere
                    if (selectionrenderer.HasSelection)
                    {
                        ForceClearSelection();
                        selectionrenderer.SelectionStartPosition = new CursorPosition(CursorPosition);
                    }
                    else
                    {
                        selectionrenderer.SetSelectionStart(new CursorPosition(CursorPosition));
                        selectionrenderer.IsSelecting = true;
                    }
                }
                canvasHelper.UpdateCursor();
            }

            PointerClickTimer.Start();
            PointerClickTimer.Tick += (s, t) =>
            {
                PointerClickTimer.Stop();
                PointerClickCount = 0;
            };
        }
        private void Canvas_Selection_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var delta = e.GetCurrentPoint(Canvas_Selection).Properties.MouseWheelDelta;
            bool needsUpdate = false;
            //Zoom using mousewheel
            if (Utils.IsKeyPressed(VirtualKey.Control))
            {
                _ZoomFactor += delta / 20;
                UpdateZoom();
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
                VerticalScrollbar.Value -= (delta * VerticalScrollSensitivity) / DefaultVerticalScrollSensitivity;
                //Only update when a line was scrolled
                if ((int)(VerticalScrollbar.Value / SingleLineHeight * DefaultVerticalScrollSensitivity) != NumberOfStartLine)
                {
                    needsUpdate = true;
                }
            }

            if (selectionrenderer.IsSelecting)
            {
                UpdateCursorVariable(e.GetCurrentPoint(Canvas_Selection).Position);
                canvasHelper.UpdateCursor();

                selectionrenderer.SelectionEndPosition = new CursorPosition(CursorPosition.CharacterPosition, CursorPosition.LineNumber);
                needsUpdate = true;
            }
            if (needsUpdate)
                canvasHelper.UpdateAll();
        }
        private void Canvas_LineNumber_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Canvas_Selection.CapturePointer(e.Pointer);

            var point = e.GetCurrentPoint(Canvas_Selection);
            if (CheckTouchInput_Click(point))
                return;

            //Select the line where the cursor is over
            SelectLine(CursorRenderer.GetCursorLineFromPoint(point.Position, SingleLineHeight, NumberOfRenderedLines, NumberOfStartLine));

            selectionrenderer.IsSelecting = true;
            selectionrenderer.IsSelectingOverLinenumbers = true;
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
            //Create resources and layouts:
            if (NeedsTextFormatUpdate || TextFormat == null || LineNumberTextFormat == null)
            {
                if (_ShowLineNumbers)
                    LineNumberTextFormat = TextLayoutHelper.CreateLinenumberTextFormat(ZoomedFontSize, FontFamily);
                TextFormat = TextLayoutHelper.CreateCanvasTextFormat(ZoomedFontSize, FontFamily);
            }

            CreateColorResources(args.DrawingSession);

            //Measure textposition and apply the value to the scrollbar
            VerticalScrollbar.Maximum = ((textManager.LinesCount + 1) * SingleLineHeight - Scroll.ActualHeight) / DefaultVerticalScrollSensitivity;
            VerticalScrollbar.ViewportSize = sender.ActualHeight;

            //Calculate number of lines that needs to be rendered
            textManager.NumberOfRenderedLines = (int)(sender.ActualHeight / SingleLineHeight);
            NumberOfStartLine = (int)((VerticalScrollbar.Value * DefaultVerticalScrollSensitivity) / SingleLineHeight);

            //Get all the lines, that need to be rendered, from the list
            textManager.NumberOfRenderedLines = textManager.NumberOfRenderedLines + NumberOfStartLine > textManager.LinesCount ? textManager.LinesCount : textManager.NumberOfRenderedLines;

            textManager.RenderedLines = textManager.totalLines.GetLines(NumberOfStartLine, textManager.NumberOfRenderedLines);
            RenderedText = textManager.RenderedLines.GetString("\n");

            if (_ShowLineNumbers)
            {
                lineNumberRenderer.GenerateLineNumberText(textManager.NumberOfRenderedLines, NumberOfStartLine);
            }

            //Get the longest line in the text:
            if (NeedsRecalculateLongestLineIndex)
            {
                NeedsRecalculateLongestLineIndex = false;
                LongestLineIndex = Utils.GetLongestLineIndex(textManager.totalLines);
            }

            string longestLineText = textManager.totalLines.GetLineText(LongestLineIndex);
            LongestLineLength = longestLineText.Length;
            Size lineLength = Utils.MeasureLineLenght(CanvasDevice.GetSharedDevice(), longestLineText, TextFormat);

            //Measure horizontal Width of longest line and apply to scrollbar
            HorizontalScrollbar.Maximum = (lineLength.Width <= sender.ActualWidth ? 0 : lineLength.Width - sender.ActualWidth + 50);
            HorizontalScrollbar.ViewportSize = sender.ActualWidth;
            ScrollIntoViewHorizontal();

            //Only update the textformat when the text changes:
            if (OldRenderedText != RenderedText || NeedsUpdateTextLayout)
            {
                NeedsUpdateTextLayout = false;
                OldRenderedText = RenderedText;

                DrawnTextLayout = TextLayoutHelper.CreateTextResource(sender, DrawnTextLayout, TextFormat, RenderedText, new Size { Height = sender.Size.Height, Width = this.ActualWidth });
                SyntaxHighlightingRenderer.UpdateSyntaxHighlighting(DrawnTextLayout, _AppTheme, _CodeLanguage, SyntaxHighlighting, RenderedText);
            }

            //render the search highlights
            if (SearchIsOpen)
                SearchHighlightsRenderer.RenderHighlights(args, DrawnTextLayout, RenderedText, searchManager.MatchingSearchLines, searchManager.SearchParameter.SearchExpression, (float)-HorizontalScroll, SingleLineHeight / DefaultVerticalScrollSensitivity, _Design.SearchHighlightColor);

            args.DrawingSession.DrawTextLayout(DrawnTextLayout, (float)-HorizontalScroll, SingleLineHeight, TextColorBrush);

            //Only update if needed, to reduce updates when scrolling
            if (lineNumberRenderer.CanUpdateCanvas())
            {
                Canvas_LineNumber.Invalidate();
            }
        }
        private void Canvas_Selection_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (selectionrenderer.SelectionStartPosition != null && selectionrenderer.SelectionEndPosition != null)
            {
                selectionrenderer.HasSelection =
                    !(selectionrenderer.SelectionStartPosition.LineNumber == selectionrenderer.SelectionEndPosition.LineNumber &&
                    selectionrenderer.SelectionStartPosition.CharacterPosition == selectionrenderer.SelectionEndPosition.CharacterPosition);
            }
            else
                selectionrenderer.HasSelection = false;

            if (selectionrenderer.HasSelection)
            {
                TextSelection = selectionrenderer.DrawSelection(DrawnTextLayout, textManager.RenderedLines, args, (float)-HorizontalScroll, SingleLineHeight / DefaultVerticalScrollSensitivity, NumberOfStartLine, NumberOfRenderedLines, ZoomedFontSize, _Design.SelectionColor);
            }

            if (TextSelection != null && !SelectionManager.Equals(OldTextSelection, TextSelection))
            {
                //Update the variables
                OldTextSelection = new TextSelection(TextSelection);
                Internal_CursorChanged();
            }
        }
        private void Canvas_Cursor_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            currentLineManager.UpdateCurrentLine();
            if (DrawnTextLayout == null || !HasFocus)
                return;

            int currentLineLength = textManager.CurrentLine.Length;
            if (CursorPosition.LineNumber >= textManager.LinesCount)
            {
                CursorPosition.LineNumber = textManager.LinesCount - 1;
                CursorPosition.CharacterPosition = currentLineLength;
            }

            //Calculate the distance to the top for the cursorposition and render the cursor
            float renderPosY = (float)((CursorPosition.LineNumber - NumberOfStartLine) * SingleLineHeight) + SingleLineHeight / DefaultVerticalScrollSensitivity;

            //Out of display-region:
            if (renderPosY > textManager.NumberOfRenderedLines * SingleLineHeight || renderPosY < 0)
                return;

            UpdateCurrentLineTextLayout();

            int characterPos = CursorPosition.CharacterPosition;
            if (characterPos > currentLineLength)
                characterPos = currentLineLength;

            CursorRenderer.RenderCursor(
                CurrentLineTextLayout,
                characterPos,
                (float)-HorizontalScroll,
                renderPosY, ZoomedFontSize,
                CursorSize,
                args,
                CursorColorBrush);


            if (_ShowLineHighlighter && SelectionManager.SelectionIsNull(selectionrenderer, TextSelection))
                LineHighlighterRenderer.Render((float)sender.ActualWidth, CurrentLineTextLayout, renderPosY, ZoomedFontSize, args, LineHighlighterBrush);

            if (!CursorManager.Equals(CursorPosition, OldCursorPosition))
            {
                OldCursorPosition = new CursorPosition(CursorPosition);
                Internal_CursorChanged();
            }
        }
        private void Canvas_LineNumber_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (!lineNumberManager._ShowLineNumbers)
            {
                lineNumberRenderer.HideLineNumbers(sender, SpaceBetweenLineNumberAndText);
                return;
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
        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            SetFocus();
        }
        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            RemoveFocus();
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
            if (IsReadonly)
                return;

            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                textActionManager.AddCharacter(stringManager.CleanUpString(await e.DataView.GetTextAsync()), true);
            }
        }
        private void UserControl_DragOver(object sender, DragEventArgs e)
        {
            if (selectionrenderer.IsSelecting || IsReadonly || !e.DataView.Contains(StandardDataFormats.Text))
                return;

            var deferral = e.GetDeferral();

            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.IsGlyphVisible = false;
            e.DragUIOverride.IsContentVisible = false;
            deferral.Complete();

            UpdateCursorVariable(e.GetPosition(Canvas_Text));
            canvasHelper.UpdateCursor();
        }
        #endregion

        #region Public functions and properties

        /// <summary>
        /// Selects the entire line specified by its index.
        /// </summary>
        /// <param name="line">The index of the line to select.</param>
        public void SelectLine(int line)
        {
            selectionrenderer.SetSelection(new CursorPosition(0, line), new CursorPosition(textManager.GetLineLength(line), line));
            CursorPosition = selectionrenderer.SelectionEndPosition;

            canvasHelper.UpdateSelection();
            canvasHelper.UpdateCursor();
        }

        /// <summary>
        /// Moves the cursor to the beginning of the specified line by its index.
        /// </summary>
        /// <param name="line">The index of the line to navigate to.</param>
        public void GoToLine(int line)
        {
            if (line >= textManager.LinesCount || line < 0)
                return;

            selectionrenderer.SelectionEndPosition = null;
            CursorPosition = selectionrenderer.SelectionStartPosition = new CursorPosition(0, line);

            ScrollLineIntoView(line);
            this.Focus(FocusState.Programmatic);

            canvasHelper.UpdateAll();
        }

        /// <summary>
        /// Loads the specified text into the textbox, resetting all text and undo history.
        /// </summary>
        /// <param name="text">The text to load into the textbox.</param>
        public void LoadText(string text)
        {
            textActionManager.Safe_LoadText(text);
        }

        /// <summary>
        /// Sets the text content of the textbox, recording an undo action.
        /// </summary>
        /// <param name="text">The new text content to set in the textbox.</param>
        public void SetText(string text)
        {
            textActionManager.Safe_SetText(text);
        }

        /// <summary>
        /// Loads the specified lines into the textbox, resetting all content and undo history.
        /// </summary>
        /// <param name="lines">An enumerable containing the lines to load into the textbox.</param>
        /// <param name="lineEnding">The line ending format used in the loaded lines (default is CRLF).</param>
        public void LoadLines(IEnumerable<string> lines, LineEnding lineEnding = LineEnding.CRLF)
        {
            textActionManager.Safe_LoadLines(lines, lineEnding);
        }

        /// <summary>
        /// Pastes the contents of the clipboard at the current cursor position.
        /// </summary>
        public void Paste()
        {
            textActionManager.Safe_Paste();
        }

        /// <summary>
        /// Copies the currently selected text to the clipboard.
        /// </summary>
        public void Copy()
        {
            textActionManager.Safe_Copy();
        }

        /// <summary>
        /// Cuts the currently selected text and copies it to the clipboard.
        /// </summary>
        public void Cut()
        {
            textActionManager.Safe_Cut();
        }

        /// <summary>
        /// Gets the entire text content of the textbox.
        /// </summary>
        /// <returns>The complete text content of the textbox as a string.</returns>

        public string GetText()
        {
            return textActionManager.Safe_Gettext();
        }

        /// <summary>
        /// Sets the text selection in the textbox starting from the specified index and with the given length.
        /// </summary>
        /// <param name="start">The index of the first character of the selection.</param>
        /// <param name="length">The length of the selection in number of characters.</param>
        public void SetSelection(int start, int length)
        {
            var result = selectionManager.GetSelectionFromPosition(start, length, CharacterCount);
            if (result != null)
            {
                selectionrenderer.SetSelection(result.StartPosition, result.EndPosition);
                if (result.EndPosition != null)
                    CursorPosition = result.EndPosition;
            }

            canvasHelper.UpdateSelection();
            canvasHelper.UpdateCursor();
        }

        /// <summary>
        /// Selects all the text in the textbox.
        /// </summary>
        public void SelectAll()
        {
            textActionManager.SelectAll();
        }

        /// <summary>
        /// Clears the current text selection in the textbox.
        /// </summary>
        public void ClearSelection()
        {
            selectionManager.ForceClearSelection(selectionrenderer, canvasHelper);
        }

        /// <summary>
        /// Undoes the last action in the textbox.
        /// </summary>
        public void Undo()
        {
            textActionManager.Undo();
        }

        /// <summary>
        /// Redoes the last undone action in the textbox.
        /// </summary>
        public void Redo()
        {
            textActionManager.Redo();
        }

        /// <summary>
        /// Scrolls the specified line to the center of the textbox if it is out of the rendered region.
        /// </summary>
        /// <param name="line">The index of the line to center.</param>
        public void ScrollLineToCenter(int line)
        {
            scrollManager.ScrollLineToCenter(line);
        }

        /// <summary>
        /// Scrolls the text one line up.
        /// </summary>
        public void ScrollOneLineUp()
        {
            scrollManager.ScrollOneLineUp();

        }

        /// <summary>
        /// Scrolls the text one line down.
        /// </summary>
        public void ScrollOneLineDown()
        {
            scrollManager.ScrollOneLineDown();
        }

        /// <summary>
        /// Forces the specified line to be scrolled into view, centering it vertically within the textbox.
        /// </summary>
        /// <param name="line">The index of the line to center.</param>
        public void ScrollLineIntoView(int line)
        {
            scrollManager.ScrollLineIntoView(line);
        }

        /// <summary>
        /// Scrolls the first line of the visible text into view.
        /// </summary>
        public void ScrollTopIntoView()
        {
            scrollManager.ScrollTopIntoView();
        }

        /// <summary>
        /// Scrolls the last visible line of the visible text into view.
        /// </summary>
        public void ScrollBottomIntoView()
        {
            scrollManager.ScrollBottomIntoView();
        }

        /// <summary>
        /// Scrolls one page up, simulating the behavior of the page up key.
        /// </summary>
        public void ScrollPageUp()
        {
            scrollManager.ScrollPageUp();
        }

        /// <summary>
        /// Scrolls one page down, simulating the behavior of the page down key.
        /// </summary>
        public void ScrollPageDown()
        {
            scrollManager.ScrollPageDown();
        }

        /// <summary>
        /// Gets the content of the line specified by the index
        /// </summary>
        /// <param name="line">The index to get the content from</param>
        /// <returns>The text from the line specified by the index</returns>
        public string GetLineText(int line)
        {
            return textManager.GetLineText(line);
        }

        /// <summary>
        /// Gets the text of multiple lines, starting from the specified line index.
        /// </summary>
        /// <param name="startLine">The index of the line to start with.</param>
        /// <param name="length">The number of lines to retrieve.</param>
        /// <returns>The concatenated text from the specified lines.</returns>
        public string GetLinesText(int startLine, int length)
        {
            if (startLine + length >= textManager.LinesCount)
                return textManager.GetString();

            return textManager.GetLines(startLine, length).GetString(textManager.NewLineCharacter);
        }

        /// <summary>
        /// Sets the content of the line specified by the index. The first line has the index 0.
        /// </summary>
        /// <param name="line">The index of the line to change the content.</param>
        /// <param name="text">The text to set for the specified line.</param>
        /// <returns>Returns true if the text was changed successfully, and false if the index was out of range.</returns>
        public bool SetLineText(int line, string text)
        {
            return textActionManager.SetLineText(line, text);
        }

        /// <summary>
        /// Deletes the line from the textbox
        /// </summary>
        /// <param name="line">The line to delete</param>
        /// <returns>Returns true if the line was deleted successfully and false if not</returns>
        public bool DeleteLine(int line)
        {
            return textActionManager.DeleteLine(line);
        }

        /// <summary>
        /// Adds a new line with the text specified
        /// </summary>
        /// <param name="line">The position to insert the line to</param>
        /// <param name="text">The text to put in the new line</param>
        /// <returns>Returns true if the line was added successfully and false if not</returns>
        public bool AddLine(int line, string text)
        {
            return textActionManager.AddLine(line, text);
        }

        /// <summary>
        /// Surrounds the selection with the text specified by the text
        /// </summary>
        /// <param name="text">The text to surround the selection with</param>
        public void SurroundSelectionWith(string text)
        {
            text = stringManager.CleanUpString(text);
            SurroundSelectionWith(text, text);
        }

        /// <summary>
        /// Surround the selection with individual text for the left and right side.
        /// </summary>
        /// <param name="text1">The text for the left side</param>
        /// <param name="text2">The text for the right side</param>
        public void SurroundSelectionWith(string text1, string text2)
        {
            if (!selectionManager.SelectionIsNull(selectionrenderer, TextSelection))
            {
                textActionManager.AddCharacter(stringManager.CleanUpString(text1) + SelectedText + stringManager.CleanUpString(text2));
            }
        }

        /// <summary>
        /// Duplicates the line specified by the index into the next line
        /// </summary>
        /// <param name="line">The index of the line to duplicate</param>
        public void DuplicateLine(int line)
        {
            textActionManager.DuplicateLine(line);
        }
        /// <summary>
        /// Duplicates the line at the current cursor position
        /// </summary>
        public void DuplicateLine()
        {
            textActionManager.DuplicateLine(CursorPosition.LineNumber);
        }

        /// <summary>
        /// Replaces all occurences in the text with another word
        /// </summary>
        /// <param name="word">The word to search for</param>
        /// <param name="replaceWord">The word to replace with</param>
        /// <param name="matchCase">Search with case sensitivity</param>
        /// <param name="wholeWord">Search for whole words</param>
        /// <returns>Found when everything was replaced and not found when nothing was replaced</returns>
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
            canvasHelper.UpdateText();
            return isFound ? SearchResult.Found : SearchResult.NotFound;
        }

        /// <summary>
        /// Searches for the next occurence. Call this after BeginSearch
        /// </summary>
        /// <returns>SearchResult.Found if the word was found</returns>
        public SearchResult FindNext()
        {
            if (!searchManager.IsSearchOpen)
                return SearchResult.SearchNotOpened;

            var res = searchManager.FindNext(CursorPosition);
            if (res.Selection != null)
            {
                selectionrenderer.SetSelection(res.Selection);
                ScrollLineIntoView(CursorPosition.LineNumber);
                canvasHelper.UpdateText();
                canvasHelper.UpdateSelection();
            }
            return res.Result;
        }

        /// <summary>
        /// Searches for the previous occurence. Call this after BeginSearch
        /// </summary>
        /// <returns>SearchResult.Found if the word was found</returns>
        public SearchResult FindPrevious()
        {
            if (!searchManager.IsSearchOpen)
                return SearchResult.SearchNotOpened;

            var res = searchManager.FindPrevious(CursorPosition);
            if (res.Selection != null)
            {
                selectionrenderer.SetSelection(res.Selection);
                ScrollLineIntoView(CursorPosition.LineNumber);
                canvasHelper.UpdateText();
                canvasHelper.UpdateSelection();
            }
            return res.Result;
        }

        /// <summary>
        /// Begins a search for the specified word in the textbox content.
        /// </summary>
        /// <param name="word">The word to search for in the textbox.</param>
        /// <param name="wholeWord">A flag indicating whether to perform a whole-word search.</param>
        /// <param name="matchCase">A flag indicating whether the search should be case-sensitive.</param>
        /// <returns>A SearchResult enum representing the result of the search.</returns>
        public SearchResult BeginSearch(string word, bool wholeWord, bool matchCase)
        {
            var res = searchManager.BeginSearch(word, wholeWord, matchCase);
            canvasHelper.UpdateText();
            return res;
        }

        /// <summary>
        /// Ends the search and removes the highlights
        /// </summary>
        public void EndSearch()
        {
            searchManager.EndSearch();
            canvasHelper.UpdateText();
        }

        /// <summary>
        /// Unloads the textbox and releases all resources.
        /// Do not use the textbox afterwards.
        /// </summary>
        public void Unload()
        {
            //Unsubscribe from events:
            inputHandler.PreviewKeyDown -= InputHandler_KeyDown;
            inputHandler.TextChanged -= InputHandler_TextChanged;

            //Dispose and null larger objects
            textManager.totalLines.Dispose();
            textRenderer.RenderedLines = null;
            lineNumberRenderer.LineNumberTextToRender = lineNumberRenderer.OldLineNumberTextToRender = null;
            undoRedo.NullAll();
        }

        /// <summary>
        /// Clears the undo and redo history of the textbox.
        /// </summary>
        /// <remarks>
        /// The ClearUndoRedoHistory method removes all the stored undo and redo actions, effectively resetting the history of the textbox.
        /// </remarks>
        public void ClearUndoRedoHistory()
        {
            undoRedo.ClearAll();
        }

        /// <summary>
        /// Gets the current cursor position in the textbox.
        /// </summary>
        /// <returns>The current cursor position represented by a Point object (X, Y).</returns>
        public Point GetCursorPosition()
        {
            return new Point
            {
                Y = (float)((CursorPosition.LineNumber - textRenderer.NumberOfStartLine) * textRenderer.SingleLineHeight) + textRenderer.SingleLineHeight / scrollManager.DefaultVerticalScrollSensitivity,
                X = CursorHelper.GetCursorPositionInLine(textRenderer.CurrentLineTextLayout, CursorPosition, 0)
            };
        }

        /// <summary>
        /// Selects the CodeLanguage based on the specified identifier.
        /// </summary>
        /// <param name="languageId">The identifier of the CodeLanguage to select.</param>
        public void SelectCodeLanguage(CodeLanguageId languageId)
        {
            if (CodeLanguages.TryGetValue(languageId, out SyntaxHighlightLanguage codelanguage))
                HighlightLanguage = codelanguage;
        }

        /// <summary>
        /// Gets or sets a value indicating whether syntax highlighting is enabled in the textbox.
        /// </summary>
        public bool SyntaxHighlighting { get; set; } = true;

        /// <summary>
        /// Gets or sets the code language to use for the syntaxhighlighting and autopairing.
        /// </summary>
        public SyntaxHighlightLanguage HighlightLanguage
        {
            get => textManager._CodeLanguage;
            set
            {
                textManager._CodeLanguage = value;
                textRenderer.NeedsUpdateTextLayout = true; //set to true to force update the textlayout
                canvasHelper.UpdateText();
            }
        }

        /// <summary>
        /// Gets or sets the line ending style used in the textbox.
        /// </summary>
        /// <remarks>
        /// The LineEnding property represents the line ending style for the text.
        /// Possible values are LineEnding.CRLF (Carriage Return + Line Feed), LineEnding.LF (Line Feed), or LineEnding.CR (Carriage Return).
        /// </remarks>
        public LineEnding LineEnding
        {
            get => textManager._LineEnding;
            set
            {
                stringManager.lineEnding = value;
                textManager._LineEnding = value;
            }
        }

        /// <summary>
        /// Gets or sets the space between the line number and the text in the textbox.
        /// </summary>
        public float SpaceBetweenLineNumberAndText { get => lineNumberManager._SpaceBetweenLineNumberAndText; set { lineNumberManager._SpaceBetweenLineNumberAndText = value; lineNumberRenderer.NeedsUpdateLineNumbers(); canvasHelper.UpdateAll(); } }

        /// <summary>
        /// Gets or sets the current cursor position in the textbox.
        /// </summary>
        /// <remarks>
        /// The cursor position is represented by a <see cref="CursorPosition"/> object, which includes the character position within the text and the line number.
        /// </remarks>
        public CursorPosition CursorPosition
        {
            get => cursorManager.currentCursorPosition;
            set { cursorManager.SetCursorPosition(new CursorPosition(value.CharacterPosition, value.LineNumber)); canvasHelper.UpdateCursor(); }
        }

        /// <summary>
        /// Gets or sets the font family used for displaying text in the textbox.
        /// </summary>
        public new FontFamily FontFamily { get => textManager._FontFamily; set { textManager._FontFamily = value; textRenderer.NeedsTextFormatUpdate = true; canvasHelper.UpdateAll(); } }

        /// <summary>
        /// Gets or sets the font size used for displaying text in the textbox.
        /// </summary>
        public new int FontSize { get => textManager._FontSize; set { textManager._FontSize = value; zoomManager.UpdateZoom(); } }

        /// <summary>
        /// Gets the actual rendered size of the font in pixels.
        /// </summary>
        public float RenderedFontSize => zoomManager.ZoomedFontSize;

        /// <summary>
        /// Gets or sets the text displayed in the textbox.
        /// </summary>
        public string Text { get => GetText(); set { SetText(value); } }

        /// <summary>
        /// Gets or sets the requested theme for the textbox.
        /// </summary>
        public new ElementTheme RequestedTheme
        {
            get => _RequestedTheme;
            set
            {
                _RequestedTheme = value;
                _AppTheme = Utils.ConvertTheme(value);

                if (UseDefaultDesign)
                    _Design = _AppTheme == ApplicationTheme.Light ? LightDesign : DarkDesign;

                this.Background = _Design.Background;
                ColorResourcesCreated = false;
                NeedsUpdateTextLayout = true;
                canvasHelper.UpdateAll();
            }
        }

        /// <summary>
        /// Gets or sets the custom design for the textbox.
        /// </summary>
        /// <remarks>
        /// Settings this null will use the default design
        /// </remarks>
        public TextControlBoxDesign Design
        {
            get => UseDefaultDesign ? null : _Design;
            set
            {
                _Design = value != null ? value : _AppTheme == ApplicationTheme.Dark ? DarkDesign : LightDesign;
                UseDefaultDesign = value == null;

                this.Background = _Design.Background;
                ColorResourcesCreated = false;
                canvasHelper.UpdateAll();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether line numbers should be displayed in the textbox.
        /// </summary>
        public bool ShowLineNumbers
        {
            get => lineNumberManager._ShowLineNumbers;
            set
            {
                lineNumberManager._ShowLineNumbers = value;
                textRenderer.NeedsUpdateTextLayout = true;
                lineNumberRenderer.NeedsUpdateLineNumbers();
                canvasHelper.UpdateAll();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the line highlighter should be shown in the custom textbox.
        /// </summary>
        public bool ShowLineHighlighter
        {
            get => lineHighlighterManager._ShowLineHighlighter;
            set { lineHighlighterManager._ShowLineHighlighter = value; canvasHelper.UpdateCursor(); }
        }

        /// <summary>
        /// Gets or sets the zoom factor in percent for the text.
        /// </summary>
        public int ZoomFactor { get =>  zoomManager._ZoomFactor; set { zoomManager._ZoomFactor = value; zoomManager.UpdateZoom(); } } //%

        /// <summary>
        /// Gets or sets a value indicating whether the textbox is in readonly mode.
        /// </summary>
        public bool IsReadonly { get; set; } //TODO

        /// <summary>
        /// Gets or sets the size of the cursor in the textbox.
        /// </summary>
        public CursorSize CursorSize { get => cursorRenderer._CursorSize; set { cursorRenderer._CursorSize = value; canvasHelper.UpdateCursor(); } }

        /// <summary>
        /// Gets or sets the context menu flyout associated with the textbox.
        /// </summary>
        /// <remarks>
        /// Setting the value to null will show the default flyout.
        /// </remarks>
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

        /// <summary>
        /// Gets or sets a value indicating whether the context flyout is disabled for the textbox.
        /// </summary>
        public bool ContextFlyoutDisabled { get; set; }

        /// <summary>
        /// Gets or sets the starting index of the selected text in the textbox.
        /// </summary>
        public int SelectionStart { get => selectionrenderer.SelectionStart; set { SetSelection(value, SelectionLength); } }

        /// <summary>
        /// Gets or sets the length of the selected text in the textbox.
        /// </summary>
        public int SelectionLength { get => selectionrenderer.SelectionLength; set { SetSelection(SelectionStart, value); } }

        /// <summary>
        /// Gets or sets the text that is currently selected in the textbox.
        /// </summary>
        public string SelectedText
        {
            get
            {
                if (TextSelection != null && selectionManager.WholeTextSelected(TextSelection))
                    return GetText();
                return selectionManager.GetSelectedText(TextSelection, CursorPosition.LineNumber);
            }
            set => textActionManager.AddCharacter(stringManager.CleanUpString(value));
        }

        /// <summary>
        /// Gets the number of lines in the textbox.
        /// </summary>
        public int NumberOfLines { get => textManager.LinesCount; }

        /// <summary>
        /// Gets the index of the current line where the cursor is positioned in the textbox.
        /// </summary>
        public int CurrentLineIndex { get => CursorPosition.LineNumber; }

        /// <summary>
        /// Gets or sets the position of the scrollbars in the textbox.
        /// </summary>
        public ScrollBarPosition ScrollBarPosition
        {
            get => new ScrollBarPosition(HorizontalScrollbar.Value, VerticalScroll);
            set { HorizontalScrollbar.Value = value.ValueX; VerticalScroll = value.ValueY; }
        }

        /// <summary>
        /// Gets the total number of characters in the textbox.
        /// </summary>
        public int CharacterCount => textManager.CountCharacters();

        /// <summary>
        /// Gets or sets the sensitivity of vertical scrolling in the textbox.
        /// </summary>
        public double VerticalScrollSensitivity { get => scrollManager._VerticalScrollSensitivity; set => scrollManager._VerticalScrollSensitivity = value < 1 ? 1 : value; }

        /// <summary>
        /// Gets or sets the sensitivity of horizontal scrolling in the textbox.
        /// </summary>
        public double HorizontalScrollSensitivity { get => scrollManager._HorizontalScrollSensitivity; set => scrollManager._HorizontalScrollSensitivity = value < 1 ? 1 : value; }

        /// <summary>
        /// Gets or sets the vertical scroll position in the textbox.
        /// </summary>
        public double VerticalScroll { get => VerticalScrollbar.Value; set { VerticalScrollbar.Value = value < 0 ? 0 : value; canvasHelper.UpdateAll(); } }

        /// <summary>
        /// Gets or sets the horizontal scroll position in the textbox.
        /// </summary>
        public double HorizontalScroll { get => HorizontalScrollbar.Value; set { HorizontalScrollbar.Value = value < 0 ? 0 : value; canvasHelper.UpdateAll(); } }

        /// <summary>
        /// Gets or sets the corner radius for the textbox.
        /// </summary>
        public new CornerRadius CornerRadius { get => MainGrid.CornerRadius; set => MainGrid.CornerRadius = value; }

        /// <summary>
        /// Gets or sets a value indicating whether to use spaces instead of tabs for indentation in the textbox.
        /// </summary>
        public bool UseSpacesInsteadTabs { get => tabSpaceHelper.UseSpacesInsteadTabs; set { tabSpaceHelper.UseSpacesInsteadTabs = value; tabSpaceHelper.UpdateTabs(); canvasHelper.UpdateAll(); } }

        /// <summary>
        /// Gets or sets the number of spaces used for a single tab in the textbox.
        /// </summary>
        public int NumberOfSpacesForTab { get => tabSpaceHelper.NumberOfSpaces; set { tabSpaceHelper.NumberOfSpaces = value; tabSpaceHelper.UpdateNumberOfSpaces(); canvasHelper.UpdateAll(); } }

        /// <summary>
        /// Gets whether the search is currently active
        /// </summary>
        public bool SearchIsOpen => searchManager.IsSearchOpen;

        /// <summary>
        /// Gets an enumerable collection of all the lines in the textbox.
        /// </summary>
        /// <remarks>
        /// Use this property to access all the lines of text in the textbox. You can use this collection to save the lines to a file using functions like FileIO.WriteLinesAsync.
        /// Utilizing this property for saving will significantly improve RAM usage during the saving process.
        /// </remarks>
        public IEnumerable<string> Lines => textManager.totalLines;

        /// <summary>
        /// Gets or sets a value indicating whether auto-pairing is enabled.
        /// </summary>
        /// <remarks>
        /// Auto-pairing automatically pairs opening and closing symbols, such as brackets or quotation marks.
        /// </remarks>
        public bool DoAutoPairing { get; set; } = true;

        #endregion

        #region Public events

        /// <summary>
        /// Represents a delegate used for handling the text changed event in the TextControlBox.
        /// </summary>
        /// <param name="sender">The instance of the TextControlBox that raised the event.</param>
        public delegate void TextChangedEvent(TextControlBox sender);
        /// <summary>
        /// Occurs when the text is changed in the TextControlBox.
        /// </summary>
        public event TextChangedEvent TextChanged;

        /// <summary>
        /// Represents a delegate used for handling the selection changed event in the TextControlBox.
        /// </summary>
        /// <param name="sender">The instance of the TextControlBox that raised the event.</param>
        /// <param name="args">The event arguments providing information about the selection change.</param>
        public delegate void SelectionChangedEvent(TextControlBox sender, SelectionChangedEventHandler args);

        /// <summary>
        /// Occurs when the selection is changed in the TextControlBox.
        /// </summary>
        public event SelectionChangedEvent SelectionChanged;

        /// <summary>
        /// Represents a delegate used for handling the zoom changed event in the TextControlBox.
        /// </summary>
        /// <param name="sender">The instance of the TextControlBox that raised the event.</param>
        /// <param name="zoomFactor">The new zoom factor value indicating the scale of the content.</param>
        public delegate void ZoomChangedEvent(TextControlBox sender, int zoomFactor);

        /// <summary>
        /// Occurs when the zoom factor is changed in the TextControlBox.
        /// </summary>
        public event ZoomChangedEvent ZoomChanged;

        /// <summary>
        /// Represents a delegate used for handling the got focus event in the TextControlBox.
        /// </summary>
        /// <param name="sender">The instance of the TextControlBox that received focus.</param>
        public delegate void GotFocusEvent(TextControlBox sender);

        /// <summary>
        /// Occurs when the TextControlBox receives focus.
        /// </summary>
        public new event GotFocusEvent GotFocus;

        /// <summary>
        /// Represents a delegate used for handling the lost focus event in the TextControlBox.
        /// </summary>
        /// <param name="sender">The instance of the TextControlBox that lost focus.</param>
        public delegate void LostFocusEvent(TextControlBox sender);

        /// <summary>
        /// Occurs when the TextControlBox loses focus.
        /// </summary>
        public new event LostFocusEvent LostFocus;
        #endregion

        #region Static functions
        //static functions
        /// <summary>
        /// Gets a dictionary containing the CodeLanguages indexed by their respective identifiers.
        /// </summary>
        /// <remarks>
        /// The CodeLanguage dictionary provides a collection of predefined CodeLanguage objects, where each object is associated with a unique identifier (language name).
        /// The dictionary is case-insensitive, and it allows quick access to the CodeLanguage objects based on their identifier.
        /// </remarks>
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

        /// <summary>
        /// Retrieves a CodeLanguage object based on the specified identifier.
        /// </summary>
        /// <param name="languageId">The identifier of the CodeLanguage to retrieve.</param>
        /// <returns>The CodeLanguage object corresponding to the provided identifier, or null if not found.</returns>
        public static SyntaxHighlightLanguage GetCodeLanguageFromId(CodeLanguageId languageId)
        {
            if (CodeLanguages.TryGetValue(languageId, out SyntaxHighlightLanguage codelanguage))
                return codelanguage;
            return null;
        }

        /// <summary>
        /// Retrieves a CodeLanguage object from a JSON representation.
        /// </summary>
        /// <param name="Json">The JSON string representing the CodeLanguage object.</param>
        /// <returns>The deserialized CodeLanguage object obtained from the provided JSON, or null if the JSON is invalid or does not represent a valid CodeLanguage.</returns>
        public static JsonLoadResult GetCodeLanguageFromJson(string Json)
        {
            return SyntaxHighlightingRenderer.GetCodeLanguageFromJson(Json);
        }

        #endregion
    }
}