using Collections.Pooled;
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
using System.Diagnostics;
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
using Color = Windows.UI.Color;

namespace TextControlBoxNS
{
    public partial class TextControlBox : UserControl
    {
        private CursorSize _CursorSize = null;
        private FontFamily _FontFamily = new FontFamily("Consolas");
        private bool UseDefaultDesign = true;
        private TextControlBoxDesign _Design { get; set; }
        private string NewLineCharacter = "\r\n";
        private bool _ShowLineNumbers = true;
        private CodeLanguage _CodeLanguage = null;
        private LineEnding _LineEnding = LineEnding.CRLF;
        private bool _ShowLineHighlighter = true;
        private int _FontSize = 18;
        private int _ZoomFactor = 100; //%
        private double _HorizontalScrollSensitivity = 1;
        private double _VerticalScrollSensitivity = 1;
        private int DefaultVerticalScrollSensitivity = 4;
        private float SingleLineHeight { get => TextFormat == null ? 0 : TextFormat.LineSpacing; }
        private float ZoomedFontSize = 0;
        private int MaxFontsize = 125;
        private int MinFontSize = 3;
        private int OldZoomFactor = 0;
        private int NumberOfStartLine = 0;
        private int LongestLineLength = 0;
        private int LongestLineIndex = 0;
        private float OldHorizontalScrollValue = 0;
        private float _SpaceBetweenLineNumberAndText = 30;
        private ElementTheme _RequestedTheme = ElementTheme.Default;
        private ApplicationTheme _AppTheme = ApplicationTheme.Light;
        private TextControlBoxDesign LightDesign = new TextControlBoxDesign(
            new SolidColorBrush(Color.FromArgb(0, 255, 255, 255)),
            Color.FromArgb(255, 50, 50, 50),
            Color.FromArgb(100, 0, 100, 255),
            Color.FromArgb(255, 0, 0, 0),
            Color.FromArgb(50, 200, 200, 200),
            Color.FromArgb(255, 180, 180, 180),
            Color.FromArgb(0, 0, 0, 0),
            Color.FromArgb(100, 200, 120, 0)
            );
        private TextControlBoxDesign DarkDesign = new TextControlBoxDesign(
            new SolidColorBrush(Color.FromArgb(0, 30, 30, 30)),
            Color.FromArgb(255, 255, 255, 255),
            Color.FromArgb(100, 0, 100, 255),
            Color.FromArgb(255, 255, 255, 255),
            Color.FromArgb(50, 100, 100, 100),
            Color.FromArgb(255, 100, 100, 100),
            Color.FromArgb(0, 0, 0, 0),
            Color.FromArgb(100, 160, 80, 0)
            );

        //Colors:
        CanvasSolidColorBrush TextColorBrush;
        CanvasSolidColorBrush CursorColorBrush;
        CanvasSolidColorBrush LineNumberColorBrush;
        CanvasSolidColorBrush LineHighlighterBrush;

        bool ColorResourcesCreated = false;
        bool NeedsTextFormatUpdate = false;
        bool DragDropSelection = false;
        bool HasFocus = true;
        bool NeedsUpdateTextLayout = false;
        bool NeedsRecalculateLongestLineIndex = true;

        CanvasTextFormat TextFormat = null;
        CanvasTextLayout DrawnTextLayout = null;
        CanvasTextLayout LineNumberTextLayout = null;
        CanvasTextFormat LineNumberTextFormat = null;

