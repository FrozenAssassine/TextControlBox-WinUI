using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using TextControlBox.Extensions;
using TextControlBox.Helper;
using TextControlBox.Renderer;
using TextControlBox.Text;
using TextControlBox_WinUI.Helper;
using TextControlBox_WinUI.Models;
using Windows.Foundation;
using Windows.System;

namespace TextControlBox
{
    public sealed partial class TextControlBox : UserControl
    {
        private readonly TextboxFocusManager focusManager;
        private readonly TextManager textManager;
        private readonly TextControlBoxProperties textboxProperties;
        private readonly TextRenderer textRenderer;
        private readonly DesignHelper designHelper;
        private readonly CanvasUpdateHelper canvasUpdater;
        private readonly CurrentWorkingLine workingLine;
        private readonly CursorRenderer cursorRenderer;
        private readonly ScrollBarManager scrollBarManager;
        private readonly LongestLineCalculationHelper longestLineCalcHelper;
        private readonly TextActionsManager textActionsManager;
        private readonly UndoRedoManager undoRedoManager;
        private readonly PointerManager pointerManager;
        private readonly SelectionRenderer selectionRenderer;
        private readonly SelectionManager selectionManager;

        public TextControlBox()
        {
            this.InitializeComponent();

            //Classes
            focusManager = new TextboxFocusManager(TextInputManager);
            textManager = new TextManager();
            textboxProperties = new TextControlBoxProperties();
            textRenderer = new TextRenderer();
            selectionManager = new SelectionManager();
            designHelper = new DesignHelper(textboxProperties);
            canvasUpdater = new CanvasUpdateHelper(Canvas_Text, Canvas_Selection, Canvas_Cursor, Canvas_LineNumber);
            workingLine = new CurrentWorkingLine(Canvas_Text, textRenderer);
            cursorRenderer = new CursorRenderer();
            scrollBarManager = new ScrollBarManager(Scroll, textManager, textboxProperties);
            longestLineCalcHelper = new LongestLineCalculationHelper(textManager, textRenderer);
            undoRedoManager = new UndoRedoManager(textManager, textboxProperties);
            textActionsManager = new TextActionsManager(this, longestLineCalcHelper, undoRedoManager, selectionManager, workingLine, canvasUpdater, textManager, textboxProperties);
            pointerManager = new PointerManager();
            selectionRenderer = new SelectionRenderer(textRenderer, textboxProperties);
            Setup();
        }

        private void Setup()
        {
            Design = null;
            RequestedTheme = ElementTheme.Dark;
            textManager.Lines.Add("");
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            focusManager.RemoveFocus();
        }
        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            focusManager.SetFocus();
        }

        private void UserControl_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {

        }

        private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
        {

        }

        private void Canvas_Selection_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var delta = e.GetCurrentPoint(this).Properties.MouseWheelDelta;
            bool needsUpdate = false;
            //Zoom using mousewheel
            if (Utils.IsKeyPressed(VirtualKey.Control))
            {
                textboxProperties.ZoomFactor += delta / 20;
                //UpdateZoom();
                return;
            }
            //Scroll horizontal using mousewheel
            else if (Utils.IsKeyPressed(VirtualKey.Shift))
            {
                HorizontalScrollbar.Value -= delta * HorizontalScrollSensitivity;
                needsUpdate = true;
            }
            //Scroll horizontal using touchpad
            else if (e.GetCurrentPoint(this).Properties.IsHorizontalMouseWheel)
            {
                HorizontalScrollbar.Value += delta * HorizontalScrollSensitivity;
                needsUpdate = true;
            }
            //Scroll vertical using mousewheel
            else
            {
                VerticalScrollbar.Value -= (delta * VerticalScrollSensitivity) / textboxProperties.DefaultVerticalScrollSensitivity;
                //Only update when a line was scrolled
                if ((int)(VerticalScrollbar.Value / textboxProperties.SingleLineHeight * textboxProperties.DefaultVerticalScrollSensitivity) != textRenderer.RenderStartLine)
                {
                    needsUpdate = true;
                }
            }

