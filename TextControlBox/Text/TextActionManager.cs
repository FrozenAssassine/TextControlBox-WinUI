using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextControlBoxNS.Extensions;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Renderer;
using Windows.ApplicationModel.DataTransfer;

namespace TextControlBoxNS.Text
{
    internal class TextActionManager
    {
        private CanvasUpdateManager canvasUpdateHelper;
        private TextManager textManager;
        private SelectionRenderer selectionRenderer;
        private SelectionManager selectionManager;
        private CursorManager cursorManager;
        private CoreTextControlBox coreTextbox;
        private UndoRedo undoRedo;
        private LongestLineManager longestLineManager;
        private CurrentLineManager currentLineManager;
        private ScrollManager scrollManager;
        private EventsManager eventsManager;
        private TextRenderer textRenderer;
        private StringManager stringManager;

        public void Init(
            CoreTextControlBox coreTextbox,
            TextRenderer textRenderer,
            UndoRedo undoRedo,
            CurrentLineManager currentLineManager,
            LongestLineManager longestLineManager,
            CanvasUpdateManager canvasUpdateHelper,
            TextManager textManager,
            SelectionRenderer selectionRenderer,
            CursorManager cursorManager,
            ScrollManager scrollManager,
            EventsManager eventsManager,
            StringManager stringManager,
            SelectionManager selectionManager)
        {
            this.canvasUpdateHelper = canvasUpdateHelper;
            this.textManager = textManager;
            this.selectionRenderer = selectionRenderer;
            this.cursorManager = cursorManager;
            this.coreTextbox = coreTextbox;
            this.undoRedo = undoRedo;
            this.longestLineManager = longestLineManager;
            this.currentLineManager = currentLineManager;
            this.scrollManager = scrollManager;
            this.eventsManager = eventsManager;
            this.textRenderer = textRenderer;
            this.stringManager = stringManager;
            this.selectionManager = selectionManager;
        }

        public void SelectAll()
        {
            //No selection can be shown
            if (textManager.LinesCount== 1 && textManager.GetLineLength(0) == 0)
                return;

            selectionRenderer.SetSelection(new CursorPosition(0, 0), new CursorPosition(textManager.GetLineLength(-1), textManager.LinesCount- 1));
            cursorManager.SetCursorPosition(new CursorPosition(selectionRenderer.SelectionEndPosition));
            canvasUpdateHelper.UpdateSelection();
            canvasUpdateHelper.UpdateCursor();
        }
        public void Undo()
        {
            if (coreTextbox.IsReadonly || !undoRedo.CanUndo)
                return;

            //Do the Undo
            coreTextbox.ChangeCursor(InputSystemCursorShape.Wait);
            var sel = undoRedo.Undo(stringManager);
            eventsManager.CallTextChanged();
            coreTextbox.ChangeCursor(InputSystemCursorShape.IBeam);

            longestLineManager.needsRecalculation = true;

            if (sel != null)
            {
                //only set cursorposition
                if (sel.StartPosition != null && sel.EndPosition == null)
                {
                    cursorManager.SetCursorPosition(sel.StartPosition);
                    canvasUpdateHelper.UpdateAll();
                    return;
                }

                selectionRenderer.SetSelection(sel);
                cursorManager.SetCursorPosition(sel.EndPosition);
            }
            else
                selectionManager.ForceClearSelection(canvasUpdateHelper);
            canvasUpdateHelper.UpdateAll();
        }
        public void Redo()
        {
            if (coreTextbox.IsReadonly || !undoRedo.CanRedo)
                return;

            //Do the Redo
            coreTextbox.ChangeCursor(InputSystemCursorShape.Wait);
            var sel = undoRedo.Redo(stringManager);
            eventsManager.CallTextChanged();
            coreTextbox.ChangeCursor(InputSystemCursorShape.IBeam);

            longestLineManager.needsRecalculation = true;

            if (sel != null)
            {
                //only set cursorposition
                if (sel.StartPosition != null && sel.EndPosition == null)
                {
                    cursorManager.SetCursorPosition(new CursorPosition(sel.StartPosition));
                    canvasUpdateHelper.UpdateAll();
                    return;
                }

                selectionRenderer.SetSelection(sel);
                cursorManager.SetCursorPosition(new CursorPosition(sel.EndPosition));
            }
            else
                selectionManager.ForceClearSelection(canvasUpdateHelper);
            canvasUpdateHelper.UpdateAll();
        }