        string RenderedText = "";
        string LineNumberTextToRender = "";
        string OldRenderedText = null;
        string OldLineNumberTextToRender = "";
        //Handle double and triple -clicks:
        int PointerClickCount = 0;
        DispatcherTimer PointerClickTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 200) };

        Point? OldTouchPosition = null;

        //CursorPosition
        CursorPosition _CursorPosition = new CursorPosition(0, 0);
        CursorPosition OldCursorPosition = null;
        CanvasTextLayout CurrentLineTextLayout = null;
        TextSelection TextSelection = null;
        TextSelection OldTextSelection = null;

        //Store the lines in Lists
        private PooledList<string> TotalLines = new PooledList<string>(0);
        private IEnumerable<string> RenderedLines;
        private int NumberOfRenderedLines = 0;

        private string CurrentLine { get => TotalLines.GetCurrentLineText(); set => TotalLines.SetCurrentLineText(value); }
        StringBuilder LineNumberContent = new StringBuilder();

        //Classes
        private readonly SelectionRenderer selectionrenderer;
        private readonly UndoRedo undoRedo = new UndoRedo();
        private readonly FlyoutHelper flyoutHelper;
        private readonly TabSpaceHelper tabSpaceHelper = new TabSpaceHelper();
        private readonly StringManager stringManager;
        private readonly SearchHelper searchHelper = new SearchHelper();
        private readonly CanvasHelper canvasHelper;

        /// <summary>
        /// Initializes a new instance of the TextControlBox class.
        /// </summary>
        public TextControlBox()
        {
            this.InitializeComponent();

            //Classes & Variables:
            selectionrenderer = new SelectionRenderer();
            flyoutHelper = new FlyoutHelper(this);
            stringManager = new StringManager(tabSpaceHelper);
            canvasHelper = new CanvasHelper(Canvas_Selection, Canvas_LineNumber, Canvas_Text, Canvas_Cursor);

            //set default values
            RequestedTheme = ElementTheme.Default;
            LineEnding = LineEnding.CRLF;

            InitialiseOnStart();
            SetFocus();
        }

        private void ChangeCursor(InputSystemCursorShape cursor)
        {
            this.ProtectedCursor = InputSystemCursor.Create(cursor);
        }

        #region Update functions
        private void UpdateZoom()
        {
            ZoomedFontSize = Math.Clamp(_FontSize * (float)_ZoomFactor / 100, MinFontSize, MaxFontsize);
            _ZoomFactor = Math.Clamp(_ZoomFactor, 4, 400);

            if (_ZoomFactor != OldZoomFactor)
            {
                NeedsUpdateTextLayout = true;
                OldZoomFactor = _ZoomFactor;
                ZoomChanged?.Invoke(this, _ZoomFactor);
            }

            NeedsTextFormatUpdate = true;

            ScrollLineIntoView(CursorPosition.LineNumber);
            NeedsUpdateLineNumbers();
            canvasHelper.UpdateAll();
        }

        private void NeedsUpdateLineNumbers()
        {
            this.OldLineNumberTextToRender = "";
        }
        private void UpdateCurrentLineTextLayout()
        {
            CurrentLineTextLayout =
                CursorPosition.LineNumber < TotalLines.Count ?
                TextLayoutHelper.CreateTextLayout(
                    Canvas_Text,
                    TextFormat,
                    TotalLines.GetLineText(CursorPosition.LineNumber) + "|",
                    Canvas_Text.Size) :
                null;
        }
        private void UpdateCursorVariable(Point point)
        {
            //Apply an offset to the cursorposition
            point = point.Subtract(-(SingleLineHeight / 4), SingleLineHeight / 4);

            CursorPosition.LineNumber = CursorRenderer.GetCursorLineFromPoint(point, SingleLineHeight, NumberOfRenderedLines, NumberOfStartLine);

            UpdateCurrentLineTextLayout();
            CursorPosition.CharacterPosition = CursorRenderer.GetCharacterPositionFromPoint(TotalLines.GetCurrentLineText(), CurrentLineTextLayout, point, (float)-HorizontalScroll);
        }
        private void UpdateCurrentLine()
        {
            TotalLines.UpdateCurrentLine(CursorPosition.LineNumber);
        }
        private void CheckRecalculateLongestLine(string text)
        {
            if (Utils.GetLongestLineLength(text) > LongestLineLength)
            {
                NeedsRecalculateLongestLineIndex = true;
            }
        }
        #endregion

        #region Textediting
        private void DeleteSelection()
        {
            if (TextSelection == null)
                return;

            //line gets deleted -> recalculate the longest line:
            if (TextSelection.IsLineInSelection(LongestLineIndex))
                NeedsRecalculateLongestLineIndex = true;

            undoRedo.RecordUndoAction(() =>
            {
                CursorPosition = Selection.Remove(TextSelection, TotalLines);
                ClearSelection();

            }, TotalLines, TextSelection, 0, NewLineCharacter);

            canvasHelper.UpdateSelection();
            canvasHelper.UpdateCursor();
        }
        private void AddCharacter(string text, bool ignoreSelection = false)
        {
            if (IsReadonly)
                return;

            if (ignoreSelection)
                ClearSelection();

            int splittedTextLength = text.Contains(NewLineCharacter, StringComparison.Ordinal) ? Utils.CountLines(text, NewLineCharacter) : 1;

            if (TextSelection == null && splittedTextLength == 1)
            {
                var res = AutoPairing.AutoPair(this, text);
                text = res.text;

                undoRedo.RecordUndoAction(() =>
                {
                    var characterPos = GetCurPosInLine();

                    if (characterPos > TotalLines.CurrentLineLength() - 1)
                        CurrentLine = CurrentLine.AddToEnd(text);
                    else
                        CurrentLine = CurrentLine.AddText(text, characterPos);
                    CursorPosition.CharacterPosition = res.length + characterPos;

                }, TotalLines, CursorPosition.LineNumber, 1, 1, NewLineCharacter);

                if (TotalLines.GetCurrentLineText().Length > LongestLineLength)
                {
                    LongestLineIndex = CursorPosition.LineNumber;
                }
            }
            else if (TextSelection == null && splittedTextLength > 1)
            {
                CheckRecalculateLongestLine(text);
                undoRedo.RecordUndoAction(() =>
                {
                    CursorPosition = Selection.InsertText(TextSelection, CursorPosition, TotalLines, text, NewLineCharacter);
                }, TotalLines, CursorPosition.LineNumber, 1, splittedTextLength, NewLineCharacter);
            }
            else if (text.Length == 0) //delete selection
            {
                DeleteSelection();
            }
            else if (TextSelection != null)
            {
                text = AutoPairing.AutoPairSelection(this, text);
                if (text == null)
                    return;

                CheckRecalculateLongestLine(text);
                undoRedo.RecordUndoAction(() =>
                {
                    CursorPosition = Selection.Replace(TextSelection, TotalLines, text, NewLineCharacter);

                    ClearSelection();
                    canvasHelper.UpdateSelection();
                }, TotalLines, TextSelection, splittedTextLength, NewLineCharacter);
            }

            ScrollLineToCenter(CursorPosition.LineNumber);
            canvasHelper.UpdateText();
            canvasHelper.UpdateCursor();
            Internal_TextChanged();
        }
        private void RemoveText(bool controlIsPressed = false)
        {
            UpdateCurrentLine();

            if (IsReadonly)
                return;

            if (TextSelection != null)
                DeleteSelection();
            else
            {
                string curLine = CurrentLine;
                var charPos = GetCurPosInLine();
                var stepsToMove = controlIsPressed ? Cursor.CalculateStepsToMoveLeft(CurrentLine, charPos) : 1;

                if (charPos - stepsToMove >= 0)
                {
                    if (CursorPosition.LineNumber == LongestLineIndex)
                        NeedsRecalculateLongestLineIndex = true;

                    undoRedo.RecordUndoAction(() =>
                    {
                        CurrentLine = curLine.SafeRemove(charPos - stepsToMove, stepsToMove);
                        CursorPosition.CharacterPosition -= stepsToMove;

                    }, TotalLines, CursorPosition.LineNumber, 1, 1, NewLineCharacter);
                }
                else if (charPos - stepsToMove < 0) //remove lines
                {
                    if (CursorPosition.LineNumber <= 0)
                        return;

                    if (CursorPosition.LineNumber == LongestLineIndex)
                        NeedsRecalculateLongestLineIndex = true;

                    undoRedo.RecordUndoAction(() =>
                    {
                        int curpos = TotalLines.GetLineLength(CursorPosition.LineNumber - 1);

                        //line still has text:
                        if (curLine.Length > 0)
                            TotalLines.String_AddToEnd(CursorPosition.LineNumber - 1, curLine);

                        TotalLines.DeleteAt(CursorPosition.LineNumber);

                        //update the cursorposition
                        CursorPosition.LineNumber -= 1;
                        CursorPosition.CharacterPosition = curpos;

                    }, TotalLines, CursorPosition.LineNumber - 1, 3, 2, NewLineCharacter);
                }
            }

            UpdateScrollToShowCursor();
            canvasHelper.UpdateText();
            canvasHelper.UpdateCursor();
            Internal_TextChanged();
        }
        private void DeleteText(bool controlIsPressed = false, bool shiftIsPressed = false)
        {
            UpdateCurrentLine();
            string curLine = CurrentLine;

            if (IsReadonly)
                return;

            //Shift + delete:
            if (shiftIsPressed && TextSelection == null)
                DeleteLine(CursorPosition.LineNumber);
            else if (TextSelection != null)
                DeleteSelection();
            else
            {
                int characterPos = GetCurPosInLine();
                //delete lines if cursor is at position 0 and the line is emty OR the cursor is at the end of a line and the line has content
                if (characterPos == curLine.Length)
                {
                    string lineToAdd = CursorPosition.LineNumber + 1 < TotalLines.Count ? TotalLines.GetLineText(CursorPosition.LineNumber + 1) : null;
                    if (lineToAdd != null)
                    {
                        if (CursorPosition.LineNumber == LongestLineIndex)
                            NeedsRecalculateLongestLineIndex = true;

                        undoRedo.RecordUndoAction(() =>
                        {
                            int curpos = TotalLines.GetLineLength(CursorPosition.LineNumber);
                            CurrentLine += lineToAdd;
                            TotalLines.DeleteAt(CursorPosition.LineNumber + 1);

                            //update the cursorposition
                            CursorPosition.CharacterPosition = curpos;

                        }, TotalLines, CursorPosition.LineNumber, 2, 1, NewLineCharacter);
                    }
                }
                //delete text in line
                else if (TotalLines.Count > CursorPosition.LineNumber)
                {
                    int stepsToMove = controlIsPressed ? Cursor.CalculateStepsToMoveRight(curLine, characterPos) : 1;

                    if (CursorPosition.LineNumber == LongestLineIndex)
                        NeedsRecalculateLongestLineIndex = true;

                    undoRedo.RecordUndoAction(() =>
                    {
                        CurrentLine = curLine.SafeRemove(characterPos, stepsToMove);
                    }, TotalLines, CursorPosition.LineNumber, 1, 1, NewLineCharacter);
                }
            }

            UpdateScrollToShowCursor();
            canvasHelper.UpdateText();
            canvasHelper.UpdateCursor();
            Internal_TextChanged();
        }
        private void AddNewLine()
        {
            if (IsReadonly)
                return;

            if (TotalLines.Count == 0)
            {
                TotalLines.AddLine();
                return;
            }

            CursorPosition startLinePos = new CursorPosition(TextSelection == null ? CursorPosition.ChangeLineNumber(CursorPosition, CursorPosition.LineNumber) : Selection.GetMin(TextSelection));

            //If the whole text is selected
            if (Selection.WholeTextSelected(TextSelection, TotalLines))
            {
                undoRedo.RecordUndoAction(() =>
                {
                    ListHelper.Clear(TotalLines, true);
                    TotalLines.InsertNewLine(-1);
                    CursorPosition = new CursorPosition(0, 1);
                }, TotalLines, 0, TotalLines.Count, 2, NewLineCharacter);
                ForceClearSelection();
                canvasHelper.UpdateAll();
                Internal_TextChanged();
                return;
            }

            if (TextSelection == null) //No selection
            {
                string startLine = TotalLines.GetLineText(startLinePos.LineNumber);

                undoRedo.RecordUndoAction(() =>
                {
                    string[] splittedLine = Utils.SplitAt(TotalLines.GetLineText(startLinePos.LineNumber), startLinePos.CharacterPosition);

                    TotalLines.SetLineText(startLinePos.LineNumber, splittedLine[1]);
                    TotalLines.InsertOrAdd(startLinePos.LineNumber, splittedLine[0]);

                }, TotalLines, startLinePos.LineNumber, 1, 2, NewLineCharacter);
            }
            else //Any kind of selection
            {
                int remove = 2;
                if (TextSelection.StartPosition.LineNumber == TextSelection.EndPosition.LineNumber)
                {
                    //line is selected completely: remove = 1
                    if (Selection.GetMax(TextSelection.StartPosition, TextSelection.EndPosition).CharacterPosition == TotalLines.GetLineLength(CursorPosition.LineNumber) &&
                        Selection.GetMin(TextSelection.StartPosition, TextSelection.EndPosition).CharacterPosition == 0)
                    {
                        remove = 1;
                    }
                }

                undoRedo.RecordUndoAction(() =>
                {
                    CursorPosition = Selection.Replace(TextSelection, TotalLines, NewLineCharacter, NewLineCharacter);
                }, TotalLines, TextSelection, remove, NewLineCharacter);
            }

            ClearSelection();
            CursorPosition.LineNumber += 1;
            CursorPosition.CharacterPosition = 0;

            if (TextSelection == null && CursorPosition.LineNumber == NumberOfRenderedLines + NumberOfStartLine)
                ScrollOneLineDown();
            else
                UpdateScrollToShowCursor();

            canvasHelper.UpdateAll();
            Internal_TextChanged();
        }
        #endregion

        #region Random functions
        private void InitialiseOnStart()
        {
            UpdateZoom();
            if (TotalLines.Count == 0)
            {
                TotalLines.AddLine();
            }
        }
        private void ForceClearSelection()
        {
            selectionrenderer.ClearSelection();
            TextSelection = null;
            canvasHelper.UpdateSelection();
        }
        private void StartSelectionIfNeeded()
        {
            if (Selection.SelectionIsNull(selectionrenderer, TextSelection))
            {
                selectionrenderer.SelectionStartPosition = selectionrenderer.SelectionEndPosition = new CursorPosition(CursorPosition.CharacterPosition, CursorPosition.LineNumber);
                TextSelection = new TextSelection(selectionrenderer.SelectionStartPosition, selectionrenderer.SelectionEndPosition);
            }
        }
        private void DoDragDropSelection()
        {
            if (TextSelection == null || IsReadonly)
                return;

            //Position to insert is selection start or selection end -> no need to drag
            if (Cursor.Equals(TextSelection.StartPosition, CursorPosition) || Cursor.Equals(TextSelection.EndPosition, CursorPosition))
            {
                ChangeCursor(InputSystemCursorShape.IBeam);
                DragDropSelection = false;
                return;
            }

            string textToInsert = SelectedText;
            CursorPosition curpos = new CursorPosition(CursorPosition);

            //Delete the selection
            RemoveText();

            CursorPosition = curpos;

            AddCharacter(textToInsert, false);

            ChangeCursor(InputSystemCursorShape.IBeam);
            DragDropSelection = false;
            canvasHelper.UpdateAll();
        }
        private void EndDragDropSelection(bool clearSelectedText = true)
        {
            DragDropSelection = false;
            if (clearSelectedText)
                ClearSelection();

            ChangeCursor(InputSystemCursorShape.IBeam);
            selectionrenderer.IsSelecting = false;
            canvasHelper.UpdateCursor();
        }
        private bool DragDropOverSelection(Point curPos)
        {
            bool res = selectionrenderer.CursorIsInSelection(CursorPosition, TextSelection) ||
                selectionrenderer.PointerIsOverSelection(curPos, TextSelection, DrawnTextLayout);

            ChangeCursor(res ? InputSystemCursorShape.UniversalNo : InputSystemCursorShape.IBeam);

            return res;
        }
        private void UpdateScrollToShowCursor(bool update = true)
        {
            if (NumberOfStartLine + NumberOfRenderedLines <= CursorPosition.LineNumber)
                VerticalScrollbar.Value = (CursorPosition.LineNumber - NumberOfRenderedLines + 1) * SingleLineHeight / DefaultVerticalScrollSensitivity;
            else if (NumberOfStartLine > CursorPosition.LineNumber)
                VerticalScrollbar.Value = (CursorPosition.LineNumber - 1) * SingleLineHeight / DefaultVerticalScrollSensitivity;

            if (update)
                canvasHelper.UpdateAll();
        }
        private int GetCurPosInLine()
        {
            int curLineLength = CurrentLine.Length;

            if (CursorPosition.CharacterPosition > curLineLength)
                return curLineLength;
            return CursorPosition.CharacterPosition;
        }
        private void ScrollIntoViewHorizontal()
        {
            float curPosInLine = CursorRenderer.GetCursorPositionInLine(CurrentLineTextLayout, CursorPosition, 0);
            if (curPosInLine == OldHorizontalScrollValue)
                return;

            HorizontalScrollbar.Value = curPosInLine - (Canvas_Text.ActualWidth - 10);
            OldHorizontalScrollValue = curPosInLine;
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
        private void CreateColorResources(ICanvasResourceCreatorWithDpi resourceCreator)
        {
            if (ColorResourcesCreated)
                return;

            Canvas_LineNumber.ClearColor = _Design.LineNumberBackground;
            MainGrid.Background = _Design.Background;
            TextColorBrush = new CanvasSolidColorBrush(resourceCreator, _Design.TextColor);
            CursorColorBrush = new CanvasSolidColorBrush(resourceCreator, _Design.CursorColor);
            LineNumberColorBrush = new CanvasSolidColorBrush(resourceCreator, _Design.LineNumberColor);
            LineHighlighterBrush = new CanvasSolidColorBrush(resourceCreator, _Design.LineHighlighterColor);
            ColorResourcesCreated = true;
        }
        private void UpdateWhenScrolled()
        {
            //only update when a line was scrolled
            if ((int)(VerticalScrollbar.Value / SingleLineHeight) != NumberOfStartLine)
            {
                canvasHelper.UpdateAll();
            }
        }
        private bool OutOfRenderedArea(int line)
        {
            //Check whether the current line is outside the bounds of the visible area
            return line < NumberOfStartLine || line >= NumberOfStartLine + NumberOfRenderedLines;
        }

        //Trys running the code and clears the memory if OutOfMemoryException gets thrown
        private async void Safe_Paste(bool handleException = true)
        {
            try
            {
                DataPackageView dataPackageView = Clipboard.GetContent();
                if (dataPackageView.Contains(StandardDataFormats.Text))
                {
                    string text = null;
                    try
                    {
                        text = await dataPackageView.GetTextAsync();
                    }
                    catch (Exception ex) //When longer holding Ctrl + V the clipboard may throw an exception:
                    {
                        Debug.WriteLine("Clipboard exception: " + ex.Message);
                        return;
                    }

                    if (await Utils.IsOverTextLimit(text.Length))
                        return;

                    AddCharacter(stringManager.CleanUpString(text));
                }
            }
            catch (OutOfMemoryException)
            {
                if (handleException)
                {
                    CleanUp();
                    Safe_Paste(false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
        private string Safe_Gettext(bool handleException = true)
        {
            try
            {
                return TotalLines.GetString(NewLineCharacter);
            }
            catch (OutOfMemoryException)
            {
                if (handleException)
                {
                    CleanUp();
                    return Safe_Gettext(false);
                }
                throw new OutOfMemoryException();
            }
        }
        private void Safe_Cut(bool handleException = true)
        {
            try
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(SelectedText);
                if (TextSelection == null)
                    DeleteLine(CursorPosition.LineNumber); //Delete the line
                else
                    DeleteText(); //Delete the selected text

                dataPackage.RequestedOperation = DataPackageOperation.Move;
                Clipboard.SetContent(dataPackage);
                ClearSelection();
            }
            catch (OutOfMemoryException)
            {
                if (handleException)
                {
                    CleanUp();
                    Safe_Cut(false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
        private void Safe_Copy(bool handleException = true)
        {
            try
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(SelectedText);
                dataPackage.RequestedOperation = DataPackageOperation.Copy;
                Clipboard.SetContent(dataPackage);
            }
            catch (OutOfMemoryException)
            {
                if (handleException)
                {
                    CleanUp();
                    Safe_Copy(false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
        private void Safe_LoadLines(IEnumerable<string> lines, LineEnding lineEnding = LineEnding.CRLF, bool HandleException = true)
        {
            try
            {
                selectionrenderer.ClearSelection();
                undoRedo.ClearAll();

                ListHelper.Clear(TotalLines);
                TotalLines.AddRange(lines);

                this.LineEnding = lineEnding;

                NeedsRecalculateLongestLineIndex = true;
                canvasHelper.UpdateAll();
            }
            catch (OutOfMemoryException)
            {
                if (HandleException)
                {
                    CleanUp();
                    Safe_LoadLines(lines, lineEnding, false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
        private async void Safe_LoadText(string text, bool handleException = true)
        {
            try
            {
                if (await Utils.IsOverTextLimit(text.Length))
                    return;

                //Get the LineEnding
                LineEnding = LineEndings.FindLineEnding(text);

                selectionrenderer.ClearSelection();
                undoRedo.ClearAll();

                NeedsRecalculateLongestLineIndex = true;

                if (text.Length == 0)
                    ListHelper.Clear(TotalLines, true);
                else
                    Selection.ReplaceLines(TotalLines, 0, TotalLines.Count, stringManager.CleanUpString(text).Split(NewLineCharacter));

                canvasHelper.UpdateAll();
            }
            catch (OutOfMemoryException)
            {
                if (handleException)
                {
                    CleanUp();
                    Safe_LoadText(text, false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
        private async void Safe_SetText(string text, bool handleException = true)
        {
            try
            {
                if (await Utils.IsOverTextLimit(text.Length))
                    return;

                selectionrenderer.ClearSelection();
                NeedsRecalculateLongestLineIndex = true;
                undoRedo.RecordUndoAction(() =>
                {
                    Selection.ReplaceLines(TotalLines, 0, TotalLines.Count, stringManager.CleanUpString(text).Split(NewLineCharacter));
                    if (text.Length == 0) //Create a new line when the text gets cleared
                    {
                        TotalLines.AddLine();
                    }
                }, TotalLines, 0, TotalLines.Count, Utils.CountLines(text, NewLineCharacter), NewLineCharacter);

                canvasHelper.UpdateAll();
            }
            catch (OutOfMemoryException)
            {
                if (handleException)
                {
                    CleanUp();
                    Safe_SetText(text, false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
        private void CleanUp()
        {
            Debug.WriteLine("Collect GC");
            ListHelper.GCList(TotalLines);
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
                    if (CursorPosition.LineNumber == TotalLines.Count - 1)
                    {
                        pointerPos.Y -= SingleLineHeight; //add one more line
                        pointerPos.X = Utils.MeasureLineLenght(CanvasDevice.GetSharedDevice(), TotalLines.GetLineText(-1), TextFormat).Width + 10;
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
                    selection = TabKey.MoveTabBack(TotalLines, TextSelection, CursorPosition, tabSpaceHelper.TabCharacter, NewLineCharacter, undoRedo);
                else
                    selection = TabKey.MoveTab(TotalLines, TextSelection, CursorPosition, tabSpaceHelper.TabCharacter, NewLineCharacter, undoRedo);

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
                        Selection.SelectSingleWord(canvasHelper, selectionrenderer, CursorPosition, CurrentLine);
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
                        TotalLines,
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
                            Cursor.MoveLeft(CursorPosition, TotalLines, CurrentLine);
                            selectionrenderer.SetSelectionEnd(CursorPosition);
                        }
                        else
                        {
                            //Move the cursor to the start of the selection
                            if (selectionrenderer.HasSelection && TextSelection != null)
                                CursorPosition = Selection.GetMin(TextSelection);
                            else
                                Cursor.MoveLeft(CursorPosition, TotalLines, CurrentLine);

                            Selection.ClearSelectionIfNeeded(this, selectionrenderer);
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
                            Cursor.MoveRight(CursorPosition, TotalLines, CurrentLine);
                            selectionrenderer.SetSelectionEnd(CursorPosition);
                        }
                        else
                        {
                            //Move the cursor to the end of the selection
                            if (selectionrenderer.HasSelection && TextSelection != null)
                                CursorPosition = Selection.GetMax(TextSelection);
                            else
                                Cursor.MoveRight(CursorPosition, TotalLines, TotalLines.GetCurrentLineText());

                            Selection.ClearSelectionIfNeeded(this, selectionrenderer);
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
                            CursorPosition = selectionrenderer.SelectionEndPosition = Cursor.MoveDown(selectionrenderer.SelectionEndPosition, TotalLines.Count);
                            selectionrenderer.IsSelecting = false;
                        }
                        else
                        {
                            Selection.ClearSelectionIfNeeded(this, selectionrenderer);
                            CursorPosition = Cursor.MoveDown(CursorPosition, TotalLines.Count);
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
                            CursorPosition = selectionrenderer.SelectionEndPosition = Cursor.MoveUp(selectionrenderer.SelectionEndPosition);
                            selectionrenderer.IsSelecting = false;
                        }
                        else
                        {
                            Selection.ClearSelectionIfNeeded(this, selectionrenderer);
                            CursorPosition = Cursor.MoveUp(CursorPosition);
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

                            Cursor.MoveToLineEnd(CursorPosition, CurrentLine);
                            selectionrenderer.SelectionEndPosition = CursorPosition;
                            canvasHelper.UpdateSelection();
                            canvasHelper.UpdateCursor();
                        }
                        else
                        {
                            Cursor.MoveToLineEnd(CursorPosition, CurrentLine);
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
                            Cursor.MoveToLineStart(CursorPosition);
                            selectionrenderer.SelectionEndPosition = CursorPosition;

                            canvasHelper.UpdateSelection();
                            canvasHelper.UpdateCursor();
                        }
                        else
                        {
                            Cursor.MoveToLineStart(CursorPosition);
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
                Selection.SelectSingleWord(canvasHelper, selectionrenderer, CursorPosition, CurrentLine);
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
        //Scrolling
        private void VerticalScrollbar_Loaded(object sender, RoutedEventArgs e)
        {
            VerticalScrollbar.Maximum = ((TotalLines.Count + 1) * SingleLineHeight - Scroll.ActualHeight) / DefaultVerticalScrollSensitivity;
            VerticalScrollbar.ViewportSize = this.ActualHeight;
        }
        private void HorizontalScrollbar_Scroll(object sender, ScrollEventArgs e)
        {
            canvasHelper.UpdateAll();
        }
        private void VerticalScrollbar_Scroll(object sender, ScrollEventArgs e)
        {
            //only update when a line was scrolled
            if ((int)(VerticalScrollbar.Value / SingleLineHeight * DefaultVerticalScrollSensitivity) != NumberOfStartLine)
            {
                canvasHelper.UpdateAll();
            }
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
            VerticalScrollbar.Maximum = ((TotalLines.Count + 1) * SingleLineHeight - Scroll.ActualHeight) / DefaultVerticalScrollSensitivity;
            VerticalScrollbar.ViewportSize = sender.ActualHeight;

            //Calculate number of lines that needs to be rendered
            NumberOfRenderedLines = (int)(sender.ActualHeight / SingleLineHeight);
            NumberOfStartLine = (int)((VerticalScrollbar.Value * DefaultVerticalScrollSensitivity) / SingleLineHeight);

            //Get all the lines, that need to be rendered, from the list
            NumberOfRenderedLines = NumberOfRenderedLines + NumberOfStartLine > TotalLines.Count ? TotalLines.Count : NumberOfRenderedLines;

            RenderedLines = TotalLines.GetLines(NumberOfStartLine, NumberOfRenderedLines);
            RenderedText = RenderedLines.GetString("\n");

            if (_ShowLineNumbers)
            {
                for (int i = 0; i < NumberOfRenderedLines; i++)
                {
                    LineNumberContent.AppendLine((i + 1 + NumberOfStartLine).ToString());
                }
                LineNumberTextToRender = LineNumberContent.ToString();
                LineNumberContent.Clear();
            }

            //Get the longest line in the text:
            if (NeedsRecalculateLongestLineIndex)
            {
                NeedsRecalculateLongestLineIndex = false;
                LongestLineIndex = Utils.GetLongestLineIndex(TotalLines);
            }

            string longestLineText = TotalLines.GetLineText(LongestLineIndex);
            LongestLineLength = longestLineText.Length;
            Size lineLength = Utils.MeasureLineLenght(CanvasDevice.GetSharedDevice(), longestLineText, TextFormat);

            //Measure horizontal Width of longest line and apply to scrollbar
            HorizontalScrollbar.Maximum = (lineLength.Width <= sender.ActualWidth ? 0 : lineLength.Width - sender.ActualWidth + 50);
            HorizontalScrollbar.ViewportSize = sender.ActualWidth;
            ScrollIntoViewHorizontal();

            //Create the textlayout --> apply the Syntaxhighlighting --> render it:

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
                SearchHighlightsRenderer.RenderHighlights(args, DrawnTextLayout, RenderedText, searchHelper.MatchingSearchLines, searchHelper.SearchParameter.SearchExpression, (float)-HorizontalScroll, SingleLineHeight / DefaultVerticalScrollSensitivity, _Design.SearchHighlightColor);

            args.DrawingSession.DrawTextLayout(DrawnTextLayout, (float)-HorizontalScroll, SingleLineHeight, TextColorBrush);

            //Only update when old text != new text, to reduce updates when scrolling
            if (OldLineNumberTextToRender == null || LineNumberTextToRender == null || !OldLineNumberTextToRender.Equals(LineNumberTextToRender, StringComparison.OrdinalIgnoreCase))
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
                TextSelection = selectionrenderer.DrawSelection(DrawnTextLayout, RenderedLines, args, (float)-HorizontalScroll, SingleLineHeight / DefaultVerticalScrollSensitivity, NumberOfStartLine, NumberOfRenderedLines, ZoomedFontSize, _Design.SelectionColor);
            }

            if (TextSelection != null && !Selection.Equals(OldTextSelection, TextSelection))
            {
                //Update the variables
                OldTextSelection = new TextSelection(TextSelection);
                Internal_CursorChanged();
            }
        }
        private void Canvas_Cursor_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            UpdateCurrentLine();
            if (DrawnTextLayout == null || !HasFocus)
                return;

            int currentLineLength = CurrentLine.Length;
            if (CursorPosition.LineNumber >= TotalLines.Count)
            {
                CursorPosition.LineNumber = TotalLines.Count - 1;
                CursorPosition.CharacterPosition = currentLineLength;
            }

            //Calculate the distance to the top for the cursorposition and render the cursor
            float renderPosY = (float)((CursorPosition.LineNumber - NumberOfStartLine) * SingleLineHeight) + SingleLineHeight / DefaultVerticalScrollSensitivity;

            //Out of display-region:
            if (renderPosY > NumberOfRenderedLines * SingleLineHeight || renderPosY < 0)
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


            if (_ShowLineHighlighter && Selection.SelectionIsNull(selectionrenderer, TextSelection))
                LineHighlighterRenderer.Render((float)sender.ActualWidth, CurrentLineTextLayout, renderPosY, ZoomedFontSize, args, LineHighlighterBrush);

            if (!Cursor.Equals(CursorPosition, OldCursorPosition))
            {
                OldCursorPosition = new CursorPosition(CursorPosition);
                Internal_CursorChanged();
            }
        }
        private void Canvas_LineNumber_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (_ShowLineNumbers)
            {
                if (LineNumberTextToRender == null || LineNumberTextToRender.Length == 0)
                    return;

                //Calculate the linenumbers             
                float lineNumberWidth = (float)Utils.MeasureTextSize(CanvasDevice.GetSharedDevice(), (TotalLines.Count).ToString(), LineNumberTextFormat).Width;
                Canvas_LineNumber.Width = lineNumberWidth + 10 + SpaceBetweenLineNumberAndText;
            }
            else
            {
                Canvas_LineNumber.Width = SpaceBetweenLineNumberAndText;
                LineNumberTextToRender = null;
                return;
            }

            float posX = (float)sender.Size.Width - SpaceBetweenLineNumberAndText;
            if (posX < 0)
                posX = 0;

            OldLineNumberTextToRender = LineNumberTextToRender;
            LineNumberTextLayout = TextLayoutHelper.CreateTextLayout(sender, LineNumberTextFormat, LineNumberTextToRender, posX, (float)sender.Size.Height);
            args.DrawingSession.DrawTextLayout(LineNumberTextLayout, 10, SingleLineHeight, LineNumberColorBrush);
        }
        //Internal events:
        private void Internal_TextChanged()
        {
            //update the possible lines if the search is open
            if (searchHelper.IsSearchOpen)
                searchHelper.UpdateSearchLines(TotalLines);

            TextChanged?.Invoke(this);
        }
        private void Internal_CursorChanged()
        {
            SelectionChangedEventHandler args = new SelectionChangedEventHandler
            {
                CharacterPositionInLine = GetCurPosInLine() + 1,
                LineNumber = CursorPosition.LineNumber,
            };
            if (selectionrenderer.SelectionStartPosition != null && selectionrenderer.SelectionEndPosition != null)
            {
                var sel = Selection.GetIndexOfSelection(TotalLines, new TextSelection(selectionrenderer.SelectionStartPosition, selectionrenderer.SelectionEndPosition));
                args.SelectionLength = sel.Length;
                args.SelectionStartIndex = sel.Index;
            }
            else
            {
                args.SelectionLength = 0;
                args.SelectionStartIndex = Cursor.CursorPositionToIndex(TotalLines, new CursorPosition { CharacterPosition = GetCurPosInLine(), LineNumber = CursorPosition.LineNumber });
            }
            SelectionChanged?.Invoke(this, args);
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
                AddCharacter(stringManager.CleanUpString(await e.DataView.GetTextAsync()), true);
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
            selectionrenderer.SetSelection(new CursorPosition(0, line), new CursorPosition(TotalLines.GetLineLength(line), line));
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
            if (line >= TotalLines.Count || line < 0)
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
            Safe_LoadText(text);
        }

        /// <summary>
        /// Sets the text content of the textbox, recording an undo action.
        /// </summary>
        /// <param name="text">The new text content to set in the textbox.</param>
        public void SetText(string text)
        {
            Safe_SetText(text);
        }

        /// <summary>
        /// Loads the specified lines into the textbox, resetting all content and undo history.
        /// </summary>
        /// <param name="lines">An enumerable containing the lines to load into the textbox.</param>
        /// <param name="lineEnding">The line ending format used in the loaded lines (default is CRLF).</param>
        public void LoadLines(IEnumerable<string> lines, LineEnding lineEnding = LineEnding.CRLF)
        {
            Safe_LoadLines(lines, lineEnding);
        }

        /// <summary>
        /// Pastes the contents of the clipboard at the current cursor position.
        /// </summary>
        public void Paste()
        {
            Safe_Paste();
        }

        /// <summary>
        /// Copies the currently selected text to the clipboard.
        /// </summary>
        public void Copy()
        {
            Safe_Copy();
        }

        /// <summary>
        /// Cuts the currently selected text and copies it to the clipboard.
        /// </summary>
        public void Cut()
        {
            Safe_Cut();
        }

        /// <summary>
        /// Gets the entire text content of the textbox.
        /// </summary>
        /// <returns>The complete text content of the textbox as a string.</returns>

        public string GetText()
        {
            return Safe_Gettext();
        }

        /// <summary>
        /// Sets the text selection in the textbox starting from the specified index and with the given length.
        /// </summary>
        /// <param name="start">The index of the first character of the selection.</param>
        /// <param name="length">The length of the selection in number of characters.</param>
        public void SetSelection(int start, int length)
        {
            var result = Selection.GetSelectionFromPosition(TotalLines, start, length, CharacterCount);
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
            //No selection can be shown
            if (TotalLines.Count == 1 && TotalLines[0].Length == 0)
                return;

            selectionrenderer.SetSelection(new CursorPosition(0, 0), new CursorPosition(TotalLines.GetLineLength(-1), TotalLines.Count - 1));
            CursorPosition = selectionrenderer.SelectionEndPosition;
            canvasHelper.UpdateSelection();
            canvasHelper.UpdateCursor();
        }

        /// <summary>
        /// Clears the current text selection in the textbox.
        /// </summary>
        public void ClearSelection()
        {
            ForceClearSelection();
        }

        /// <summary>
        /// Undoes the last action in the textbox.
        /// </summary>
        public void Undo()
        {
            if (IsReadonly || !undoRedo.CanUndo)
                return;

            //Do the Undo
            ChangeCursor(InputSystemCursorShape.Wait);
            var sel = undoRedo.Undo(TotalLines, stringManager, NewLineCharacter);
            Internal_TextChanged();
            ChangeCursor(InputSystemCursorShape.IBeam);

            NeedsRecalculateLongestLineIndex = true;

            if (sel != null)
            {
                //only set cursorposition
                if (sel.StartPosition != null && sel.EndPosition == null)
                {
                    CursorPosition = sel.StartPosition;
                    canvasHelper.UpdateAll();
                    return;
                }

                selectionrenderer.SetSelection(sel);
                CursorPosition = sel.EndPosition;
            }
            else
                ForceClearSelection();
            canvasHelper.UpdateAll();
        }

        /// <summary>
        /// Redoes the last undone action in the textbox.
        /// </summary>
        public void Redo()
        {
            if (IsReadonly || !undoRedo.CanRedo)
                return;

            //Do the Redo
            ChangeCursor(InputSystemCursorShape.Wait);
            var sel = undoRedo.Redo(TotalLines, stringManager, NewLineCharacter);
            Internal_TextChanged();
            ChangeCursor(InputSystemCursorShape.IBeam);

            NeedsRecalculateLongestLineIndex = true;

            if (sel != null)
            {
                //only set cursorposition
                if (sel.StartPosition != null && sel.EndPosition == null)
                {
                    CursorPosition = sel.StartPosition;
                    canvasHelper.UpdateAll();
                    return;
                }

                selectionrenderer.SetSelection(sel);
                CursorPosition = sel.EndPosition;
            }
            else
                ForceClearSelection();
            canvasHelper.UpdateAll();
        }

        /// <summary>
        /// Scrolls the specified line to the center of the textbox if it is out of the rendered region.
        /// </summary>
        /// <param name="line">The index of the line to center.</param>
        public void ScrollLineToCenter(int line)
        {
            if (OutOfRenderedArea(line))
                ScrollLineIntoView(line);
        }

        /// <summary>
        /// Scrolls the text one line up.
        /// </summary>
        public void ScrollOneLineUp()
        {
            VerticalScrollbar.Value -= SingleLineHeight / DefaultVerticalScrollSensitivity;
            canvasHelper.UpdateAll();
        }

        /// <summary>
        /// Scrolls the text one line down.
        /// </summary>
        public void ScrollOneLineDown()
        {
            VerticalScrollbar.Value += SingleLineHeight / DefaultVerticalScrollSensitivity;
            canvasHelper.UpdateAll();
        }

        /// <summary>
        /// Forces the specified line to be scrolled into view, centering it vertically within the textbox.
        /// </summary>
        /// <param name="line">The index of the line to center.</param>
        public void ScrollLineIntoView(int line)
        {
            VerticalScrollbar.Value = (line - NumberOfRenderedLines / 2) * SingleLineHeight / DefaultVerticalScrollSensitivity;
            canvasHelper.UpdateAll();
        }

        /// <summary>
        /// Scrolls the first line of the visible text into view.
        /// </summary>
        public void ScrollTopIntoView()
        {
            VerticalScrollbar.Value = (CursorPosition.LineNumber - 1) * SingleLineHeight / DefaultVerticalScrollSensitivity;
            canvasHelper.UpdateAll();
        }

        /// <summary>
        /// Scrolls the last visible line of the visible text into view.
        /// </summary>
        public void ScrollBottomIntoView()
        {
            VerticalScrollbar.Value = (CursorPosition.LineNumber - NumberOfRenderedLines + 1) * SingleLineHeight / DefaultVerticalScrollSensitivity;
            canvasHelper.UpdateAll();
        }

        /// <summary>
        /// Scrolls one page up, simulating the behavior of the page up key.
        /// </summary>
        public void ScrollPageUp()
        {
            CursorPosition.LineNumber -= NumberOfRenderedLines;
            if (CursorPosition.LineNumber < 0)
                CursorPosition.LineNumber = 0;

            VerticalScrollbar.Value -= NumberOfRenderedLines * SingleLineHeight / DefaultVerticalScrollSensitivity;
            canvasHelper.UpdateAll();
        }

        /// <summary>
        /// Scrolls one page down, simulating the behavior of the page down key.
        /// </summary>
        public void ScrollPageDown()
        {
            CursorPosition.LineNumber += NumberOfRenderedLines;
            if (CursorPosition.LineNumber > TotalLines.Count - 1)
                CursorPosition.LineNumber = TotalLines.Count - 1;
            VerticalScrollbar.Value += NumberOfRenderedLines * SingleLineHeight / DefaultVerticalScrollSensitivity;
            canvasHelper.UpdateAll();
        }

        /// <summary>
        /// Gets the content of the line specified by the index
        /// </summary>
        /// <param name="line">The index to get the content from</param>
        /// <returns>The text from the line specified by the index</returns>
        public string GetLineText(int line)
        {
            return TotalLines.GetLineText(line);
        }

        /// <summary>
        /// Gets the text of multiple lines, starting from the specified line index.
        /// </summary>
        /// <param name="startLine">The index of the line to start with.</param>
        /// <param name="length">The number of lines to retrieve.</param>
        /// <returns>The concatenated text from the specified lines.</returns>
        public string GetLinesText(int startLine, int length)
        {
            if (startLine + length >= TotalLines.Count)
                return TotalLines.GetString(NewLineCharacter);

            return TotalLines.GetLines(startLine, length).GetString(NewLineCharacter);
        }

        /// <summary>
        /// Sets the content of the line specified by the index. The first line has the index 0.
        /// </summary>
        /// <param name="line">The index of the line to change the content.</param>
        /// <param name="text">The text to set for the specified line.</param>
        /// <returns>Returns true if the text was changed successfully, and false if the index was out of range.</returns>
        public bool SetLineText(int line, string text)
        {
            if (line >= TotalLines.Count || line < 0)
                return false;

            if (text.Length > LongestLineLength)
                LongestLineIndex = line;

            undoRedo.RecordUndoAction(() =>
            {
                TotalLines.SetLineText(line, stringManager.CleanUpString(text));
            }, TotalLines, line, 1, 1, NewLineCharacter);
            canvasHelper.UpdateText();
            return true;
        }

        /// <summary>
        /// Deletes the line from the textbox
        /// </summary>
        /// <param name="line">The line to delete</param>
        /// <returns>Returns true if the line was deleted successfully and false if not</returns>
        public bool DeleteLine(int line)
        {
            if (line >= TotalLines.Count || line < 0)
                return false;

            if (line == LongestLineIndex)
                NeedsRecalculateLongestLineIndex = true;

            undoRedo.RecordUndoAction(() =>
            {
                TotalLines.RemoveAt(line);
            }, TotalLines, line, 2, 1, NewLineCharacter);

            if (TotalLines.Count == 0)
            {
                TotalLines.AddLine();
            }

            canvasHelper.UpdateText();
            return true;
        }

        /// <summary>
        /// Adds a new line with the text specified
        /// </summary>
        /// <param name="line">The position to insert the line to</param>
        /// <param name="text">The text to put in the new line</param>
        /// <returns>Returns true if the line was added successfully and false if not</returns>
        public bool AddLine(int line, string text)
        {
            if (line > TotalLines.Count || line < 0)
                return false;

            if (text.Length > LongestLineLength)
                LongestLineIndex = line;

            undoRedo.RecordUndoAction(() =>
            {
                TotalLines.InsertOrAdd(line, stringManager.CleanUpString(text));

            }, TotalLines, line, 1, 2, NewLineCharacter);

            canvasHelper.UpdateText();
            return true;
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
            if (!Selection.SelectionIsNull(selectionrenderer, TextSelection))
            {
                AddCharacter(stringManager.CleanUpString(text1) + SelectedText + stringManager.CleanUpString(text2));
            }
        }

        /// <summary>
        /// Duplicates the line specified by the index into the next line
        /// </summary>
        /// <param name="line">The index of the line to duplicate</param>
        public void DuplicateLine(int line)
        {
            undoRedo.RecordUndoAction(() =>
            {
                TotalLines.InsertOrAdd(line, TotalLines.GetLineText(line));
                CursorPosition.LineNumber += 1;
            }, TotalLines, line, 1, 2, NewLineCharacter);

            if (OutOfRenderedArea(line))
                ScrollBottomIntoView();

            canvasHelper.UpdateText();
            canvasHelper.UpdateCursor();
        }
        /// <summary>
        /// Duplicates the line at the current cursor position
        /// </summary>
        public void DuplicateLine()
        {
            DuplicateLine(CursorPosition.LineNumber);
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
                for (int i = 0; i < TotalLines.Count; i++)
                {
                    if (TotalLines[i].Contains(searchParameter))
                    {
                        isFound = true;
                        SetLineText(i, Regex.Replace(TotalLines[i], searchParameter.SearchExpression, replaceWord));
                    }
                }
            }, TotalLines, 0, TotalLines.Count, TotalLines.Count, NewLineCharacter);
            canvasHelper.UpdateText();
            return isFound ? SearchResult.Found : SearchResult.NotFound;
        }

        /// <summary>
        /// Searches for the next occurence. Call this after BeginSearch
        /// </summary>
        /// <returns>SearchResult.Found if the word was found</returns>
        public SearchResult FindNext()
        {
            if (!searchHelper.IsSearchOpen)
                return SearchResult.SearchNotOpened;

            var res = searchHelper.FindNext(TotalLines, CursorPosition);
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
            if (!searchHelper.IsSearchOpen)
                return SearchResult.SearchNotOpened;

            var res = searchHelper.FindPrevious(TotalLines, CursorPosition);
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
            var res = searchHelper.BeginSearch(TotalLines, word, wholeWord, matchCase);
            canvasHelper.UpdateText();
            return res;
        }

        /// <summary>
        /// Ends the search and removes the highlights
        /// </summary>
        public void EndSearch()
        {
            searchHelper.EndSearch();
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
            TotalLines.Dispose();
            RenderedLines = null;
            LineNumberTextToRender = RenderedText = null;
            LineNumberContent = null;
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
                Y = (float)((CursorPosition.LineNumber - NumberOfStartLine) * SingleLineHeight) + SingleLineHeight / DefaultVerticalScrollSensitivity,
                X = CursorRenderer.GetCursorPositionInLine(CurrentLineTextLayout, CursorPosition, 0)
            };
        }

        /// <summary>
        /// Selects the CodeLanguage based on the specified identifier.
        /// </summary>
        /// <param name="languageId">The identifier of the CodeLanguage to select.</param>
        public void SelectCodeLanguage(CodeLanguageId languageId)
        {
            if (CodeLanguages.TryGetValue(languageId, out CodeLanguage codelanguage))
                CodeLanguage = codelanguage;
        }

        /// <summary>
        /// Gets or sets a value indicating whether syntax highlighting is enabled in the textbox.
        /// </summary>
        public bool SyntaxHighlighting { get; set; } = true;

        /// <summary>
        /// Gets or sets the code language to use for the syntaxhighlighting and autopairing.
        /// </summary>
        public CodeLanguage CodeLanguage
        {
            get => _CodeLanguage;
            set
            {
                _CodeLanguage = value;
                NeedsUpdateTextLayout = true; //set to true to force update the textlayout
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
            get => _LineEnding;
            set
            {
                NewLineCharacter = LineEndings.LineEndingToString(value);
                stringManager.lineEnding = value;
                _LineEnding = value;
            }
        }

        /// <summary>
        /// Gets or sets the space between the line number and the text in the textbox.
        /// </summary>
        public float SpaceBetweenLineNumberAndText { get => _SpaceBetweenLineNumberAndText; set { _SpaceBetweenLineNumberAndText = value; NeedsUpdateLineNumbers(); canvasHelper.UpdateAll(); } }

        /// <summary>
        /// Gets or sets the current cursor position in the textbox.
        /// </summary>
        /// <remarks>
        /// The cursor position is represented by a <see cref="CursorPosition"/> object, which includes the character position within the text and the line number.
        /// </remarks>
        public CursorPosition CursorPosition
        {
            get => _CursorPosition;
            set { _CursorPosition = new CursorPosition(value.CharacterPosition, value.LineNumber); canvasHelper.UpdateCursor(); }
        }

        /// <summary>
        /// Gets or sets the font family used for displaying text in the textbox.
        /// </summary>
        public new FontFamily FontFamily { get => _FontFamily; set { _FontFamily = value; NeedsTextFormatUpdate = true; canvasHelper.UpdateAll(); } }

        /// <summary>
        /// Gets or sets the font size used for displaying text in the textbox.
        /// </summary>
        public new int FontSize { get => _FontSize; set { _FontSize = value; UpdateZoom(); } }

        /// <summary>
        /// Gets the actual rendered size of the font in pixels.
        /// </summary>
        public float RenderedFontSize => ZoomedFontSize;

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
            get => _ShowLineNumbers;
            set
            {
                _ShowLineNumbers = value;
                NeedsUpdateTextLayout = true;
                NeedsUpdateLineNumbers();
                canvasHelper.UpdateAll();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the line highlighter should be shown in the custom textbox.
        /// </summary>
        public bool ShowLineHighlighter
        {
            get => _ShowLineHighlighter;
            set { _ShowLineHighlighter = value; canvasHelper.UpdateCursor(); }
        }

        /// <summary>
        /// Gets or sets the zoom factor in percent for the text.
        /// </summary>
        public int ZoomFactor { get => _ZoomFactor; set { _ZoomFactor = value; UpdateZoom(); } } //%

        /// <summary>
        /// Gets or sets a value indicating whether the textbox is in readonly mode.
        /// </summary>
        public bool IsReadonly { get; set; } //TODO

        /// <summary>
        /// Gets or sets the size of the cursor in the textbox.
        /// </summary>
        public CursorSize CursorSize { get => _CursorSize; set { _CursorSize = value; canvasHelper.UpdateCursor(); } }

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
                if (TextSelection != null && Selection.WholeTextSelected(TextSelection, TotalLines))
                    return GetText();
                return Selection.GetSelectedText(TotalLines, TextSelection, CursorPosition.LineNumber, NewLineCharacter);
            }
            set => AddCharacter(stringManager.CleanUpString(value));
        }

        /// <summary>
        /// Gets the number of lines in the textbox.
        /// </summary>
        public int NumberOfLines { get => TotalLines.Count; }

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
        public int CharacterCount => Utils.CountCharacters(TotalLines);

        /// <summary>
        /// Gets or sets the sensitivity of vertical scrolling in the textbox.
        /// </summary>
        public double VerticalScrollSensitivity { get => _VerticalScrollSensitivity; set => _VerticalScrollSensitivity = value < 1 ? 1 : value; }

        /// <summary>
        /// Gets or sets the sensitivity of horizontal scrolling in the textbox.
        /// </summary>
        public double HorizontalScrollSensitivity { get => _HorizontalScrollSensitivity; set => _HorizontalScrollSensitivity = value < 1 ? 1 : value; }

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
        public bool UseSpacesInsteadTabs { get => tabSpaceHelper.UseSpacesInsteadTabs; set { tabSpaceHelper.UseSpacesInsteadTabs = value; tabSpaceHelper.UpdateTabs(TotalLines); canvasHelper.UpdateAll(); } }

        /// <summary>
        /// Gets or sets the number of spaces used for a single tab in the textbox.
        /// </summary>
        public int NumberOfSpacesForTab { get => tabSpaceHelper.NumberOfSpaces; set { tabSpaceHelper.NumberOfSpaces = value; tabSpaceHelper.UpdateNumberOfSpaces(TotalLines); canvasHelper.UpdateAll(); } }

        /// <summary>
        /// Gets whether the search is currently active
        /// </summary>
        public bool SearchIsOpen => searchHelper.IsSearchOpen;

        /// <summary>
        /// Gets an enumerable collection of all the lines in the textbox.
        /// </summary>
        /// <remarks>
        /// Use this property to access all the lines of text in the textbox. You can use this collection to save the lines to a file using functions like FileIO.WriteLinesAsync.
        /// Utilizing this property for saving will significantly improve RAM usage during the saving process.
        /// </remarks>
        public IEnumerable<string> Lines => TotalLines;

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
        public static Dictionary<CodeLanguageId, CodeLanguage> CodeLanguages => new Dictionary<CodeLanguageId, CodeLanguage>()
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
        public static CodeLanguage GetCodeLanguageFromId(CodeLanguageId languageId)
        {
            if (CodeLanguages.TryGetValue(languageId, out CodeLanguage codelanguage))
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