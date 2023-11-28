using System;
using TextControlBox.Extensions;
using TextControlBox.Helper;
using TextControlBox.Text;
using TextControlBox_WinUI.Models;

namespace TextControlBox_WinUI.Helper
{
    internal class TextActionsManager
    {
        private TextControlBox.TextControlBox textBox;
        private TextManager textManager;
        private TextControlBoxProperties textBoxProps;
        private UndoRedoManager undoRedo;
        private CurrentWorkingLine currentWorkingLine;
        private SelectionManager selectionManager;
        private LongestLineCalculationHelper longestLineCalculationHelper;
        private CanvasUpdateHelper canvasUpdater;

        public TextActionsManager(TextControlBox.TextControlBox textBox, 
            LongestLineCalculationHelper longestLineCalculationHelper, 
            UndoRedoManager undoRedo,
            SelectionManager selectionManager, 
            CurrentWorkingLine currentWorkingLine, 
            CanvasUpdateHelper canvasUpdater, 
            TextManager textManager,
            TextControlBoxProperties textBoxProps)
        {
            this.textBox = textBox;
            this.textManager = textManager;
            this.textBoxProps = textBoxProps;
            this.undoRedo = undoRedo;
            this.selectionManager = selectionManager;
            this.longestLineCalculationHelper = longestLineCalculationHelper; 
            this.currentWorkingLine = currentWorkingLine;
            this.canvasUpdater = canvasUpdater;
        }

