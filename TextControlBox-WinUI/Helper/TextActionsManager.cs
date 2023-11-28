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

        public void AddText(string text, bool ignoreSelection = false)
        {
            //if (textBox.IsReadonly)
            //return;

            if(ignoreSelection)
                selectionManager.ClearSelection();

            int splittedTextLength = text.Contains(textBoxProps.NewLineCharacter, StringComparison.Ordinal) ? Utils.CountLines(text, textBoxProps.NewLineCharacter) : 1;

            if (selectionManager.Selection == null && splittedTextLength == 1)
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
            else if (selectionManager.Selection == null && splittedTextLength > 1)
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
        private void RemoveText(bool controlIsPressed = false)
        {
            currentWorkingLine.Update(textManager, selectionManager.Cursor);

            //if (textBox.IsReadonly)
                //return;

            if (selectionManager.Selection != null)
                DeleteSelection();
            else
            {
                var charPos = CursorHelper.GetCurrentPosInLine(currentWorkingLine, selectionManager.Cursor);
                var stepsToMove = controlIsPressed ? Cursor.CalculateStepsToMoveLeft(currentWorkingLine.Text, charPos) : 1;

                if (charPos - stepsToMove >= 0)
                {
                    if (longestLineCalculationHelper.EqualsIndex(selectionManager.Cursor.LineNumber))
                        longestLineCalculationHelper.NeedsRecalculate();

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
        private void DeleteSelection()
        {
            if (selectionManager.Selection == null)
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
    }
}