        //Trys running the code and clears the memory if OutOfMemoryException gets thrown
        public async void Safe_Paste(bool handleException = true)
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
                    textManager.CleanUp();
                    Safe_Paste(false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
        public string Safe_Gettext(bool handleException = true)
        {
            try
            {
                return textManager.totalLines.GetString(textManager.NewLineCharacter);
            }
            catch (OutOfMemoryException)
            {
                if (handleException)
                {
                    textManager.CleanUp();
                    return Safe_Gettext(false);
                }
                throw new OutOfMemoryException();
            }
        }
        public void Safe_Cut(bool handleException = true)
        {
            try
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(coreTextbox.SelectedText);
                if (selectionManager.currentTextSelection == null)
                    DeleteLine(cursorManager.LineNumber); //Delete the line
                else
                    DeleteText(); //Delete the selected text

                dataPackage.RequestedOperation = DataPackageOperation.Move;
                Clipboard.SetContent(dataPackage);
                selectionManager.ForceClearSelection(canvasUpdateHelper);
            }
            catch (OutOfMemoryException)
            {
                if (handleException)
                {
                    textManager.CleanUp();
                    Safe_Cut(false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
        public void Safe_Copy(bool handleException = true)
        {
            try
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(coreTextbox.SelectedText);
                dataPackage.RequestedOperation = DataPackageOperation.Copy;
                Clipboard.SetContent(dataPackage);
            }
            catch (OutOfMemoryException)
            {
                if (handleException)
                {
                    textManager.CleanUp();
                    Safe_Copy(false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
        public void Safe_LoadLines(IEnumerable<string> lines, LineEnding lineEnding = LineEnding.CRLF, bool HandleException = true)
        {
            try
            {
                selectionRenderer.ClearSelection();
                undoRedo.ClearAll();

                textManager.ClearText();
                textManager.totalLines.AddRange(lines);

                textManager._LineEnding = lineEnding;

                longestLineManager.needsRecalculation = true;
                canvasUpdateHelper.UpdateAll();
            }
            catch (OutOfMemoryException)
            {
                if (HandleException)
                {
                    textManager.CleanUp();
                    Safe_LoadLines(lines, lineEnding, false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
        public async void Safe_LoadText(string text, bool handleException = true)
        {
            try
            {
                if (await Utils.IsOverTextLimit(text.Length))
                    return;

                //Get the LineEnding
                textManager._LineEnding = LineEndings.FindLineEnding(text);

                selectionRenderer.ClearSelection();
                undoRedo.ClearAll();

                longestLineManager.needsRecalculation = true;

                if (text.Length == 0)
                    textManager.ClearText(true);
                else
                    selectionManager.ReplaceLines(0, textManager.LinesCount, stringManager.CleanUpString(text).Split(textManager.NewLineCharacter));

                canvasUpdateHelper.UpdateAll();
            }
            catch (OutOfMemoryException)
            {
                if (handleException)
                {
                    textManager.CleanUp();
                    Safe_LoadText(text, false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
        public async void Safe_SetText(string text, bool handleException = true)
        {
            try
            {
                if (await Utils.IsOverTextLimit(text.Length))
                    return;

                selectionRenderer.ClearSelection();
                longestLineManager.needsRecalculation = true;
                undoRedo.RecordUndoAction(() =>
                {
                    selectionManager.ReplaceLines(0, textManager.LinesCount, stringManager.CleanUpString(text).Split(textManager.NewLineCharacter));
                    if (text.Length == 0) //Create a new line when the text gets cleared
                    {
                        textManager.AddLine();
                    }
                }, 0, textManager.LinesCount, Utils.CountLines(text, textManager.NewLineCharacter));

                canvasUpdateHelper.UpdateAll();
            }
            catch (OutOfMemoryException)
            {
                if (handleException)
                {
                    textManager.CleanUp();
                    Safe_SetText(text, false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }

        public void DeleteSelection()
        {
            if (selectionManager.currentTextSelection == null)
                return;

            //line gets deleted -> recalculate the longest line:
            longestLineManager.CheckSelection();

            undoRedo.RecordUndoAction(() =>
            {
                cursorManager.SetCursorPosition(selectionManager.Remove(selectionManager.currentTextSelection));
                selectionManager.ForceClearSelection(canvasUpdateHelper);

            }, selectionManager.currentTextSelection, 0);

            canvasUpdateHelper.UpdateSelection();
            canvasUpdateHelper.UpdateCursor();
        }
        public void AddCharacter(string text, bool ignoreSelection = false)
        {
            if (textManager._IsReadonly)
                return;

            if (ignoreSelection)
                selectionManager.ForceClearSelection(canvasUpdateHelper);

            int splittedTextLength = text.Contains(textManager.NewLineCharacter, StringComparison.Ordinal) ? Utils.CountLines(text, textManager.NewLineCharacter) : 1;

            if (selectionManager.currentTextSelection== null && splittedTextLength == 1)
            {
                var res = AutoPairing.AutoPair(coreTextbox, text);
                text = res.text;

                undoRedo.RecordUndoAction(() =>
                {
                    var characterPos = cursorManager.GetCurPosInLine();

                    if (characterPos > currentLineManager.CurrentLineLength() - 1)
                        currentLineManager.AddToEnd(text);
                    else
                        currentLineManager.AddText(text, characterPos);
                    cursorManager.CharacterPosition= res.length + characterPos;

                }, cursorManager.LineNumber, 1, 1);

                if (currentLineManager.GetCurrentLineText().Length > longestLineManager.longestLineLength)
                {
                    longestLineManager.longestIndex = cursorManager.LineNumber;
                }
            }
            else if (selectionManager.currentTextSelection == null && splittedTextLength > 1)
            {
                longestLineManager.CheckRecalculateLongestLine(text);
                undoRedo.RecordUndoAction(() =>
                {
                    cursorManager.SetCursorPosition(selectionManager.InsertText(selectionManager.currentTextSelection, cursorManager.currentCursorPosition, text));
                }, cursorManager.LineNumber, 1, splittedTextLength);
            }
            else if (text.Length == 0) //delete selection
            {
                DeleteSelection();
            }
            else if (selectionManager.currentTextSelection != null)
            {
                text = AutoPairing.AutoPairSelection(coreTextbox, text);
                if (text == null)
                    return;

                longestLineManager.CheckRecalculateLongestLine(text);
                undoRedo.RecordUndoAction(() =>
                {
                    cursorManager.SetCursorPosition(selectionManager.Replace(selectionManager.currentTextSelection, text));

                    selectionManager.ForceClearSelection(canvasUpdateHelper);
                }, selectionManager.currentTextSelection, splittedTextLength);
            }

            scrollManager.ScrollLineToCenter(cursorManager.LineNumber);
            canvasUpdateHelper.UpdateText();
            canvasUpdateHelper.UpdateCursor();
            eventsManager.CallTextChanged();
        }
        public void RemoveText(bool controlIsPressed = false)
        {
            currentLineManager.UpdateCurrentLine(cursorManager.LineNumber);

            if (textManager._IsReadonly)
                return;

            if (selectionManager.currentTextSelection != null)
                DeleteSelection();
            else
            {
                string curLine = currentLineManager.CurrentLine;
                var charPos = cursorManager.GetCurPosInLine();
                var stepsToMove = controlIsPressed ? cursorManager.CalculateStepsToMoveLeft(charPos) : 1;

                if (charPos - stepsToMove >= 0)
                {
                    if (cursorManager.LineNumber == longestLineManager.longestIndex)
                        longestLineManager.needsRecalculation = true;

                    undoRedo.RecordUndoAction(() =>
                    {
                        currentLineManager.CurrentLine.SafeRemove(charPos - stepsToMove, stepsToMove);
                        cursorManager.CharacterPosition -= stepsToMove;

                    }, cursorManager.LineNumber, 1, 1);
                }
                else if (charPos - stepsToMove < 0) //remove lines
                {
                    if (cursorManager.LineNumber <= 0)
                        return;

                    if (cursorManager.LineNumber == longestLineManager.longestIndex)
                        longestLineManager.needsRecalculation = true;

                    undoRedo.RecordUndoAction(() =>
                    {
                        int curpos = textManager.GetLineLength(cursorManager.LineNumber - 1);

                        //line still has text:
                        if (curLine.Length > 0)
                            textManager.totalLines.String_AddToEnd(cursorManager.LineNumber - 1, curLine);

                        textManager.DeleteAt(cursorManager.LineNumber);

                        //update the cursorposition
                        cursorManager.LineNumber -= 1;
                        cursorManager.CharacterPosition = curpos;

                    }, cursorManager.LineNumber - 1, 3, 2);
                }
            }

            scrollManager.UpdateScrollToShowCursor();
            canvasUpdateHelper.UpdateText();
            canvasUpdateHelper.UpdateCursor();

            eventsManager.CallTextChanged();
        }
        public void DeleteText(bool controlIsPressed = false, bool shiftIsPressed = false)
        
        {
            currentLineManager.UpdateCurrentLine(cursorManager.LineNumber);

            if (textManager._IsReadonly)
                return;

            //Shift + delete:
            if (shiftIsPressed && selectionManager.currentTextSelection == null)
                coreTextbox.DeleteLine(cursorManager.LineNumber);
            else if (selectionManager.currentTextSelection != null)
                DeleteSelection();
            else
            {
                int characterPos = cursorManager.GetCurPosInLine();
                //delete lines if cursor is at position 0 and the line is emty OR the cursor is at the end of a line and the line has content
                if (characterPos == currentLineManager.Length)
                {
                    string lineToAdd = cursorManager.LineNumber + 1 < textManager.LinesCount ? textManager.totalLines.GetLineText(cursorManager.LineNumber + 1) : null;
                    if (lineToAdd != null)
                    {
                        if (cursorManager.LineNumber == longestLineManager.longestIndex)
                            longestLineManager.needsRecalculation = true;

                        undoRedo.RecordUndoAction(() =>
                        {
                            int curpos = textManager.GetLineLength(cursorManager.LineNumber);
                            currentLineManager.CurrentLine += lineToAdd;
                            textManager.DeleteAt(cursorManager.LineNumber + 1);

                            //update the cursorposition
                            cursorManager.CharacterPosition = curpos;

                        }, cursorManager.LineNumber, 2, 1);
                    }
                }
                //delete text in line
                else if (textManager.LinesCount > cursorManager.LineNumber)
                {
                    int stepsToMove = controlIsPressed ? cursorManager.CalculateStepsToMoveRight(characterPos) : 1;

                    if (cursorManager.LineNumber == longestLineManager.longestIndex)
                        longestLineManager.needsRecalculation = true;

                    undoRedo.RecordUndoAction(() =>
                    {
                        currentLineManager.SafeRemove(characterPos, stepsToMove);
                    }, cursorManager.LineNumber, 1, 1);
                }
            }

            scrollManager.UpdateScrollToShowCursor();
            canvasUpdateHelper.UpdateText();
            canvasUpdateHelper.UpdateCursor();

            eventsManager.CallTextChanged();
        }
        public void AddNewLine()
        {
            if (textManager._IsReadonly)
                return;

            if (textManager.LinesCount == 0)
            {
                textManager.AddLine();
                return;
            }

            CursorPosition startLinePos = new CursorPosition(selectionManager.TextSelIsNull ? CursorPosition.ChangeLineNumber(cursorManager.currentCursorPosition, cursorManager.LineNumber) : selectionManager.GetMin(selectionManager.currentTextSelection));

            //If the whole text is selected
            if (selectionManager.WholeTextSelected(selectionManager.currentTextSelection))
            {
                undoRedo.RecordUndoAction(() =>
                {
                    textManager.ClearText(true);
                    textManager.totalLines.InsertNewLine(-1);
                    cursorManager.SetCursorPosition(new CursorPosition(0, 1));
                }, 0, textManager.LinesCount, 2);
                selectionManager.ForceClearSelection(canvasUpdateHelper);
                canvasUpdateHelper.UpdateAll();
                eventsManager.CallTextChanged();
                return;
            }

            if (selectionManager.TextSelIsNull) //No selection
            {
                string startLine = textManager.totalLines.GetLineText(startLinePos.LineNumber);

                undoRedo.RecordUndoAction(() =>
                {
                    string[] splittedLine = Utils.SplitAt(textManager.totalLines.GetLineText(startLinePos.LineNumber), startLinePos.CharacterPosition);

                    textManager.SetLineText(startLinePos.LineNumber, splittedLine[1]);
                    textManager.InsertOrAdd(startLinePos.LineNumber, splittedLine[0]);

                }, startLinePos.LineNumber, 1, 2);
            }
            else //Any kind of selection
            {
                int remove = 2;
                if (selectionManager.currentTextSelection.StartPosition.LineNumber == selectionManager.currentTextSelection.EndPosition.LineNumber)
                {
                    //line is selected completely: remove = 1
                    if (selectionManager.GetMax(selectionManager.currentTextSelection.StartPosition, selectionManager.currentTextSelection.EndPosition).CharacterPosition == textManager.GetLineLength(cursorManager.LineNumber) &&
                        selectionManager.GetMin(selectionManager.currentTextSelection.StartPosition, selectionManager.currentTextSelection.EndPosition).CharacterPosition == 0)
                    {
                        remove = 1;
                    }
                }

                undoRedo.RecordUndoAction(() =>
                {
                    cursorManager.SetCursorPosition(selectionManager.Replace(selectionManager.currentTextSelection, textManager.NewLineCharacter));
                }, selectionManager.currentTextSelection, remove);
            }

            selectionManager.ForceClearSelection(canvasUpdateHelper);
            cursorManager.LineNumber += 1;
            cursorManager.CharacterPosition = 0;

            if (selectionManager.currentTextSelection == null && cursorManager.LineNumber == textRenderer.NumberOfRenderedLines + textRenderer.NumberOfStartLine)
                scrollManager.ScrollOneLineDown();
            else
                scrollManager.UpdateScrollToShowCursor();

            canvasUpdateHelper.UpdateAll();
            eventsManager.CallTextChanged();
        }

        public bool DeleteLine(int line)
        {
            if (line >= textManager.LinesCount || line < 0)
                return false;

            if (line == longestLineManager.longestIndex)
                longestLineManager.needsRecalculation = true;

            undoRedo.RecordUndoAction(() =>
            {
                textManager.totalLines.RemoveAt(line);
            }, line, 2, 1);

            if (textManager.LinesCount == 0)
            {
                textManager.AddLine();
            }

            canvasUpdateHelper.UpdateText();
            return true;
        }

        public bool AddLine(int line, string text)
        {
            if (line > textManager.LinesCount || line < 0)
                return false;

            if (text.Length > longestLineManager.longestLineLength)
                longestLineManager.longestIndex = line;

            undoRedo.RecordUndoAction(() =>
            {
                textManager.InsertOrAdd(line, stringManager.CleanUpString(text));

            }, line, 1, 2);

            canvasUpdateHelper.UpdateText();
            return true;
        }

        public bool SetLineText(int line, string text)
        {
            if (line >= textManager.LinesCount || line < 0)
                return false;

            if (text.Length > longestLineManager.longestLineLength)
                longestLineManager.longestIndex = line;

            undoRedo.RecordUndoAction(() =>
            {
                textManager.SetLineText(line, stringManager.CleanUpString(text));
            }, line, 1, 1);
            canvasUpdateHelper.UpdateText();
            return true;
        }

        public void DuplicateLine(int line)
        {
            undoRedo.RecordUndoAction(() =>
            {
                textManager.InsertOrAdd(line, textManager.GetLineText(line));
                cursorManager.LineNumber += 1;
            }, line, 1, 2);

            if (textRenderer.OutOfRenderedArea(line))
                scrollManager.ScrollBottomIntoView();

            canvasUpdateHelper.UpdateText();
            canvasUpdateHelper.UpdateCursor();
        }
    }
}