        public void DeleteText(bool controlIsPressed = false, bool shiftIsPressed = false)
        {
            currentWorkingLine.Update(textManager, selectionManager.Cursor);

            if (textBox.IsReadonly)
                return;

            //Shift + delete:
            if (shiftIsPressed && selectionManager.HasSelection)
                textBox.DeleteLine(selectionManager.Cursor.LineNumber);
            else if (selectionManager.Selection != null)
                DeleteSelection();
            else
            {
                int characterPos = CursorHelper.GetCurrentPosInLine(currentWorkingLine, selectionManager.Cursor);
                //delete lines if cursor is at position 0 and the line is emty OR the cursor is at the end of a line and the line has content
                if (characterPos == currentWorkingLine.Text.Length)
                {
                    string lineToAdd = selectionManager.Cursor.LineNumber + 1 < textManager.Lines.Count ? textManager.Lines.GetLineText(selectionManager.Cursor.LineNumber + 1) : null;
                    if (lineToAdd != null)
                    {
                        longestLineCalculationHelper.NeedsRecalculate(selectionManager.Cursor.LineNumber == longestLineCalculationHelper.LongestLineIndex);

                        undoRedo.RecordUndoAction(() =>
                        {
                            int curpos = textManager.Lines.GetLineLength(selectionManager.Cursor.LineNumber);
                            currentWorkingLine.Text += lineToAdd;
                            textManager.Lines.DeleteAt(selectionManager.Cursor.LineNumber + 1);

                            //update the cursorposition
                            selectionManager.Cursor.CharacterPosition = curpos;

                        }, selectionManager.Cursor.LineNumber, 2, 1);
                    }
                }
                //delete text in line
                else if (textManager.Lines.Count > selectionManager.Cursor.LineNumber)
                {
                    int stepsToMove = controlIsPressed ? Cursor.CalculateStepsToMoveRight(currentWorkingLine.Text, characterPos) : 1;

                    longestLineCalculationHelper.NeedsRecalculate(selectionManager.Cursor.LineNumber == longestLineCalculationHelper.LongestLineIndex);

                    undoRedo.RecordUndoAction(() =>
                    {
                        currentWorkingLine.Text = currentWorkingLine.Text.SafeRemove(characterPos, stepsToMove);
                    }, selectionManager.Cursor.LineNumber, 1, 1);
                }
            }

            //UpdateScrollToShowCursor();
            canvasUpdater.UpdateText().UpdateCursor();
            //Internal_TextChanged();
        }
        public void AddText(string text, bool ignoreSelection = false)
        {
            if (textBox.IsReadonly)
                return;

            if(ignoreSelection)
                selectionManager.ClearSelection();

            int splittedTextLength = text.Contains(textBoxProps.NewLineCharacter, StringComparison.Ordinal) ? Utils.CountLines(text, textBoxProps.NewLineCharacter) : 1;

            if (selectionManager.HasSelection && splittedTextLength == 1)
            {
                //var res = AutoPairingHelper.AutoPair(this, text);
                //text = res.text;

                undoRedo.RecordUndoAction(() =>
                {
                    var characterPos = CursorHelper.GetCurrentPosInLine(currentWorkingLine, selectionManager.Cursor);

                    if (characterPos > currentWorkingLine.Text.Length - 1)
                        currentWorkingLine.Text = currentWorkingLine.Text.AddToEnd(text);
                    else
                        currentWorkingLine.Text = currentWorkingLine.Text.AddText(text, characterPos);
                    selectionManager.Cursor.CharacterPosition = text.Length + characterPos;

                }, selectionManager.Cursor.LineNumber, 1, 1);

                if (currentWorkingLine.Text.Length > longestLineCalculationHelper.LongestLineLength)
                {
                    longestLineCalculationHelper.ChangeIndex(selectionManager.Cursor.LineNumber);
                }
            }
            else if (selectionManager.HasSelection && splittedTextLength > 1)
            {
                longestLineCalculationHelper.CheckNeedsRecalculate(text);
                undoRedo.RecordUndoAction(() =>
                {
                    selectionManager.Cursor = Selection.InsertText(textManager, textBoxProps, selectionManager.Selection, selectionManager.Cursor, text);
                }, selectionManager.Cursor.LineNumber, 1, splittedTextLength);
            }
            else if (text.Length == 0) //delete selection
            {
                DeleteSelection();
            }
            else if (selectionManager.Selection != null)
            {
                //text = AutoPairing.AutoPairSelection(this, text);
                if (text == null)
                    return;

                longestLineCalculationHelper.CheckNeedsRecalculate(text);
                undoRedo.RecordUndoAction(() =>
                {
                    selectionManager.Cursor = Selection.Replace(textManager, textBoxProps, selectionManager.Selection, text);

                    selectionManager.ClearSelection();
                    canvasUpdater.UpdateSelection();
                }, selectionManager.Selection, splittedTextLength);

            }

            //ScrollLineToCenter(cursorPos.LineNumber);
            canvasUpdater.UpdateText().UpdateCursor();
        }
        public void RemoveText(bool controlIsPressed = false)
        {
            currentWorkingLine.Update(textManager, selectionManager.Cursor);

            if (textBox.IsReadonly)
                return;

            if (selectionManager.HasSelection)
                DeleteSelection();
            else
            {
                var charPos = CursorHelper.GetCurrentPosInLine(currentWorkingLine, selectionManager.Cursor);
                var stepsToMove = controlIsPressed ? Cursor.CalculateStepsToMoveLeft(currentWorkingLine.Text, charPos) : 1;

                if (charPos - stepsToMove >= 0)
                {
                    longestLineCalculationHelper.NeedsRecalculate(longestLineCalculationHelper.EqualsIndex(selectionManager.Cursor.LineNumber));

                    undoRedo.RecordUndoAction(() =>
                    {
                        currentWorkingLine.Text = currentWorkingLine.Text.SafeRemove(charPos - stepsToMove, stepsToMove);
                        selectionManager.Cursor.CharacterPosition -= stepsToMove;

                    }, selectionManager.Cursor.LineNumber, 1, 1);
                }
                else if (charPos - stepsToMove < 0) //remove lines
                {
                    if (selectionManager.Cursor.LineNumber <= 0)
                        return;

                    if (longestLineCalculationHelper.EqualsIndex(selectionManager.Cursor.LineNumber))
                        longestLineCalculationHelper.NeedsRecalculate();

                    undoRedo.RecordUndoAction(() =>
                    {
                        int curpos = textManager.Lines.GetLineLength(selectionManager.Cursor.LineNumber - 1);

                        //line still has text:
                        if (currentWorkingLine.Text.Length > 0)
                            textManager.Lines.String_AddToEnd(selectionManager.Cursor.LineNumber - 1, currentWorkingLine.Text);

                        textManager.Lines.DeleteAt(selectionManager.Cursor.LineNumber);

                        //update the cursorposition
                        selectionManager.Cursor.LineNumber -= 1;
                        selectionManager.Cursor.CharacterPosition = curpos;

                    }, selectionManager.Cursor.LineNumber - 1, 3, 2);
                }
            }

            //UpdateScrollToShowCursor();
            canvasUpdater.UpdateText().UpdateCursor();
        }
        public void DeleteSelection()
        {
            if (selectionManager.HasSelection)
                return;

            //line gets deleted -> recalculate the longest line:
            if (selectionManager.Selection.IsLineInSelection(longestLineCalculationHelper.LongestLineIndex))
                longestLineCalculationHelper.NeedsRecalculate();

            undoRedo.RecordUndoAction(() =>
            {
                selectionManager.ClearSelection();
                selectionManager.Cursor = Selection.Remove(textManager, selectionManager.Selection);
            }, selectionManager.Selection, 0);

            canvasUpdater.UpdateSelection().UpdateCursor();
        }
        public void AddNewLine()
        {
            if (textBox.IsReadonly)
                return;

            if (textManager.Lines.Count == 0)
            {
                textManager.Lines.AddLine();
                return;
            }

            CursorPosition startLinePos = new CursorPosition(selectionManager.HasSelection ? CursorPosition.ChangeLineNumber(selectionManager.Cursor, selectionManager.Cursor.LineNumber) : Selection.GetMin(selectionManager.Selection));

            //If the whole text is selected
            if (Selection.WholeTextSelected(textManager, selectionManager.Selection))
            {
                undoRedo.RecordUndoAction(() =>
                {
                    ListHelper.Clear(textManager.Lines, true);
                    textManager.Lines.InsertNewLine(-1);
                    selectionManager.Cursor = new CursorPosition(0, 1);
                }, 0, textManager.Lines.Count, 2);
                selectionManager.ClearSelection();
                canvasUpdater.UpdateAll();
                //Internal_TextChanged();
                return;
            }

            if (selectionManager.HasSelection) //No selection
            {
                string startLine = textManager.Lines.GetLineText(startLinePos.LineNumber);

                undoRedo.RecordUndoAction(() =>
                {
                    string[] splittedLine = Utils.SplitAt(textManager.Lines.GetLineText(startLinePos.LineNumber), startLinePos.CharacterPosition);

                    textManager.Lines.SetLineText(startLinePos.LineNumber, splittedLine[1]);
                    textManager.Lines.InsertOrAdd(startLinePos.LineNumber, splittedLine[0]);

                }, startLinePos.LineNumber, 1, 2);
            }
            else //Any kind of selection
            {
                int removeCount = 2;
                if (selectionManager.Selection.StartPosition.LineNumber == selectionManager.Selection.EndPosition.LineNumber)
                {
                    //line is selected completely: remove = 1
                    if (Selection.GetMax(selectionManager.Selection.StartPosition, selectionManager.Selection.EndPosition).CharacterPosition == textManager.Lines.GetLineLength(selectionManager.Cursor.LineNumber) &&
                        Selection.GetMin(selectionManager.Selection.StartPosition, selectionManager.Selection.EndPosition).CharacterPosition == 0)
                    {
                        removeCount = 1;
                    }
                }

                undoRedo.RecordUndoAction(() =>
                {
                    selectionManager.Cursor = Selection.Replace(textManager, textBoxProps, selectionManager.Selection, textBoxProps.NewLineCharacter);
                }, selectionManager.Selection, removeCount);
            }

            selectionManager.ClearSelection();
            selectionManager.Cursor.LineNumber += 1;
            selectionManager.Cursor.CharacterPosition = 0;

            //if (selectionManager.HasSelection && selectionManager.Cursor.LineNumber == NumberOfRenderedLines + NumberOfStartLine)
            //    ScrollOneLineDown();
            //else
            //    UpdateScrollToShowCursor();

            canvasUpdater.UpdateAll();
            //Internal_TextChanged();
        }
    }
}