            if (needsUpdate)
                canvasUpdater.UpdateAll();
        }

        private void TextInputManager_TextChanged(string text)
        {
            if (!focusManager.HasFocus)
                return;

            if(text.Length > 0)
            {
                textActionsManager.AddText(text);
            }
        }

        #region Draw Canvas
        private void Canvas_Text_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            var textLayout = textRenderer.Render(sender, textManager, VerticalScrollbar, textboxProperties);

            scrollBarManager.SetVerticalScrollBounds(VerticalScrollbar, sender);
            scrollBarManager.SetHorizontalScrollBounds(HorizontalScrollbar, sender, longestLineCalcHelper);
            designHelper.UpdateColorResources(args.DrawingSession, sender, MainGrid);

            args.DrawingSession.DrawTextLayout(textLayout, (float)-HorizontalScroll, textboxProperties.SingleLineHeight, textboxProperties.TextColorBrush);
        }
        private void Canvas_Cursor_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            cursorRenderer.Render(this, args, selectionManager.Cursor, textRenderer, textboxProperties, textManager, workingLine);
        }
        private void Canvas_Selection_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            selectionManager.Selection = selectionRenderer.Render(this, designHelper, selectionManager, args);
        }
        #endregion

        #region Properties
        public TextControlBoxDesign Design
        {
            get => designHelper.UseDefaultDesign ? null : designHelper.CurrentDesign;
            set
            {
                designHelper.CurrentDesign = value != null ? value : textboxProperties.AppTheme == ApplicationTheme.Dark ? textboxProperties.DarkDesign : textboxProperties.LightDesign;
                designHelper.UseDefaultDesign = value == null;

                this.Background = designHelper.CurrentDesign.Background;
                designHelper.NeedsUpdateColorResources();
                canvasUpdater.UpdateAll();
            }
        }

        public new ElementTheme RequestedTheme
        {
            get => textboxProperties.RequestedTheme;
            set
            {
                textboxProperties.RequestedTheme = value;
                textboxProperties.AppTheme = Utils.ConvertTheme(value);

                if (designHelper.UseDefaultDesign)
                    designHelper.CurrentDesign = textboxProperties.AppTheme == ApplicationTheme.Light ? textboxProperties.LightDesign : textboxProperties.DarkDesign;

                this.Background = designHelper.CurrentDesign.Background;
                designHelper.NeedsUpdateColorResources();
                textRenderer.NeedsTextFormatUpdate();
                canvasUpdater.UpdateAll();
            }
        }

        /// <summary>
        /// Gets or sets the vertical scroll position in the textbox.
        /// </summary>
        public double VerticalScroll { get => VerticalScrollbar.Value; set { VerticalScrollbar.Value = value < 0 ? 0 : value; canvasUpdater.UpdateAll(); } }

        /// <summary>
        /// Gets or sets the horizontal scroll position in the textbox.
        /// </summary>
        public double HorizontalScroll { get => HorizontalScrollbar.Value; set { HorizontalScrollbar.Value = value < 0 ? 0 : value; canvasUpdater.UpdateAll(); } }

        /// <summary>
        /// Gets or sets the sensitivity of vertical scrolling in the textbox.
        /// </summary>
        public double VerticalScrollSensitivity { get => textboxProperties.VerticalScrollSensitivity; set => textboxProperties.VerticalScrollSensitivity = value < 1 ? 1 : value; }

        /// <summary>
        /// Gets or sets the sensitivity of horizontal scrolling in the textbox.
        /// </summary>
        public double HorizontalScrollSensitivity { get => textboxProperties.HorizontalScrollSensitivity; set => textboxProperties.HorizontalScrollSensitivity = value < 1 ? 1 : value; }

        /// <summary>
        /// Gets or sets the size of the cursor in the textbox.
        /// </summary>
        public CursorSize CursorSize { get => textboxProperties._CursorSize; set { textboxProperties._CursorSize = value; canvasUpdater.UpdateCursor(); } }
        
        /// <summary>
        /// Loads the specified lines into the textbox, resetting all content and undo history.
        /// </summary>
        /// <param name="lines">An enumerable containing the lines to load into the textbox.</param>
        /// <param name="lineEnding">The line ending format used in the loaded lines (default is CRLF).</param>
        public void LoadLines(IEnumerable<string> lines, LineEnding lineEnding = LineEnding.CRLF)
        {
            SafeHandler.Safe_LoadLines(canvasUpdater,this, textManager, lines, lineEnding);
        }
        /// <summary>
        /// Selects the entire line specified by its index.
        /// </summary>
        /// <param name="line">The index of the line to select.</param>
        public void SelectLine(int line)
        {
            selectionManager.SetSelection(new CursorPosition(0, line), new CursorPosition(textManager.Lines.GetLineLength(line), line));
            selectionManager.Selection.StartPosition = selectionManager.Selection.EndPosition;

            canvasUpdater.UpdateSelection().UpdateCursor();
        }
        #endregion

        private void HorizontalScrollbar_Scroll(object sender, Microsoft.UI.Xaml.Controls.Primitives.ScrollEventArgs e)
        {
            canvasUpdater.UpdateAll();
        }

        private void VerticalScrollbar_Scroll(object sender, Microsoft.UI.Xaml.Controls.Primitives.ScrollEventArgs e)
        {
            //only update when a line was scrolled
            if ((int)(VerticalScrollbar.Value / textboxProperties.SingleLineHeight * textboxProperties.DefaultVerticalScrollSensitivity) != textRenderer.RenderStartLine)
            {
                canvasUpdater.UpdateAll();
            }
        }



        private void Canvas_Selection_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(Canvas_Selection);
            if (point.Properties.IsLeftButtonPressed && !Utils.IsKeyPressed(VirtualKey.Shift)) 
            {
                pointerManager.LeftDown();
            }

            if (pointerManager.PointerClickCount == 3)
            {
                SelectLine(selectionManager.Cursor.LineNumber);
                pointerManager.ResetClicks();
                return;
            }
            else if(pointerManager.PointerClickCount == 2)
            {
                CursorHelper.UpdateCursorVariable(this, point.Position, textboxProperties, selectionManager, textRenderer, textManager, workingLine);
                Selection.SelectSingleWord(canvasUpdater, selectionManager, workingLine, selectionManager.Cursor);
            }
            else
            {
                if (point.Properties.IsRightButtonPressed)
                {
                    //Context Menu
                }

                if (point.Properties.IsLeftButtonPressed)
                {
                    if (Utils.IsKeyPressed(VirtualKey.Shift))
                    {
                        if (selectionManager.Selection.StartPosition == null)
                            selectionManager.SetSelectionStart(new CursorPosition(selectionManager.Cursor));

                        CursorHelper.UpdateCursorVariable(this, point.Position, textboxProperties, selectionManager, textRenderer, textManager, workingLine);
                        selectionManager.SetSelectionEnd(new CursorPosition(selectionManager.Cursor));
                        
                        Canvas_Selection.ReleasePointerCaptures();

                        canvasUpdater.UpdateSelection().UpdateCursor();

                        return;
                    }

                    CursorHelper.UpdateCursorVariable(this, point.Position, textboxProperties, selectionManager, textRenderer, textManager, workingLine);

                    //pressing inside the selection:
                    if (selectionManager.HasSelection)
                    {
                        selectionManager.ClearSelection();
                        canvasUpdater.UpdateSelection();
                    }
                    else
                    {
                        Canvas_Selection.CapturePointer(e.Pointer);
                        selectionManager.SetSelectionStart(new CursorPosition(selectionManager.Cursor));
                        selectionManager.IsSelecting = false;
                    }
                }
            }
        }

        private void Canvas_Selection_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!focusManager.HasFocus)
                return;

            var point = e.GetCurrentPoint(Canvas_Selection);
            if (selectionManager.IsSelecting)
            {
                //selection started over the linenumbers:
                if (selectionManager.IsSelectingOverLinenumbers)
                {
                    Point pointerPos = point.Position;
                    pointerPos.Y += textboxProperties.SingleLineHeight; //add one more line

                    //When the selection reaches the end of the textbox select the last line completely
                    if (selectionManager.Cursor.LineNumber == textManager.Lines.Count - 1)
                    {
                        pointerPos.Y -= textboxProperties.SingleLineHeight; //add one more line
                        pointerPos.X = Utils.MeasureLineLenght(CanvasDevice.GetSharedDevice(), textManager.Lines.GetLineText(-1), textRenderer.textFormat).Width + 10;
                    }
                    CursorHelper.UpdateCursorVariable(this, pointerPos, textboxProperties, selectionManager, textRenderer, textManager, workingLine);
                }
                else //Default selection
                    CursorHelper.UpdateCursorVariable(this, point.Position, textboxProperties, selectionManager, textRenderer, textManager, workingLine);

                //Update:
                canvasUpdater.UpdateCursor();
                selectionManager.SetSelectionEnd(new CursorPosition(selectionManager.Cursor.CharacterPosition, selectionManager.Cursor.LineNumber));
                canvasUpdater.UpdateSelection();
            }
        }

        private void Canvas_Selection_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!focusManager.HasFocus)
                return;

            Canvas_Selection.ReleasePointerCaptures();
        }
    }
}